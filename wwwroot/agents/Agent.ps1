Write-Host "[INFO] Initializing NetSentinel Agent deployment..." -ForegroundColor Cyan


# Prepara o ambiente no C:\ e verificaa se aja existe 
$installDir = "C:\NetSentinel"
$agentPath = "$installDir\agent.ps1"

if (-not (Test-Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
}

#Agent code 
$agentCode = @'
$registryPaths = @(
    "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*",
    "HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*"
)

$installedApps = Get-ItemProperty $registryPaths -ErrorAction SilentlyContinue |
    Where-Object { $_.DisplayName -ne $null -and $_.DisplayName -ne "" } |
    Select-Object @{Name="Name"; Expression={$_.DisplayName}},
                  @{Name="Version"; Expression={$_.DisplayVersion}},
                  @{Name="Publisher"; Expression={$_.Publisher}} |
    Sort-Object Name -Unique

$payload = @{
    hostname = $env:COMPUTERNAME
    windowsUsername = $env:USERNAME
    ipv4Address = (Get-NetIPAddress -AddressFamily IPv4 -InterfaceAlias Wi-Fi, Ethernet -ErrorAction SilentlyContinue).IPAddress | Select-Object -First 1
    ipv6Address = (Get-NetIPAddress -AddressFamily IPv6 -InterfaceAlias Wi-Fi, Ethernet -ErrorAction SilentlyContinue).IPAddress | Select-Object -First 1
    macAddress = (Get-NetAdapter -ErrorAction SilentlyContinue | Where-Object Status -eq 'Up' | Select-Object -First 1).MacAddress
    operatingSystem = (Get-CimInstance Win32_OperatingSystem).Caption
    installedApplications = $installedApps
}

$jsonPayload = $payload | ConvertTo-Json -Depth 4

# Configurações de conexão com a API NetSentinel
$apiUrl = "http://192.168.5.81:5149/api/manager/device/register"

$apiKey = "sentinelAgentRegisterDevice01020304050607080910" 

$headers = @{
    "Content-Type"  = "application/json"
    "X-Api-Key"     = $apiKey 
}

try {
    # Disparo silencioso para a API enviando o JSON
    Invoke-RestMethod -Uri $apiUrl -Method Post -Headers $headers -Body $jsonPayload | Out-Null
}
catch {
    
}
'@

# Escreve no C:
Set-Content -Path $agentPath -Value $agentCode -Encoding UTF8
Write-Host "[OK] Agent payload successfully injected into system ($agentPath)" -ForegroundColor Yellow



#Registra tarefa agendada e deleta anteriores
Unregister-ScheduledTask -TaskName "NetSentinelHybridAgent" -Confirm:$false -ErrorAction SilentlyContinue

$taskName = "NetSentinelHybridAgent"
$xml = @"
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
    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
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

# Injeta o XML no Task Scheduler do Windows
Register-ScheduledTask -Xml $xml -TaskName $taskName -Force | Out-Null

Write-Host "[OK] Scheduled Task registered successfully!" -ForegroundColor Yellow
Write-Host "`n[SUCCESS] NetSentinel Agent deployed! MSI Trigger + 1-Minute Polling active." -ForegroundColor Green