# Arquitetura do Order Management

## Objetivo

A arquitetura separa responsabilidades sem introduzir complexidade desnecessária. O foco é manter o código compreensível, testável e adequado a um sistema corporativo de pequeno/médio porte.

## Fluxo de uma requisição

```text
Browser
  |
  | HTTP + JSON + JWT
  v
Angular
  |
  v
Controller
  |
  v
Application Service
  |
  +----------------------+
  |                      |
  v                      v
Repository           Infrastructure Service
  |                      |
  +----------+-----------+
             |
             v
          EF Core
             |
             v
         PostgreSQL
```

## Camadas

### API

Responsável por:

- endpoints HTTP;
- status codes;
- model binding;
- autenticação e autorização;
- Swagger/OpenAPI;
- CORS;
- registro das dependências.

O Controller deve permanecer fino e delegar regras para serviços.

### Application

Contém:

- DTOs de entrada e saída;
- interfaces de serviços;
- interfaces de repositories;
- casos de uso que não dependem diretamente do transporte HTTP.

### Domain

Contém os conceitos centrais do sistema:

- `User`
- `Customer`
- `Category`
- `Product`
- `Order`
- `OrderItem`
- enums de status e roles.

### Infrastructure

Responsável por detalhes externos:

- Entity Framework Core;
- `AppDbContext`;
- PostgreSQL;
- repositories;
- transações;
- consultas e operações de concorrência.

## Dependency Injection

As dependências são registradas no ASP.NET Core e recebidas pelo construtor.

Exemplo conceitual:

```text
ProductsController
       |
       v
IProductService
       |
       v
ProductService
       |
       v
IProductRepository
       |
       v
ProductRepository
```

Isso reduz o acoplamento e permite testar as regras com implementações simuladas.

## Persistência

O `AppDbContext` representa a unidade de trabalho do EF Core durante a requisição. O contexto é registrado como `Scoped`, lifetime adequado para operações HTTP típicas.

Consultas somente de leitura usam `AsNoTracking()` quando possível para evitar trabalho desnecessário do Change Tracker.

## Concorrência de estoque

A criação do pedido usa uma transação e bloqueio pessimista no PostgreSQL:

```sql
SELECT 1
FROM products
WHERE "Id" = ...
FOR UPDATE;
```

Depois do lock, a entidade é recarregada e o estoque é validado novamente.

O objetivo é evitar este cenário:

```text
Estoque = 1

Request A lê 1
Request B lê 1

A vende 1
B vende 1

Resultado incorreto: estoque vendido duas vezes
```

Com lock:

```text
Request A bloqueia produto
Request B aguarda
A valida / reduz / commit
B continua
B recarrega estoque atualizado
B falha se não houver quantidade suficiente
```

## Transações

Operações de pedido são tratadas atomicamente:

```text
BEGIN
  validar cliente
  bloquear produto
  validar estoque
  baixar estoque
  criar itens
  salvar pedido
COMMIT
```

Se ocorrer exceção:

```text
ROLLBACK
```

Isso mantém consistência entre pedido e estoque.

## Autenticação

O usuário realiza login e recebe JWT.

```text
Login
  |
  v
JWT
  |
  v
Angular armazena sessão
  |
  v
Interceptor adiciona Authorization: Bearer <token>
  |
  v
ASP.NET Core valida assinatura, issuer, audience e expiração
```

Roles permitem restringir operações administrativas.

## Frontend

O Angular está dividido principalmente em:

```text
pages/
core/
  auth.service
  data.service
  auth.guard
  auth.interceptor
```

- `AuthService`: autenticação e sessão.
- `AuthGuard`: impede acesso a rotas protegidas sem login.
- `AuthInterceptor`: envia JWT automaticamente.
- `DataService`: centraliza chamadas dos módulos principais.

## Decisões deliberadas

O projeto evita, neste estágio:

- CQRS completo;
- MediatR;
- Event Sourcing;
- mensageria;
- múltiplos microserviços;
- abstrações adicionais sem necessidade.

Essas técnicas podem ser válidas em outros contextos, mas adicioná-las sem requisito concreto aumentaria a complexidade sem benefício proporcional.
