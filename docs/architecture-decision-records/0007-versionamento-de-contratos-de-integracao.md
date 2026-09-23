# 0007. Estratégia de versionamento dos contratos de integração (eventos)

## Status

Aceita

## Contexto

`Shared.Contracts` define o contrato `EntryCreatedEvent`, compartilhado por referência de projeto entre o
publicador (`Entries.Infrastructure`/`Entries.Worker`) e o consumidor (`DailyBalance.Worker`). Como os dois
serviços têm ciclos de deploy independentes (ADR 0001), precisava decidir como esse evento poderia evoluir sem
quebrar o consumidor quando os deploys não acontecem ao mesmo tempo.

## Decisão

Defini as seguintes regras para evolução do contrato:

- Mudanças em `EntryCreatedEvent` devem ser **aditivas** (novas propriedades como opcionais/anuláveis), nunca
  remover ou renomear campo existente — o consumidor deve ser tolerante a campos novos que ainda não conhece.
- Uma mudança incompatível (breaking change) exige um **novo tipo de evento versionado** (ex.:
  `EntryCreatedEventV2`), mantendo o publicador emitindo o evento antigo até todos os consumidores migrarem.
- O contrato fica isolado em `Shared.Contracts`, sem dependência de infraestrutura, para que o acoplamento entre
  os serviços fique restrito ao formato da mensagem.

## Alternativas Consideradas

- **Duplicar o contrato do evento em cada projeto** (`Entries` mantém sua própria classe `EntryCreatedEvent` e
  `DailyBalance` mantém outra igual, sem compartilhar `Shared.Contracts`): rejeitei porque isso elimina qualquer
  garantia de compatibilidade em tempo de compilação entre publicador e consumidor. Duplicar o contrato também significa duplicar esforço a
  cada mudança (editar dois lugares em vez de um) e cria risco real de os dois modelos divergirem silenciosamente
  ao longo do tempo.

## Consequências

**Positivas**
- Consumidor tolerante a campos novos evita quebra quando os deploys de Entries e DailyBalance não são
  coordenados.

**Trade-offs que assumi**
- As regras de evolução (aditivo / novo evento versionado) são uma diretriz que documentei para orientar mudanças
  futuras, mas nunca precisaram ser exercitadas de fato neste teste — hoje existe apenas uma versão do
  `EntryCreatedEvent`, então não houve um caso real de breaking change para validar a estratégia na prática.
- Não implementei nenhuma validação automática de contrato (ex.: teste de consumidor garantindo que mudanças no
  evento não quebram o `DailyBalance.Worker`); ficaria como próximo passo natural se o número de eventos crescesse.
