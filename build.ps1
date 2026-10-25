# Build KeyBuddy
Write-Host "Building KeyBuddy (.NET 8 WPF)..." -ForegroundColor Cyan

$dotnet = "C:\Users\Asus\AppData\Local\Microsoft\dotnet\dotnet.exe"
if (-not (Test-Path $dotnet)) {
    $dotnet = "dotnet"
}

Set-Location $PSScriptRoot

& $dotnet build KeyBuddy.sln -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "Build Succeeded!" -ForegroundColor Green
