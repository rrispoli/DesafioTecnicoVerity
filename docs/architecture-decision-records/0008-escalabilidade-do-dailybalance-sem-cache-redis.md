# 0008. Escalabilidade do DailyBalance sem cache (Redis)

## Status

Aceita

## Contexto

O desafio define uma meta quantitativa explícita: o serviço de consolidado diário (DailyBalance) precisa suportar
picos de **50 requisições por segundo, com no máximo 5% de perda**. Como a consulta de saldo (`GetBalanceQuery`) é
o endpoint mais sensível a esse pico, eu precisava decidir se a estratégia para atingir essa meta passaria por
introduzir uma camada de cache (Redis, por exemplo) na frente da consulta, ou se o modelo de dados já seria
suficiente.

## Decisão

Optei por **não introduzir Redis (ou qualquer cache distribuído)** e, em vez disso, modelei a tabela `Balances` e a
consulta de saldo para que a leitura já fosse barata o bastante para absorver o pico sozinha:

- A coluna `Date` tem um **índice único** (`HasIndex(x => x.Date).IsUnique()`), então `GetBalanceQueryHandler` faz
  uma busca por igualdade (`FirstOrDefaultAsync(x => x.Date == query.Date)`) que o SQL Server resolve como um
  index seek, não um scan.
- A tabela `Balances` tem **uma linha por dia** (uma por serviço/instância de negócio), então mesmo com anos de
  histórico o volume de dados é pequeno o suficiente para caber tranquilamente em memória/buffer pool do SQL
  Server — não existe o cenário de tabela grande que normalmente justifica um cache de leitura.
- Rodei testes de performance locais simulando o pico de 50 req/s contra o endpoint de consulta de saldo; as
  evidências estão documentadas em `docs/assets/`.

## Alternativas Consideradas

- **Cache distribuído (Redis) na frente de `GetBalanceQuery`**: rejeitei para o escopo atual porque adicionaria uma
  peça de infraestrutura extra (mais um serviço para subir, monitorar e invalidar) para resolver um problema que o
  índice único em `Date` e o volume baixo de linhas já resolvem sozinhos. Cache de leitura compensa quando a
  consulta é cara (joins pesados, agregações, tabela grande) ou quando o mesmo dado é lido com muito mais
  frequência do que é escrito — não é o caso aqui: a consulta já é um index seek em uma tabela pequena.

## Consequências

**Positivas**
- Uma peça a menos de infraestrutura (sem Redis, sem lógica de invalidação de cache, sem risco de servir dado
  desatualizado por cache mal invalidado após uma atualização de saldo).
- A consulta de saldo permanece simples e correta por padrão: sempre lê o dado mais recente direto do banco.
- O índice único em `Date` também reforça a regra de negócio (uma linha de saldo por dia) diretamente no schema.

**Trade-offs que assumi**
- Toda requisição de leitura ainda bate no banco; se o padrão de acesso mudar no futuro (ex.: muitos clientes
  consultando repetidamente a mesma data, ou o volume de linhas crescer de forma muito diferente do esperado),
  cache passaria a fazer sentido — é a evolução natural já apontada na ADR 0003.
- Não implementei rate limiting no endpoint; a meta de 50 req/s foi validada via teste de performance manual (ver
  `docs/assets/`), não por um teste de carga automatizado no pipeline de CI.
