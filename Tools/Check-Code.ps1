param([string]$EditorData = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Data')
$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
Set-Location -LiteralPath $project
New-Item -ItemType Directory -Force -Path 'Logs\CodeCheck' | Out-Null
$mono = Join-Path $EditorData 'MonoBleedingEdge\bin\mono.exe'
$compiler = Join-Path $EditorData 'MonoBleedingEdge\lib\mono\4.5\csc.exe'
$references = @(Get-ChildItem -LiteralPath (Join-Path $EditorData 'Managed\UnityEngine') -Filter '*.dll' | ForEach-Object { '/r:"' + $_.FullName + '"' })
$references += '/r:"' + (Join-Path $EditorData 'NetStandard\ref\2.1.0\netstandard.dll') + '"'
$sources = @(Get-ChildItem -LiteralPath 'Assets\Creek' -Recurse -Filter '*.cs' | ForEach-Object { '"' + $_.FullName + '"' })
@('/nologo','/nostdlib','/target:library','/langversion:latest','/define:UNITY_EDITOR','/out:Logs/CodeCheck/Riffle.dll') + $references + $sources | Set-Content -LiteralPath 'Logs\CodeCheck\compile.rsp'
& $mono $compiler /noconfig '@Logs/CodeCheck/compile.rsp'
if ($LASTEXITCODE -ne 0) { throw 'Unity-reference C# compilation failed.' }
$testSources = @('Assets/Creek/Scripts/PanInput.cs','Assets/Creek/Scripts/PanSimulation.cs','Assets/Creek/Scripts/Progression.cs','Assets/Creek/Scripts/SoftAudioSamples.cs','Assets/Creek/Editor/SimulationChecks.cs','Tools/CheckEntry.cs')
@('/nologo','/nostdlib','/target:exe','/langversion:latest','/out:Logs/CodeCheck/SimulationChecks.exe') + $references + $testSources | Set-Content -LiteralPath 'Logs\CodeCheck\tests.rsp'
& $mono $compiler /noconfig '@Logs/CodeCheck/tests.rsp'
if ($LASTEXITCODE -ne 0) { throw 'Simulation check compilation failed.' }
$env:MONO_PATH = (Join-Path $EditorData 'Managed\UnityEngine')
& $mono 'Logs/CodeCheck/SimulationChecks.exe'
if ($LASTEXITCODE -ne 0) { throw 'Simulation checks failed.' }
