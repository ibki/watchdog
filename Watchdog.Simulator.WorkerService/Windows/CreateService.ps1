$serviceUserAccount = "WatchdogSimulator"
$serviceName = "WatchdogSimulator"
$displayName = "WatchdogSimulator"
$exeFilePath = "$(Get-Location)\..\Watchdog.Simulator.WorkerService.exe"
$description = "Testing services..."

# Create a service (local)user account
# another:
# > powershell -Command "New-LocalUser -Name $serviceUserAccount"
New-LocalUser -Name $serviceUserAccount

# "Log on as a service" grant permissions - Local Security Policy
# downloads = https://gallery.technet.microsoft.com/scriptcenter/Grant-Log-on-as-a-service-11a50893
.".\Add Account To LogonAsService.ps1" $env:computername\$serviceUserAccount

# Create a service
$acl = Get-Acl (Get-Location)
$aclRuleArgs = $serviceUserAccount, "Read,Write,ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($aclRuleArgs)
$acl.SetAccessRule($accessRule)

New-Service -Name $serviceName -BinaryPathName $exeFilePath -Credential $env:computername\$serviceUserAccount -Description $description -DisplayName $displayName -StartupType Automatic

# Start a service
Start-Service -Name $serviceName

# Determine a service's status
# > Get-Service -Name {SERVICE NAME}

# Stop a service
# > Stop-Service -Name {SERVICE NAME}

# Remove a service
# > sc.exe delete {SERVICE NAME}
# another: 
# > $service = Get-WmiObject -Class Win32_Service -Filter "Name='{SERVICE NAME}'"
# > $service.delete()
# PowerShell 6.0:
# > Remove-Service -Name {SERVICE NAME}