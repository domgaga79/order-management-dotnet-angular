$ErrorActionPreference = "SilentlyContinue"

function Stop-Port([int]$Port) {
    $connections = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
    foreach ($connection in $connections) {
        Stop-Process -Id $connection.OwningProcess -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "Encerrando Order Management..." -ForegroundColor Cyan
Stop-Port 4200
Stop-Port 4201
Stop-Port 5294
Write-Host "Frontend e backend encerrados." -ForegroundColor Green
