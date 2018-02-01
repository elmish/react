@echo off
cls

.paket\paket.exe restore
if errorlevel 1 (
  exit /b %errorlevel%
)
.paket/paket.exe generate-load-scripts -t fsx -f netstandard2.0 -g Main

packages\build\FAKE\tools\FAKE.exe build.fsx %*
