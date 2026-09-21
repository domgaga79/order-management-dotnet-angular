# Order Management — .NET 8 + Angular 17

Sistema full stack para gerenciamento de pedidos, clientes, produtos, categorias e estoque, desenvolvido como projeto de estudo e portfólio com foco em práticas aplicáveis a sistemas corporativos.

## Visão geral

A aplicação possui frontend em Angular 17 e API REST em ASP.NET Core/.NET 8. O backend utiliza Entity Framework Core com PostgreSQL, autenticação JWT, autorização por roles e transações para proteger regras de estoque e pedidos.

### Principais funcionalidades

- Autenticação com JWT.
- Perfis de acesso `Admin` e `Operator`.
- CRUD de categorias.
- CRUD de produtos com preço, estoque, status e categoria.
- CRUD de clientes.
- Criação de pedidos com múltiplos itens.
- Snapshot do preço do produto no item do pedido.
- Baixa automática de estoque ao criar pedido.
- Validação de estoque insuficiente.
- Bloqueio pessimista com `FOR UPDATE` nas operações críticas.
- Cancelamento de pedido com restauração do estoque.
- Conclusão de pedido.
- Dashboard com indicadores operacionais.
- Swagger/OpenAPI com suporte a Bearer Token.
- Docker Compose para PostgreSQL.
- Testes automatizados com xUnit.
- Pipeline de CI para build e testes do backend e build do frontend.

## Arquitetura

```text
Angular 17
    |
    | HTTP / JSON / JWT
    v
ASP.NET Core Web API
    |
    +--> Controllers
    |       |
    |       v
    |    Services
    |       |
    |       v
    | Repository / EF Core
    |       |
    v       v
 PostgreSQL 16
```

A solução .NET está organizada em projetos separados:

```text
backend/
├── OrderManagement.Api
├── OrderManagement.Application
├── OrderManagement.Domain
├── OrderManagement.Infrastructure
└── OrderManagement.Tests
```

### Responsabilidades

| Projeto | Responsabilidade |
|---|---|
| `OrderManagement.Api` | Controllers, autenticação, Swagger, DI e configuração HTTP |
| `OrderManagement.Application` | DTOs, interfaces e serviços de aplicação |
| `OrderManagement.Domain` | Entidades e enums do domínio |
| `OrderManagement.Infrastructure` | EF Core, PostgreSQL, repositories e serviços de persistência |
| `OrderManagement.Tests` | Testes automatizados |

Mais detalhes em [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md).

## Tecnologias

### Backend

- C#
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- Npgsql
- PostgreSQL 16
- JWT Bearer Authentication
- BCrypt
- Swagger / OpenAPI
- xUnit
- Moq

### Frontend

- Angular 17
- TypeScript
- Angular Router
- HttpClient
- RxJS
- Guards e Interceptors

### DevOps

- Docker / Docker Compose
- Git / GitHub
- GitHub Actions

## Regras de negócio importantes

### Criação do pedido

Ao criar um pedido, a aplicação:

1. valida o cliente;
2. agrupa itens repetidos do mesmo produto;
3. inicia uma transação;
4. bloqueia o registro do produto com `FOR UPDATE`;
5. recarrega o estoque dentro da transação;
6. valida a quantidade disponível;
7. reduz o estoque;
8. salva `UnitPrice` e `Subtotal` no item do pedido;
9. calcula o total;
10. persiste o pedido e confirma a transação.

Isso reduz o risco de duas requisições concorrentes venderem o mesmo estoque disponível.

### Cancelamento

Pedidos pendentes podem ser cancelados e suas quantidades são devolvidas ao estoque dentro de uma transação. Pedidos já concluídos não podem ser cancelados.

### Histórico de preço

O item do pedido possui `UnitPrice`, preservando o preço aplicado no momento da venda mesmo que o preço atual do produto seja alterado posteriormente.

## Como executar

### Pré-requisitos

- .NET SDK 8
- Docker Desktop
- Node.js
- npm

### 1. Banco de dados

Na raiz:

```powershell
docker compose up -d
```

### 2. Backend

```powershell
dotnet restore
dotnet build
dotnet run --project backend\OrderManagement.Api
```

Swagger:

```text
http://localhost:5294/swagger
```

### 3. Frontend

Em outro terminal:

```powershell
cd frontend\order-management-web
npm install
npm start
```

Frontend:

```text
http://localhost:4200
```

### Atalho de desenvolvimento

Se os scripts do projeto estiverem presentes:

```powershell
.\start-dev.ps1
```

Para encerrar:

```powershell
.\stop-dev.ps1
```

## Usuário local de demonstração

Para ambiente local de desenvolvimento:

```text
E-mail: admin@local.test
Senha:  Admin123!
```

> Esta credencial é apenas para desenvolvimento local. Em produção, segredos e contas administrativas devem ser configurados por mecanismos seguros de provisionamento e variáveis de ambiente.

## Endpoints principais

```text
POST   /api/auth/login
POST   /api/auth/register

GET    /api/categories
POST   /api/categories
PUT    /api/categories/{id}
DELETE /api/categories/{id}

GET    /api/products
POST   /api/products
GET    /api/products/{id}
PUT    /api/products/{id}
DELETE /api/products/{id}

GET    /api/customers
POST   /api/customers
PUT    /api/customers/{id}
DELETE /api/customers/{id}

GET    /api/orders
GET    /api/orders/{id}
POST   /api/orders
PUT    /api/orders/{id}/complete
PUT    /api/orders/{id}/cancel
```

## Testes

```powershell
dotnet test
```

O projeto de testes utiliza xUnit e pode ser expandido com testes de integração usando `WebApplicationFactory`.

## CI

O workflow em `.github/workflows/ci.yml` executa:

```text
Backend
restore -> build -> test

Frontend
install -> build
```

O pipeline é executado em `push` e `pull_request`.

## Screenshots

Adicione imagens reais da aplicação em `docs/screenshots/` e atualize esta seção. Recomenda-se capturar:

- Login
- Dashboard
- Produtos
- Clientes
- Pedidos
- Swagger

## Pontos técnicos para entrevista

Este projeto permite discutir de forma concreta:

- Dependency Injection e lifetimes.
- `async/await` e `Task`.
- DTOs e separação de responsabilidades.
- EF Core, migrations e Change Tracking.
- `AsNoTracking`.
- LINQ e tradução para SQL.
- JWT e RBAC.
- transações ACID.
- concorrência e `FOR UPDATE`.
- REST e status HTTP.
- Angular services, guards e interceptors.
- Docker e CI/CD.

Há um roteiro específico em [`docs/INTERVIEW.md`](docs/INTERVIEW.md).

## Próximas evoluções

- testes de integração com PostgreSQL isolado;
- paginação e filtros server-side;
- refresh token;
- auditoria de alterações;
- soft delete onde fizer sentido;
- observabilidade e logs estruturados;
- configuração de secrets fora do `appsettings.json`;
- deploy automatizado.

---

Projeto desenvolvido para aprofundar práticas de desenvolvimento full stack com .NET e Angular, aplicando conceitos de APIs REST, regras transacionais, persistência relacional e arquitetura de aplicações corporativas.
