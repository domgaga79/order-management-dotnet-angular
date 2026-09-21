# Roteiro de entrevista — Order Management

## Pitch curto — 60 a 90 segundos

> Desenvolvi um sistema full stack de gerenciamento de pedidos usando .NET 8 e Angular 17. O backend é uma API REST em ASP.NET Core com Entity Framework Core e PostgreSQL, autenticação JWT e autorização por roles.
>
> O ponto técnico mais importante é o fluxo de pedidos e estoque. A criação roda em transação, agrupa itens repetidos, bloqueia os registros de produto com `FOR UPDATE`, valida estoque e baixa as quantidades de forma atômica. O item do pedido guarda o preço da venda, então o histórico não muda se o preço atual do produto for alterado.
>
> No cancelamento, o estoque é restaurado; pedidos concluídos não podem ser cancelados. Também implementei Angular com rotas protegidas, services e interceptor, Docker para o PostgreSQL, Swagger, CI no GitHub Actions e uma suíte com testes unitários e testes de integração contra PostgreSQL real via Testcontainers.

## Estrutura para explicar qualquer decisão

Use:

```text
problema -> risco -> decisão -> implementação -> resultado
```

Exemplo:

```text
Problema: duas vendas concorrentes podem tentar consumir o mesmo estoque.
Risco: overselling.
Decisão: proteger a validação e a baixa em transação.
Implementação: PostgreSQL FOR UPDATE + reload + validação.
Resultado: a segunda transação enxerga o estoque atualizado antes de concluir.
```

## Cinco pontos que vale enfatizar

1. **Regras de negócio, não só CRUD** — estoque, estados de pedido e histórico de preço.
2. **Concorrência** — lock pessimista com `FOR UPDATE`.
3. **Consistência** — transação e rollback em falhas.
4. **Qualidade** — unit tests + integração com PostgreSQL real em Testcontainers.
5. **Entrega completa** — Angular, API, banco, Docker, Swagger e GitHub Actions.

## Perguntas prováveis

### Por que DTO em vez de expor a entidade?

Para separar o contrato HTTP do modelo interno. Isso permite controlar os dados de entrada e saída e evoluir o domínio sem necessariamente quebrar clientes da API.

### O que é Dependency Injection?

É fornecer dependências externamente em vez de criá-las dentro da classe. No ASP.NET Core registro implementações no container e recebo interfaces pelo construtor, reduzindo acoplamento e facilitando testes.

### O que significa `Scoped`?

Uma instância é reutilizada dentro da mesma requisição HTTP e descartada ao final dela. É adequado para `DbContext`.

### Por que `DbContext` não deve ser Singleton?

Porque mantém estado de tracking e não é thread-safe. Compartilhar o mesmo contexto entre requisições pode misturar estado e causar problemas de concorrência.

### O que `async/await` resolve aqui?

Banco e rede são I/O. Enquanto a aplicação aguarda a resposta, a thread não precisa ficar bloqueada, o que melhora a escalabilidade.

### O que é `AsNoTracking`?

Em consultas apenas de leitura, evita que o EF Core rastreie alterações nas entidades retornadas, reduzindo trabalho do Change Tracker.

### Por que `decimal` para dinheiro?

Porque representa valores decimais com precisão apropriada para cálculos financeiros, evitando problemas comuns de representação binária de `double`.

### Por que uma transação na criação do pedido?

Para garantir atomicidade: ou pedido e estoque são atualizados juntos, ou nada fica parcialmente persistido.

### Por que `FOR UPDATE`?

Para bloquear a linha do produto dentro da transação e impedir que duas requisições validem simultaneamente o mesmo estoque antigo.

### O que o teste com Testcontainers acrescenta?

Ele executa as regras críticas contra PostgreSQL real em um container descartável. Assim, o teste cobre comportamento relacional, transações e SQL específico como `FOR UPDATE`, que um provider em memória não reproduziria de forma fiel.

### 401 x 403

- `401 Unauthorized`: autenticação ausente ou inválida.
- `403 Forbidden`: usuário autenticado, mas sem permissão.

### `IQueryable` x `IEnumerable`

`IQueryable` permite que o provider componha e traduza a consulta para SQL antes de executá-la. `IEnumerable` trabalha com objetos já enumerados do lado da aplicação.

## Caso real de debugging

> No cadastro de produtos, a API retornou 500. Segui o stack trace e identifiquei que o `RangeAttribute` tentava converter `0.01` usando cultura `pt-BR`. Corrigi a validação e validei os dois caminhos: `201 Created` no caso válido e `400 Bad Request` no inválido.

Outro caso:

> Durante o setup, uma instância antiga da API mantinha DLLs bloqueadas. O próprio log mostrava o PID do processo. Encerrei a API, removi um teste-placeholder residual e ajustei o setup para parar imediatamente em caso de falha.

## Limitações/evoluções que posso citar

- testes HTTP end-to-end com `WebApplicationFactory`;
- testes de carga/concorrência com múltiplas requisições simultâneas;
- refresh token;
- logs estruturados e observabilidade;
- paginação e filtros server-side;
- secrets fora de configuração local;
- deploy automatizado.

Reconhecer evoluções demonstra visão técnica sem desvalorizar o que já foi entregue.
