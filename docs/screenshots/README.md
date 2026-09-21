# Screenshots do projeto

Os arquivos visuais desta pasta são gerados a partir da aplicação real em execução.

Na raiz do repositório:

```powershell
.\start-dev.ps1
.\scripts\capture-portfolio.ps1
```

A captura usa Playwright apenas como ferramenta temporária (`npm --no-save`) e não altera o `package.json`.

Arquivos esperados:

- `01-dashboard.png`
- `02-categories.png`
- `03-products.png`
- `04-customers.png`
- `05-orders.png`
- `order-flow.gif`

Depois da captura, revise as imagens e faça commit delas junto com o README.
