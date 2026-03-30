<#
.SYNOPSIS
    [Brief] Dropper e Bootstrap do Agente NetSentinel.
    Prepara o ambiente, injeta o payload de monitoramento no disco e garante 
    a persistência através do Agendador de Tarefas do Windows.

.DESCRIPTION
    [Details] Arquitetura e Fluxo de Execução:
    
    1. Preparação de Ambiente (Bootstrap): 
       Cria o diretório blindado (C:\NetSentinel) que servirá de base para 
       os binários e logs do agente.
       
    2. Injeção de Payload (agent.ps1): 
       Escreve fisicamente o script de monitoramento no disco, contendo:
       - Compatibilidade Legada: Conversão JSON nativa para Windows 7 (PS 2.0).
       - Resolução de Identidade Avançada: Captura de dados do usuário real via 
         Win32_ComputerSystem, contornando a limitação do contexto SYSTEM.
       - Telemetria Híbrida: Coleta via WMI (Hardware/Rede) e Registry (Softwares), 
         focando exclusivamente em instalações reais (MSI/EXE).
       - Transmissão Segura: Envio de payload JSON via System.Net.WebClient 
         com autenticação via header (X-Api-Key).

    3. Persistência (Task Scheduler): 
       Registra a tarefa invisível 'NetSentinelHybridAgent' com privilégios 
       máximos (SYSTEM / S-1-5-18) baseada em Disparo Duplo (Hybrid Trigger):
       - Polling: Execução intervalada a cada 1 minuto.
       - Event-Driven: Execução imediata baseada em EventIDs do MsiInstaller 
         (Instalação/Remoção de softwares).

.NOTES
    Architecture: Hybrid (C# API + PowerShell Agent)
#>

Write-Host "[INFO] Initializing NetSentinel Agent deployment..." -ForegroundColor Cyan

# 1. Environment Preparation
$installDir = "C:\NetSentinel"
$agentPath = "$installDir\agent.ps1"

if (-not (Test-Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
}

# 2. Agent Payload Injection
$agentCode = @'
# --- DEPENDENCIES ---
function Get-NetSentinelJson($obj) {
    if ($PSVersionTable.PSVersion.Major -ge 3) {
        return $obj | ConvertTo-Json -Depth 4
    }
    
    $appsJson = ($obj.installedApplications | ForEach-Object {
        '{"Name":"' + $_.Name + '","Version":"' + $_.Version + '","Publisher":"' + $_.Publisher + '"}'
    }) -join ","
    
    return @"
{
    "hostname": "$($obj.hostname)",
    "windowsUsername": "$($obj.windowsUsername)",
    "userFullName": "$($obj.userFullName)",  
    "userEmail": "$($obj.userEmail)",        
    "ipv4Address": "$($obj.ipv4Address)",
    "macAddress": "$($obj.macAddress)",
    "operatingSystem": "$($obj.operatingSystem)",
    "installedApplications": [$appsJson]
}
"@
}

# --- IDENTITY RESOLUTION ---
$userEmail = "Not_Identified"
$userFullName = "Not_Identified"
$realUsername = "Not_Identified"

try {
    $consoleUser = (Get-WmiObject -Class Win32_ComputerSystem).UserName
    if ($consoleUser) {
        $realUsername = $consoleUser.Split('\')[-1]
    } else {
        $realUsername = "No_User_Logged"
    }
} catch {}

if ($realUsername -ne "No_User_Logged" -and $realUsername -ne "Not_Identified") {
    try {
        $userFullName = (Get-WmiObject Win32_UserAccount -Filter "Name='$realUsername'").FullName
        if (-not $userFullName) { $userFullName = "Not_Identified" }
    } catch {}
}

try {
    $upn = whoami /upn 2>$null
    if ($upn -match "@") { $userEmail = $upn.Trim() }
} catch {}

if ($userEmail -eq "Not_Identified") {
    try {
        $regPath = "HKCU:\Software\Microsoft\Office\16.0\Common\Identity\Identities"
        $identities = Get-ChildItem -Path $regPath -ErrorAction SilentlyContinue
        foreach ($id in $identities) {
            $email = (Get-ItemProperty -Path $id.PSPath -Name "EmailAddress" -ErrorAction SilentlyContinue).EmailAddress
            if ($email -match "@") {
                $userEmail = $email
                break
            }
        }
    } catch {}
}

# --- SYSTEM TELEMETRY ---
$registryPaths = @(
    "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*",
    "HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*"
)

$installedApps = Get-ItemProperty $registryPaths -ErrorAction SilentlyContinue |
    Where-Object { $_.DisplayName -ne $null } |
    Select-Object @{Name="Name"; Expression={$_.DisplayName}},
                  @{Name="Version"; Expression={$_.DisplayVersion}},
                  @{Name="Publisher"; Expression={$_.Publisher}}

$network = Get-WmiObject Win32_NetworkAdapterConfiguration | Where-Object { $_.IPEnabled -eq $true } | Select-Object -First 1
$os = (Get-WmiObject Win32_OperatingSystem).Caption

# --- PAYLOAD ASSEMBLY ---
$payload = @{
    hostname = $env:COMPUTERNAME
    windowsUsername = $realUsername     
    userFullName = $userFullName        
    userEmail = $userEmail              
    ipv4Address = $network.IPAddress[0]
    macAddress = $network.MACAddress
    operatingSystem = $os
    installedApplications = $installedApps
}

$jsonPayload = Get-NetSentinelJson $payload
# Config API ip and public apikey register 
$apiUrl = "http://192.168.5.81:5149/api/manager/device/register"
$apiKey = "sentinelAgentRegisterDevice01020304050607080910"

# --- DATA TRANSMISSION ---
try {
    $webClient = New-Object System.Net.WebClient
    $webClient.Headers.Add("Content-Type", "application/json")
    $webClient.Headers.Add("X-Api-Key", $apiKey)
    $webClient.Encoding = [System.Text.Encoding]::UTF8
    $webClient.UploadString($apiUrl, "POST", $jsonPayload) | Out-Null
} catch {}
'@

[System.IO.File]::WriteAllText($agentPath, $agentCode)
Write-Host "[OK] Payload staged at $agentPath" -ForegroundColor Yellow

# 3. Task Scheduler Registration
$taskName = "NetSentinelHybridAgent"
$xmlPath = "$installDir\task.xml"

$xmlContent = @"
<?xml version="1.0" encoding="UTF-16"?>
<Task version="1.2" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
  <Triggers>
    <EventTrigger>
      <Enabled>true</Enabled>
      <Subscription>&lt;QueryList&gt;&lt;Query Id="0" Path="Application"&gt;&lt;Select Path="Application"&gt;*[System[Provider[@Name='MsiInstaller'] and (EventID=11707 or EventID=1033 or EventID=1042)]]&lt;/Select&gt;&lt;/Query&gt;&lt;/QueryList&gt;</Subscription>
    </EventTrigger>
    <TimeTrigger>
      <Repetition>
        <Interval>PT1M</Interval>
        <StopAtDurationEnd>false</StopAtDurationEnd>
      </Repetition>
      <StartBoundary>2024-01-01T00:00:00</StartBoundary>
      <Enabled>true</Enabled>
    </TimeTrigger>
  </Triggers>
  <Principals>
    <Principal id="Author">
      <UserId>S-1-5-18</UserId>
      <RunLevel>HighestAvailable</RunLevel>
    </Principal>
  </Principals>
  <Settings>
    <MultipleInstancesPolicy>Queue</MultipleInstancesPolicy>
    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
    <ExecutionTimeLimit>PT1H</ExecutionTimeLimit>
    <Hidden>true</Hidden>
  </Settings>
  <Actions Context="Author">
    <Exec>
      <Command>powershell.exe</Command>
      <Arguments>-WindowStyle Hidden -ExecutionPolicy Bypass -File "$agentPath"</Arguments>
    </Exec>
  </Actions>
</Task>
"@

[System.IO.File]::WriteAllText($xmlPath, $xmlContent)

schtasks /Delete /TN $taskName /F 2>$null | Out-Null
schtasks /Create /XML $xmlPath /TN $taskName /F | Out-Null

Write-Host "[SUCCESS] Agent deployed and monitoring services started." -ForegroundColor Green