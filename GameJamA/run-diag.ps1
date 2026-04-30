$unityExe  = "C:\Program Files\Unity\Hub\Editor\6000.4.3f1\Editor\Unity.exe"
$projectPath = $PSScriptRoot
$logFile   = Join-Path $projectPath "Logs\diag-leak.log"

Write-Host "Launching Unity with temp-memory-leak validation..."
Write-Host "Log -> $logFile"

& $unityExe `
    -projectPath $projectPath `
    -diag-temp-memory-leak-validation `
    -logFile $logFile

Write-Host "Unity exited. Check $logFile for leak callstacks."
