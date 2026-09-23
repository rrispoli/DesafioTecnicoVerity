# 0001. Dois serviços independentes: Entries e DailyBalance

## Status

Aceita

## Contexto

O desafio pede duas responsabilidades bem distintas: registrar lançamentos (créditos e débitos) e consolidar/consultar
o saldo diário. Um requisito não funcional deixa isso ainda mais claro: o serviço de lançamentos não pode ficar
indisponível se o serviço de saldo cair, e o serviço de saldo precisa aguentar picos de carga (50 req/s, ≤5% de perda)
sem afetar o outro. Precisava decidir logo no início se isso seria um monolito modular ou dois serviços separados.

## Decisão

Optei por criar **Entries** e **DailyBalance** como dois serviços independentes, cada um com seu próprio banco
(`entries-database` e `dailybalance-database`) e sua própria Clean Architecture completa (Domain → Application →
Infrastructure → WebApi/Worker). Eles só se comunicam de forma assíncrona, via evento de integração
(`EntryCreatedEvent`) publicado pelo Outbox do Entries e consumido pelo Worker do DailyBalance.

## Alternativas Consideradas

- **Monolito modular** (um único processo com módulos internos se comunicando em memória): seria mais rápido de montar,
  mas quebra exatamente o requisito que motivou a separação — no mesmo processo, um problema no módulo de saldo
  (vazamento de memória, pico de CPU) derruba junto o módulo de lançamentos.
- **Comunicação síncrona (REST/gRPC) entre os dois serviços**: descartei porque isso recria o mesmo acoplamento de
  disponibilidade que eu estava tentando eliminar ao separar os serviços.

## Consequências

**Positivas**
- Um serviço pode cair, escalar ou reiniciar sem impactar o outro.
- DailyBalance (mais lido) pode escalar horizontalmente sem precisar escalar Entries junto, e vice-versa.
- Bancos separados evitam concorrência de lock/IO entre os dois domínios (Database-per-Service).

**Trade-offs que assumi**
- Consistência eventual entre os dois serviços (detalhado nas ADRs 0005 e 0006) — não existe uma "foto" única e
  transacional do lançamento + saldo.
- Mais overhead operacional: dois bancos, dois pipelines de deploy, precisa de observabilidade distribuída (por isso
  usei OpenTelemetry + Aspire Dashboard, já presentes na solução).
- Um teste end-to-end (lançamento → saldo) exige subir os dois serviços juntos; hoje cobri esse fluxo apenas com
  testes unitários, e testes de integração com Testcontainers seriam o próximo passo natural.
