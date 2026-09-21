$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

Write-Host "== Order Management: finalizacao segura ==" -ForegroundColor Cyan

# Sempre execute a partir da raiz do repositorio.
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

Write-Host "[1/9] Encerrando instancia antiga da API, se existir..." -ForegroundColor Yellow
Get-Process -Name "OrderManagement.Api" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1

Write-Host "[2/9] Removendo teste placeholder do template..." -ForegroundColor Yellow
$placeholder = Join-Path $root "backend\OrderManagement.Tests\UnitTest1.cs"
if (Test-Path $placeholder) {
    Remove-Item $placeholder -Force
}

Write-Host "[3/9] Garantindo import global do xUnit..." -ForegroundColor Yellow
$usings = Join-Path $root "backend\OrderManagement.Tests\Usings.cs"
if (-not (Test-Path $usings)) {
    "global using Xunit;" | Set-Content -Path $usings -Encoding UTF8
}

Write-Host "[4/9] Garantindo dotnet-ef 8.0.31..." -ForegroundColor Yellow
try {
    dotnet tool update --global dotnet-ef --version 8.0.31 | Out-Host
} catch {
    dotnet tool install --global dotnet-ef --version 8.0.31 | Out-Host
}

Write-Host "[5/9] Subindo PostgreSQL..." -ForegroundColor Yellow
docker compose up -d

Write-Host "[6/9] Limpando bin/obj e restaurando pacotes..." -ForegroundColor Yellow
Get-ChildItem -Path (Join-Path $root "backend") -Directory -Recurse |
    Where-Object { $_.Name -in @("bin", "obj") } |
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

dotnet restore
if ($LASTEXITCODE -ne 0) { throw "dotnet restore falhou." }

Write-Host "[7/9] Compilando solucao..." -ForegroundColor Yellow
dotnet build --no-restore
if ($LASTEXITCODE -ne 0) { throw "dotnet build falhou. Corrija antes de gerar migrations." }

Write-Host "[8/9] Executando testes..." -ForegroundColor Yellow
dotnet test --no-build
if ($LASTEXITCODE -ne 0) { throw "dotnet test falhou. Corrija antes de gerar migrations." }

Write-Host "[9/9] Verificando/aplicando migrations..." -ForegroundColor Yellow
$migrationsDir = Join-Path $root "backend\OrderManagement.Infrastructure\Data\Migrations"
$hasCoreMigration = $false
if (Test-Path $migrationsDir) {
    $hasCoreMigration = [bool](Get-ChildItem $migrationsDir -Filter "*AddCoreModules*.cs" -ErrorAction SilentlyContinue | Where-Object { $_.Name -notlike "*.Designer.cs" } | Select-Object -First 1)
}

if (-not $hasCoreMigration) {
    Write-Host "Gerando migration AddCoreModules..." -ForegroundColor Cyan
    dotnet ef migrations add AddCoreModules `
        --project backend\OrderManagement.Infrastructure `
        --startup-project backend\OrderManagement.Api `
        --output-dir Data\Migrations
    if ($LASTEXITCODE -ne 0) { throw "Falha ao gerar AddCoreModules." }
} else {
    Write-Host "Migration AddCoreModules ja existe; pulando geracao." -ForegroundColor DarkGray
}

dotnet ef database update `
    --project backend\OrderManagement.Infrastructure `
    --startup-project backend\OrderManagement.Api
if ($LASTEXITCODE -ne 0) { throw "Falha ao atualizar o banco." }

Write-Host "" 
Write-Host "OK: backend compilado, testes executados e banco atualizado." -ForegroundColor Green
Write-Host "Agora execute:" -ForegroundColor Cyan
Write-Host "  dotnet run --project backend\OrderManagement.Api"
