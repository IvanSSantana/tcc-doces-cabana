# Checklist de conclusão — [NOME DA FEATURE]

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [ ] Todo `RF-xx` da spec tem código correspondente
- [ ] Todo `CA-xx` foi verificado manualmente na aplicação rodando
- [ ] Nada fora do escopo declarado entrou junto na entrega
- [ ] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

## Constituição

- [ ] **I** — Nenhuma referência nova entre projetos que inverta a direção de dependência
- [ ] **II** — Entidades novas têm `private set`, validam no construtor e têm `protected Ctor()`
- [ ] **III** — Regras críticas estão no validator **e** no domínio
- [ ] **IV** — Nomes, mensagens e comentários em português
- [ ] **V** — Os testes foram escritos antes e falharam antes de passar
- [ ] **VI** — Toda escrita chama `IUnitOfWork`; migration criada se o esquema mudou
- [ ] **VII** — `[ValidateAntiForgeryToken]` em todo POST, chamadas assíncronas aguardadas, rota administrativa autorizada, POST-Redirect-Get no sucesso
- [ ] **VIII** — Sem `try/catch` em ação de controller

## Testes

- [ ] `dotnet build` sem warnings novos
- [ ] `dotnet test` verde
- [ ] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [ ] Feature que toca persistência tem teste de integração

## Interface

- [ ] `asp-action` de cada formulário aponta para uma ação que existe de fato
- [ ] Erros de validação aparecem no campo (`asp-validation-for`) e não só no resumo
- [ ] Testado em largura de tela pequena
- [ ] Valores monetários e datas formatados em `pt-BR`

## Segurança

- [ ] Nenhum segredo commitado
- [ ] Entrada do usuário não é interpolada em HTML sem escape
- [ ] Mensagens de erro não vazam existência de conta nem detalhe interno
