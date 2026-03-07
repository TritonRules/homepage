param(
  [string]$Configuration = "Release",
  [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$Project = "src/Inicio.IptvPlayer.Wpf/Inicio.IptvPlayer.Wpf.csproj"
$OutputDir = "publish/$Runtime"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
  throw "No se encontró .NET SDK. Instala .NET 8 SDK desde https://dotnet.microsoft.com/download"
}

Write-Host "[1/3] Restaurando paquetes..."
dotnet restore $Project

Write-Host "[2/3] Publicando aplicación WPF ($Runtime)..."
dotnet publish $Project `
  -c $Configuration `
  -r $Runtime `
  --self-contained true `
  -p:PublishSingleFile=false `
  -p:PublishTrimmed=false `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o $OutputDir

$exePath = Join-Path $OutputDir "Inicio.IptvPlayer.Wpf.exe"
if (-not (Test-Path $exePath)) {
  throw "No se encontró el ejecutable esperado: $exePath"
}

$launcherPath = Join-Path $OutputDir "INICIAR_IPTV_PLAYER.cmd"
@"
@echo off
cd /d %~dp0
start "" "Inicio.IptvPlayer.Wpf.exe"
"@ | Out-File -FilePath $launcherPath -Encoding ascii -Force

Write-Host "[3/3] Publicación lista en: $OutputDir"
Write-Host "Ejecuta: $launcherPath"
