# 0005. Outbox Pattern para publicação confiável de eventos

## Status

Aceita

## Contexto

O serviço **Entries** precisa notificar o **DailyBalance** sempre que um lançamento é criado, via RabbitMQ
(MassTransit). O desafio deixa explícito que o **Entries não pode ficar indisponível se o DailyBalance estiver fora
do ar**, ou seja, a gravação do lançamento e a publicação do evento não podem depender uma da outra na mesma
operação síncrona. O problema clássico aqui é que se eu gravar no banco e publicar na fila como duas
operações separadas, e a publicação falhar depois do banco confirmar, o evento se perde e o saldo nunca é
atualizado.

## Decisão

Adotei o **Outbox Pattern**: ao criar um lançamento, o `Entries.Application` grava, no mesmo `SaveChanges` do EF
Core, tanto a entidade `Entry` quanto uma linha em `OutboxMessages`. Um `BackgroundService`
(`OutboxPublisherWorker`, no `Entries.Worker`) varre essa tabela periodicamente, publica as mensagens pendentes no
RabbitMQ e marca cada uma como processada (ou registra falha com contagem de tentativas). Assim, gravar o
lançamento é sempre uma operação local ao banco do Entries, sem depender do RabbitMQ/DailyBalance estarem no ar.

## Alternativas Consideradas

- **Publicar direto no mesmo request HTTP**: descartei porque isso acopla a disponibilidade do endpoint de escrita
  à disponibilidade do broker — exatamente o que o requisito pede para evitar.

## Consequências

**Positivas**
- Entries continua 100% disponível para registrar lançamentos mesmo com RabbitMQ/DailyBalance fora do ar — o que
  acumula é só backlog na tabela de outbox.
- Entrega "at-least-once" garantida, com retry automático e contagem de falhas (`RetryCount`).
- Estado persistido e evento publicado ficam sempre consistentes, porque são gravados na mesma transação.

**Trade-offs que assumi**
- Consistência eventual: existe uma janela entre criar o lançamento e publicar o evento (hoje limitada pelo
  intervalo de polling do worker).
- Preciso tratar entrega duplicada no consumidor (ver ADR 0006), e a tabela de outbox cresce e vai precisar de
  rotina de limpeza/arquivamento.
- Deixei o intervalo e o tamanho de lote do `OutboxPublisherWorker` fixos em código; em um cenário real de alto volume, isso precisaria virar configurável.
