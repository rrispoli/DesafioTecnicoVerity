# 0006. Idempotência e consistência eventual no consumo de eventos

## Status

Aceita

## Contexto

Como consequência do Outbox Pattern (ADR 0005) e do RabbitMQ/MassTransit, a entrega dos eventos é
**at-least-once**: em cenários de falha (timeout, crash do worker antes de confirmar, reprocessamento do outbox), o
mesmo `EntryCreatedEvent` pode chegar duplicado ao `DailyBalance.Worker`. Sem tratamento, isso duplicaria
créditos/débitos no saldo — inaceitável em um domínio financeiro. Além disso, múltiplas instâncias do
`DailyBalance.Worker` rodando em paralelo podem tentar atualizar o saldo da mesma data ao mesmo tempo.

## Decisão

Resolvi isso com duas medidas:

1. **Idempotência via tabela de controle**: cada evento processado gera uma linha em `ProcessedEvents` com o
   `EventId` como chave. Antes de aplicar a atualização de saldo, o `UpdateBalanceCommandHandler` checa se aquele
   `EventId` já foi processado; se sim, não reaplica o débito/crédito.
2. **Retry otimista em conflito de concorrência**: se o `SaveChangesAsync` disparar `DbUpdateException` por
   conflito na mesma linha de saldo (em cenários onde o primeiro registro tenta ser salvo ao mesmo tempo), eu destaco as entidades do `ChangeTracker`, espero um backoff curto e
   aleatório (150–400ms) e tento novamente, até 5 vezes.

## Alternativas Consideradas

- **"Exactly-once" via transação distribuída entre broker e banco**: tecnicamente complexo demais e sem suporte
  nativo simples entre RabbitMQ e SQL Server; idempotência no consumidor é a abordagem mais usada nesse cenário.
- **Deduplicação no nível do broker** (ex.: filtros de idempotência prontos do MassTransit): avaliei, mas preferi
  uma tabela de controle explícita no domínio, para ter visibilidade total do que já foi processado e garantir que
  "aplicar o saldo" e "marcar como processado" aconteçam na mesma transação.
- **Lock pessimista na tabela de Balance**: reduziria a necessidade de retry, mas aumenta contenção sob alta
  concorrência; preferi o retry otimista com backoff por ser mais amigável a escalonamento horizontal do worker.

## Consequências

**Positivas**
- Proteção contra duplicação de eventos, essencial para a integridade do saldo.
- Suporta múltiplas instâncias do `DailyBalance.Worker` rodando em paralelo, tratando corretamente conflitos de
  escrita concorrente.
- `ProcessedEvents` também serve como trilha de auditoria de quais eventos já impactaram o saldo.

**Trade-offs que assumi**
- A tabela `ProcessedEvents` cresce indefinidamente e vai precisar de uma estratégia de retenção/arquivamento no
  longo prazo.
- Consistência eventual: existe uma janela entre criar o lançamento e atualizar o saldo, então uma consulta ao
  saldo logo após criar um lançamento pode não refletir a mudança de imediato. Considerei isso aceitável dado o
  requisito de negócio (relatório de saldo diário, não consistência forte em tempo real).
