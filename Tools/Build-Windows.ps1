param([string]$Unity = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe')
$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
Set-Location -LiteralPath $project
New-Item -ItemType Directory -Force -Path Logs | Out-Null
& $Unity -batchmode -nographics -projectPath $project -executeMethod RiffleCreek.Editor.BuildCreek.BuildCurrent -quit -logFile "$project\Logs\build.log" | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Unity build failed. See Logs/build.log.' }
Write-Host 'Ready: Builds/Windows/Riffle.exe'
