# 0004. Autenticação via API Key (em vez de JWT/OAuth2)

## Status

Aceita

## Contexto

As duas APIs (`Entries.WebApi` e `DailyBalance.WebApi`) precisam de autenticação. O desafio não pede um usuário
final fazendo login e não deixa claro que é um sistema com múltiplos usuários, logo assumi uma comunicação serviço-a-serviço/cliente técnico consumindo endpoints REST de
lançamentos e saldo.

## Decisão

Implementei autenticação via **API Key**, enviada no header `X-API-Key`. Criei um
`AuthenticationHandler<ApiKeyAuthenticationOptions>` customizado que compara a chave recebida com a chave
configurada (`ApiKey:Value`) usando `CryptographicOperations.FixedTimeEquals`, para evitar *timing attacks* na
comparação de strings.

## Alternativas Consideradas

- **JWT com um Identity Provider** (IdentityServer, Auth0, Entra ID, Keycloak): mais robusto para cenários
  multiusuário com escopos e expiração de token, mas seria complexidade desproporcional para uma comunicação
  simples serviço-a-serviço, e não é o foco principal do desafio (que é sobre lançamentos/saldo, não sobre identidade).
- **Sem autenticação**: descartei logo no início — os endpoints expõem dados financeiros e o desafio pede segurança.

## Consequências

**Positivas**
- Simples de implementar, configurar e testar (Scalar já documenta o header exigido).
- Comparação em tempo constante evita vazar informação da chave via timing attack.
- Suficiente para o escopo do desafio e para uma comunicação server-to-server controlada.

**Trade-offs que assumi**
- Não existe identidade de usuário final, escopos granulares ou expiração/rotação automática de credencial — uma
  única chave estática dá acesso total à API.
- Rotação de chave é manual e não há suporte nativo a múltiplas chaves ativas ao mesmo tempo.
- Se o sistema precisasse de múltiplos consumidores externos ou usuários finais no futuro, eu migraria para
  OAuth2/OIDC, mantendo a API Key só para integrações internas.
