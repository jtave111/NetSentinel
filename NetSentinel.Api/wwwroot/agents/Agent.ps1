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
       - Validação de Integridade: Geração de SHA-256 para binários detectados 
         visando identificar adulterações ou malwares.
       - Transmissão Segura: Envio de payload JSON via System.Net.WebClient 
         com autenticação via header (X-Api-Key).

    3. Persistência (Task Scheduler): 
       Registra a tarefa invisível 'NetSentinelHybridAgent' com privilégios 
       máximos (SYSTEM / S-1-5-18) baseada em Disparo Duplo (Hybrid Trigger):
       - Polling: Execução intervalada a cada 1 minuto.
       - Event-Driven: Execução imediata baseada em EventIDs do MsiInstaller 
         (Instalação/Remoção de softwares).

    O nosso Agente de coleta faz a extração de Hashes baseando-se em chaves de desinstalação padrão do Windows. Porém, 
    muitos softwares modernos e pacotes MSI não registram o caminho absoluto do binário (.exe), apontando apenas para 
    diretórios ou não preenchendo as chaves DisplayIcon e InstallLocation. O agente foi desenhado de forma resiliente: 
    se ele não tem certeza absoluta de qual é o binário principal, ele marca como N/A para evitar gerar um falso-positivo 
    de integridade.

.NOTES
    Architecture: Hybrid (C# API + PowerShell Agent)
    Integrity: SHA-256 Binaries Fingerprinting
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
# --- DEPENDENCIES: HASHING & JSON ---

# Função para cálculo de Hash compatível com .NET antigo (PS 2.0+)
function Get-NetSentinelHash($filePath) {
    if (-not $filePath -or -not (Test-Path $filePath)) { return "N/A" }
    
    # Limpa caminhos que contém aspas ou índices de ícone (ex: C:\app.exe,0)
    $cleanPath = $filePath.Replace('"', '').Split(',')[0].Trim()
    if (-not (Test-Path $cleanPath) -or (Test-Path $cleanPath -PathType Container)) { return "N/A" }

    try {
        $stream = [System.IO.File]::OpenRead($cleanPath)
        $sha256 = New-Object System.Security.Cryptography.SHA256Managed
        $hashBytes = $sha256.ComputeHash($stream)
        $stream.Close()
        return [BitConverter]::ToString($hashBytes).Replace("-", "").ToLower()
    } catch {
        return "Access_Denied"
    }
}

function Get-NetSentinelJson($obj) {
    if ($PSVersionTable.PSVersion.Major -ge 3) {
        return $obj | ConvertTo-Json -Depth 4
    }
    
    # Fallback para Windows 7 (PS 2.0) incluindo o novo campo Hash com o nome correto para a API
    $appsJson = ($obj.installedApplications | ForEach-Object {
        '{"Name":"' + $_.Name + '","Version":"' + $_.Version + '","Publisher":"' + $_.Publisher + '","hashApplication":"' + $_.hashApplication + '"}'
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

# --- SYSTEM TELEMETRY (Híbrida com Geração de Hash) ---
$registryPaths = @(
    "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*",
    "HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*"
)

$installedApps = Get-ItemProperty $registryPaths -ErrorAction SilentlyContinue |
    Where-Object { $_.DisplayName -ne $null } |
    ForEach-Object {
        # Tenta localizar o binário original para gerar o Hash (DisplayIcon ou InstallLocation)
        $possiblePath = $_.DisplayIcon
        if (-not $possiblePath) { $possiblePath = $_.InstallLocation }

        New-Object PSObject -Property @{
            Name            = $_.DisplayName
            Version         = $_.DisplayVersion
            Publisher       = $_.Publisher
            hashApplication = Get-NetSentinelHash($possiblePath) # <--- Nome corrigido aqui
        }
    }

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
$apiUrl = "http://192.168.5.80:5149/api/manager/device/register"
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
Write-Host "[OK] Payload staged at $agentPath (SHA-256 support enabled)" -ForegroundColor Yellow

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

# Comando para rodar na máquina alvo:
# Set-ExecutionPolicy Bypass -Scope Process -Force; IEX (New-Object Net.WebClient).DownloadString('http://192.168.110.65:5149/DeploySentinel.ps1')