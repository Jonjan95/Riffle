@echo off
cd /d "%~dp0"
if exist "Builds\Windows\Riffle.exe" (
  start "Riffle - Alder Creek" "Builds\Windows\Riffle.exe" -force-d3d11 -screen-fullscreen 0
) else (
  echo Open this folder in Unity 6000.1.4f1.
  echo Open Assets/Creek/Scenes/AlderCreek.unity and press Play.
  pause
)
