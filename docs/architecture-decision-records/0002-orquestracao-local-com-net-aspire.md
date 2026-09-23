# 0002. .NET Aspire como orquestrador local de desenvolvimento

## Status

Aceita

## Contexto

Com dois serviços independentes (ADR 0001), cada um com WebApi + Worker, mais SQL Server e RabbitMQ, subir tudo
manualmente para rodar/testar localmente ficaria trabalhoso. Precisava de uma forma simples de orquestrar essa stack
durante o desenvolvimento, com boa observabilidade para acompanhar o fluxo entre os serviços.

## Decisão

Usei o **.NET Aspire** (projeto `AppHost`) para orquestrar o ambiente de desenvolvimento local: um `dotnet run` no
`AppHost` sobe toda a stack (SQL Server, RabbitMQ, as quatro aplicações e o Aspire Dashboard com OpenTelemetry) com
service discovery e health checks prontos, sem precisar de scripts adicionais.

## Alternativas Consideradas

- **Docker Compose simples**: funcionaria, mas eu perderia o dashboard de telemetria integrado, o service discovery
  automático e a resiliência HTTP padrão que o Aspire já traz via `ServiceDefaults`.
- **Subir cada serviço manualmente** (`dotnet run` em cada projeto + containers do SQL Server/RabbitMQ à parte):
  funcional, mas repetitivo e sem a correlação de telemetria entre os serviços que o Aspire Dashboard oferece.

## Consequências

**Positivas**
- Onboarding trivial: `cd AppHost && dotnet run` sobe a stack inteira sem scripts adicionais.
- Observabilidade (traces/metrics/logs) correlacionada entre os serviços desde o primeiro dia, via `ServiceDefaults`.
- Resiliência HTTP (`AddStandardResilienceHandler`) e health checks já habilitados por convenção.

**Trade-offs que assumi**
- Aspire acopla o fluxo de desenvolvimento local a essa ferramenta específica; quem quiser rodar sem o Aspire
  precisaria subir SQL Server, RabbitMQ e os quatro processos manualmente.