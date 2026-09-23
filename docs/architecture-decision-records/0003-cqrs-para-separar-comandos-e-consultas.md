# 0003. CQRS para separar comandos e consultas

## Status

Aceita

## Contexto

Entries e DailyBalance têm operações de escrita (`CreateEntryCommand`, `UpdateBalanceCommand`) com validação e
efeitos colaterais (persistência, publicação de evento) bem diferentes das operações de leitura (`GetEntryQuery`,
`GetBalanceQuery`), que só precisam projetar dados rapidamente, sem regra de negócio. Além disso, dentro da Clean
Architecture já adotada no projeto, eu queria uma forma de organizar a camada de Application que reforçasse a
separação entre caso de uso de escrita e caso de uso de leitura, em vez de um único serviço genérico fazendo os
dois papéis.

## Decisão

Apliquei **CQRS (Command Query Responsibility Segregation)**: comandos e queries são modelados como classes
separadas, cada um com seu handler, sua validação (FluentValidation) e seu modelo de retorno próprio, seguindo
abstrações simples em `Shared.Application` (`ICommandHandler<TCommand, TResponse>`, `IQueryHandler<TQuery,
TResponse>`) injetadas diretamente nos endpoints Minimal API.

## Alternativas Consideradas

- **Um único modelo de handler/serviço para escrita e leitura** (sem CQRS): mais simples de começar, mas mistura
  responsabilidades diferentes na mesma classe e dificulta testes focados, além de não deixar claro, na camada de
  Application, qual código é caso de uso de escrita e qual é apenas leitura/projeção.
- **Repository genérico com métodos de leitura e escrita compartilhados**: mais próximo de um CRUD tradicional,
  mas não favorece a evolução de leitura e escrita em ritmos e tecnologias diferentes.

## Consequências

**Positivas**
- Encaixa bem na Clean Architecture já usada no projeto: comandos e queries ficam isolados na camada de
  Application, sem vazar detalhes de infraestrutura, e cada um pode evoluir sua própria implementação
  independentemente.
- Modelos de leitura e escrita evoluem de forma independente: o lado de queries pode, por exemplo, evoluir para
  consultar o saldo consolidado a partir de um cache (Redis) ou de uma read model otimizada, sem tocar em nada do
  lado de comandos — a separação já deixa esse caminho pronto para quando o volume de consultas justificar.
- Handlers pequenos e com responsabilidade única, mais fáceis de testar isoladamente (cada comando/query tem seu
  próprio teste, sem depender do restante do fluxo).

**Trade-offs que assumi**
- Para o tamanho deste desafio, CQRS é mais arquivo/indireção do que um CRUD simples pediria; assumi esse custo
  porque acho que deixa as responsabilidades mais claras para quem for avaliar o código e prepara o projeto para
  crescer (ex.: adicionar cache só no lado de queries).
- Não usei uma biblioteca de mediator (como MediatR) para o despacho de comandos/queries — implementei manualmente
  com as abstrações citadas acima, então não tenho pipeline behaviors prontos (logging, validação, transação
  automáticos); apliquei isso manualmente onde precisei (ex.: filtro `ValidationEndpointFilter`).
