param(
    [string]$FrontendUrl = "http://localhost:4200"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$frontend = Join-Path $repoRoot "frontend\order-management-web"

Write-Host "== Captura de portfolio ==" -ForegroundColor Cyan
Write-Host "Frontend esperado em $FrontendUrl"

try {
    Invoke-WebRequest -Uri $FrontendUrl -UseBasicParsing -TimeoutSec 5 | Out-Null
}
catch {
    throw "Frontend não está acessível em $FrontendUrl. Execute .\start-dev.ps1 antes."
}

Push-Location $frontend
try {
    Write-Host "Instalando ferramentas temporárias de captura..." -ForegroundColor Yellow
    npm install --no-save --package-lock=false @playwright/test@1.63.0 gif-encoder-2 pngjs

    Write-Host "Garantindo Chromium do Playwright..." -ForegroundColor Yellow
    npx playwright install chromium

    $env:PORTFOLIO_BASE_URL = $FrontendUrl

    Write-Host "Capturando telas reais..." -ForegroundColor Yellow
    npx playwright test portfolio.capture.spec.ts --config=playwright.portfolio.config.ts --reporter=line

    Write-Host "Gerando GIF..." -ForegroundColor Yellow
    node build-portfolio-gif.cjs
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "Arquivos gerados em docs\screenshots:" -ForegroundColor Green
Write-Host "  01-dashboard.png"
Write-Host "  02-categories.png"
Write-Host "  03-products.png"
Write-Host "  04-customers.png"
Write-Host "  05-orders.png"
Write-Host "  order-flow.gif"
