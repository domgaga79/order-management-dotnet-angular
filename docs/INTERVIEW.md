# Roteiro de entrevista — Order Management

## Apresentação de 2 a 3 minutos

> Desenvolvi um sistema de gerenciamento de pedidos usando .NET 8 no backend e Angular 17 no frontend. A API é ASP.NET Core, com Entity Framework Core e PostgreSQL. Organizei a solução em API, Application, Domain, Infrastructure e Tests para separar responsabilidades sem exagerar na arquitetura.
>
> O sistema possui autenticação JWT e roles, CRUD de categorias, produtos e clientes, além do fluxo de pedidos. A parte que considero mais interessante é a regra de estoque: ao criar um pedido, inicio uma transação, agrupo itens repetidos, bloqueio os produtos no PostgreSQL com `FOR UPDATE`, valido o estoque e faço a baixa de forma atômica. O item do pedido também guarda o preço da venda para preservar o histórico mesmo se o produto mudar de preço depois.
>
> No cancelamento, o estoque é restaurado dentro de uma transação; pedidos concluídos não podem ser cancelados. No frontend usei Angular com services, guard, interceptor e rotas protegidas. Também configurei Swagger, Docker e testes com xUnit, e o repositório possui CI para validar backend e frontend.
>
> O projeto foi útil principalmente para consolidar conceitos de C#/.NET e Angular usando regras de negócio que eu já conhecia de sistemas ERP e de pedidos.

## Perguntas prováveis

### Por que DTO em vez de expor a entidade?

Para separar o contrato HTTP do modelo interno. Isso permite controlar o que entra e sai da API, aplicar validações específicas e alterar a entidade sem necessariamente quebrar clientes da API.

### O que é Dependency Injection?

É fornecer as dependências de uma classe externamente em vez de criá-las dentro dela. No ASP.NET Core registro implementações no container e recebo interfaces pelo construtor, reduzindo acoplamento e facilitando testes.

### O que significa `Scoped`?

Uma instância é reutilizada dentro da mesma requisição HTTP e descartada ao final dela. É o lifetime padrão adequado para `DbContext` em aplicações web.

### Por que `DbContext` não deve ser Singleton?

Porque mantém estado de tracking e não é thread-safe. Compartilhar o mesmo contexto entre requests poderia misturar estado e causar problemas de concorrência.

### O que `async/await` resolve aqui?

Operações de banco e rede são I/O. Enquanto a aplicação aguarda a resposta, a thread não precisa ficar bloqueada. Isso melhora escalabilidade sob múltiplas requisições concorrentes.

### `Task` é uma thread?

Não. `Task` representa uma operação assíncrona/futura. Uma Task não implica necessariamente uma nova thread.

### O que é `AsNoTracking`?

Em consultas de leitura, informa ao EF Core que não preciso rastrear alterações nas entidades retornadas, reduzindo trabalho do Change Tracker.

### O que é uma migration?

Uma representação versionada de alterações no schema. `migrations add` gera a mudança; `database update` aplica ao banco e registra o histórico.

### Por que usar `decimal` para dinheiro?

Porque `decimal` representa valores decimais com precisão adequada para cálculos financeiros, evitando vários problemas de representação binária de ponto flutuante de `double`.

### Qual o papel da transação no pedido?

Garantir atomicidade. Se qualquer etapa da criação falhar, nenhuma alteração parcial de pedido ou estoque deve permanecer.

### Por que `FOR UPDATE`?

Para bloquear a linha do produto durante a transação e evitar que duas requisições validem simultaneamente o mesmo estoque antigo e vendam além da quantidade disponível.

### O que é JWT?

Um token assinado com claims do usuário. Depois do login, o cliente envia o token no header `Authorization: Bearer`. A API valida assinatura, validade, issuer e audience antes de autorizar o acesso.

### 401 x 403

- `401 Unauthorized`: autenticação ausente ou inválida.
- `403 Forbidden`: usuário autenticado, mas sem permissão para a operação.

### 200 x 201 x 204

- `200 OK`: operação executada e normalmente retorna conteúdo.
- `201 Created`: novo recurso criado.
- `204 No Content`: operação concluída sem corpo de resposta.

### `IQueryable` x `IEnumerable`

`IQueryable` permite ao provider compor e traduzir a consulta, por exemplo para SQL, antes de executá-la. `IEnumerable` trabalha com enumeração de objetos do lado da aplicação. É importante filtrar no banco antes de materializar grandes conjuntos.

## Pergunta comportamental ligada ao projeto

### Conte um problema que você encontrou e como resolveu

Um bom exemplo real deste projeto:

> Durante o cadastro de produtos, a API retornava 500. Em vez de alterar banco ou repository sem evidência, segui o stack trace. Ele apontava para `RangeAttribute` convertendo `0.01` em decimal sob cultura `pt-BR`. Corrigi a validação para não depender desse parsing cultural e depois validei tanto o caso de sucesso `201` quanto o caso inválido `400`. Esse episódio reforçou a importância de diagnosticar pela origem do erro em vez de alterar camadas aleatoriamente.

Outro exemplo:

> Ao executar o setup, o build falhou porque uma instância anterior da API mantinha DLLs bloqueadas e ainda havia um teste-placeholder do template. Identifiquei o processo pelo próprio log, encerrei a API, removi o arquivo residual e ajustei o setup para parar imediatamente em caso de erro.

## Como explicar decisões sem parecer decorado

Use esta sequência:

```text
problema
  -> risco
  -> decisão
  -> implementação
  -> resultado
```

Exemplo:

```text
Risco: duas vendas concorrentes consumirem o mesmo estoque.
Decisão: proteger a operação dentro de transação com lock pessimista.
Implementação: PostgreSQL FOR UPDATE + reload + validação.
Resultado: a segunda requisição enxerga o estoque atualizado antes de concluir.
```

## Limitações que você pode reconhecer

É positivo reconhecer o que ainda evoluiria:

- aumentar cobertura de testes;
- adicionar testes de integração;
- tirar segredos de configuração local;
- paginação e filtros;
- logs estruturados;
- refresh token;
- deploy automatizado.

Isso demonstra consciência técnica sem desvalorizar o projeto.
