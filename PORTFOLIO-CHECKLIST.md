# Checklist antes de publicar no GitHub

- [ ] `dotnet build` retorna 0 erros.
- [ ] `dotnet test` passa.
- [ ] `npm run build` passa.
- [ ] `docker compose up -d` funciona.
- [ ] Login funciona.
- [ ] Categoria pode ser criada/editada/excluída.
- [ ] Produto pode ser criado/editado/excluído.
- [ ] Cliente pode ser criado/editado/excluído.
- [ ] Pedido reduz estoque.
- [ ] Cancelamento restaura estoque.
- [ ] Conclusão bloqueia cancelamento posterior.
- [ ] Dashboard carrega dados.
- [ ] Nenhum JWT real está commitado.
- [ ] Nenhuma senha pessoal está commitada.
- [ ] `appsettings` contém apenas configuração segura para demonstração/local.
- [ ] `node_modules`, `bin`, `obj` e `.angular` estão ignorados.
- [ ] `package-lock.json` foi commitado.
- [ ] Screenshots foram adicionados.
- [ ] GitHub Actions está verde.

## Comandos finais

```powershell
dotnet build
dotnet test
cd frontend\order-management-web
npm run build
```
