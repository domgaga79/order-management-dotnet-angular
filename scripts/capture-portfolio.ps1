param(
    [string]$FrontendUrl = "http://localhost:4200"
)

$ErrorActionPreference = "Stop"

function Assert-LastExitCode {
    param([string]$Step)
    if ($LASTEXITCODE -ne 0) {
        throw "$Step falhou com exit code $LASTEXITCODE."
    }
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$frontend = Join-Path $repoRoot "frontend\order-management-web"
$screenshots = Join-Path $repoRoot "docs\screenshots"

Write-Host "== Captura de portfolio ==" -ForegroundColor Cyan
Write-Host "Repositorio: $repoRoot"
Write-Host "Frontend:    $frontend"
Write-Host "Saida:       $screenshots"
Write-Host ""

if (-not (Test-Path $frontend)) {
    throw "Diretorio do frontend nao encontrado: $frontend"
}

$requiredFiles = @(
    "playwright.portfolio.config.ts",
    "portfolio.capture.spec.ts",
    "build-portfolio-gif.cjs"
)

foreach ($file in $requiredFiles) {
    $fullPath = Join-Path $frontend $file
    if (-not (Test-Path $fullPath)) {
        throw "Arquivo obrigatorio nao encontrado: $fullPath"
    }
}

try {
    Invoke-WebRequest -Uri $FrontendUrl -UseBasicParsing -TimeoutSec 5 | Out-Null
}
catch {
    throw "Frontend nao esta acessivel em $FrontendUrl. Execute .\start-dev.ps1 antes."
}

New-Item -ItemType Directory -Force -Path $screenshots | Out-Null

Push-Location $frontend
try {
    Write-Host "Instalando ferramentas temporarias..." -ForegroundColor Yellow
    npm install --no-save --package-lock=false @playwright/test@1.63.0 gif-encoder-2 pngjs
    Assert-LastExitCode "npm install"

    Write-Host "Instalando Chromium do Playwright..." -ForegroundColor Yellow
    npx playwright install chromium
    Assert-LastExitCode "playwright install chromium"

    $env:PORTFOLIO_BASE_URL = $FrontendUrl

    Write-Host "Capturando telas..." -ForegroundColor Yellow
    npx playwright test portfolio.capture.spec.ts --config=playwright.portfolio.config.ts --reporter=line
    Assert-LastExitCode "playwright test"

    $pngs = @(
        "01-dashboard.png",
        "02-categories.png",
        "03-products.png",
        "04-customers.png",
        "05-orders.png"
    )

    foreach ($png in $pngs) {
        $pngPath = Join-Path $screenshots $png
        if (-not (Test-Path $pngPath)) {
            throw "Captura esperada nao foi criada: $pngPath"
        }
    }

    Write-Host "Gerando GIF..." -ForegroundColor Yellow
    node build-portfolio-gif.cjs
    Assert-LastExitCode "geracao do GIF"

    $gifPath = Join-Path $screenshots "order-flow.gif"
    if (-not (Test-Path $gifPath)) {
        throw "GIF esperado nao foi criado: $gifPath"
    }
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "Captura concluida com sucesso." -ForegroundColor Green
Get-ChildItem $screenshots |
    Select-Object Name, Length, LastWriteTime |
    Format-Table -AutoSize
