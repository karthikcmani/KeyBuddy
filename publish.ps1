# Publish KeyBuddy as a self-contained single-file Windows x64 executable
Write-Host "Publishing KeyBuddy as self-contained Windows x64 executable..." -ForegroundColor Cyan

$dotnet = "C:\Users\Asus\AppData\Local\Microsoft\dotnet\dotnet.exe"
if (-not (Test-Path $dotnet)) {
    $dotnet = "dotnet"
}

Set-Location $PSScriptRoot

$publishDir = Join-Path $PSScriptRoot "publish"
Get-Process KeyBuddy -ErrorAction SilentlyContinue | Where-Object { $_.Path -like "*KeyBuddy-App\publish*" } | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 300
if (Test-Path $publishDir) {
    Remove-Item -Path $publishDir -Recurse -Force
}

& $dotnet publish src/KeyBuddy/KeyBuddy.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $publishDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publish failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "`nPublish Succeeded!" -ForegroundColor Green
Write-Host "Executable location: $publishDir\KeyBuddy.exe" -ForegroundColor Yellow
