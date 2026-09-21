$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

function Stop-Port([int]$Port) {
    $connections = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
    foreach ($connection in $connections) {
        try { Stop-Process -Id $connection.OwningProcess -Force -ErrorAction SilentlyContinue } catch {}
    }
}

Write-Host "== Order Management: ambiente de desenvolvimento ==" -ForegroundColor Cyan
Write-Host "Encerrando processos antigos nas portas 5294 e 4200..."
Stop-Port 5294
Stop-Port 4200

Write-Host "Garantindo PostgreSQL..."
Set-Location $root
docker compose up -d

$backendCommand = "Set-Location '$root'; dotnet run --project backend\OrderManagement.Api"
$frontendPath = Join-Path $root "frontend\order-management-web"
$frontendCommand = "Set-Location '$frontendPath'; npm start"

Write-Host "Abrindo backend..."
Start-Process powershell -ArgumentList "-NoExit", "-ExecutionPolicy", "Bypass", "-Command", $backendCommand
Start-Sleep -Seconds 3

Write-Host "Abrindo frontend..."
Start-Process powershell -ArgumentList "-NoExit", "-ExecutionPolicy", "Bypass", "-Command", $frontendCommand
Start-Sleep -Seconds 4

Write-Host "Frontend: http://localhost:4200" -ForegroundColor Green
Write-Host "Swagger : http://localhost:5294/swagger" -ForegroundColor Green
Start-Process "http://localhost:4200"
