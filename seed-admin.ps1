$body = @{ name='Administrador'; email='admin@local.test'; password='Admin123!'; role=1 } | ConvertTo-Json
try { Invoke-RestMethod -Method Post -Uri 'http://localhost:5294/api/auth/register' -ContentType 'application/json' -Body $body | ConvertTo-Json -Depth 5 } catch { Write-Host $_.Exception.Message }
