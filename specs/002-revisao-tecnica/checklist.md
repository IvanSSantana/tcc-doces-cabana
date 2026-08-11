# Checklist de conclusão — Revisão técnica da base

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente — RF-01/02 em
      `UsuarioService`/`IdentityDependencyInjection`; RF-03 em `Produto`/`ProdutoMapper`;
      RF-04/05/06 em `AutenticacaoController`/`EsqueceuSenha.cshtml`; RF-07 em
      `Produto`/`Usuario`
- [x] Todo `CA-xx` foi verificado manualmente na aplicação rodando — ver T048;
      CA-05/CA-06 verificados por teste automatizado (`ProdutoTests`,
      `ProdutoMapperTests`, `ProdutoServiceTests`), já que o formulário de
      cadastro de produto só fica funcional na spec `001`
- [x] Nada fora do escopo declarado entrou junto na entrega — a exceção é a
      correção da renderização de erro em `EsqueceuSenha.cshtml` (ver nota
      abaixo), encontrada pela fumaça manual do próprio T048
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

> **Nota sobre a correção fora do plano original:** o T048 (fumaça manual)
> revelou que a refatoração de `EsqueceuSenha` para `TempData` (Fase 2) havia
> removido, sem substituto, a renderização de `ModelState[string.Empty]` — o
> caminho que `FilterException` usa para mostrar "Um erro interno ocorreu"
> quando uma exceção não tratada acontece na ação (por exemplo, falha de SMTP).
> Antes da correção, esse caminho ficava mudo: nem confirmação, nem erro. A
> view voltou a renderizar os dois casos. Verificado ao vivo: com as
> credenciais de SMTP placeholder do `appsettings.json`, a tentativa de envio
> lança `FormatException`, e a mensagem "Um erro interno ocorreu, tente
> novamente mais tarde." aparece corretamente.

## Constituição

- [x] **I** — Nenhuma referência nova entre projetos; `Domain.csproj` teve uma
      removida (`Microsoft.Extensions.Identity.Stores`, não utilizada)
- [x] **II** — `Produto` e `Usuario` passam a validar antes de atribuir; ambos
      já tinham `private set` e `protected Ctor()`
- [x] **III** — `ProdutoDTOValidator` criado, espelhando as invariantes de
      `Produto`
- [x] **IV** — Nomes, mensagens e comentários em português; emenda 1.1.0
      acrescenta a regra arquivo↔tipo↔namespace
- [x] **V** — Testes escritos antes de cada correção; T008/T021 registraram o
      estado vermelho antes da implementação
- [x] **VI** — `IUnitOfWork.SalvarAlteracoes` é o único caminho de escrita;
      nenhuma migration criada (nenhuma mudança de esquema)
- [x] **VII** — `EsqueceuSenha` ganhou guarda de `ModelState` e
      `[ValidateAntiForgeryToken]` já existia; POST-Redirect-Get aplicado
- [x] **VIII** — Nenhum `try/catch` novo em ação de controller

## Testes

- [x] `dotnet build` sem warnings novos — 0 avisos (os 6 NU1903 anteriores
      foram eliminados, não apenas mantidos)
- [x] `dotnet test` verde — 152/152
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...` — confirmado
      sem exceção (T046)
- [x] Feature que toca persistência tem teste de integração — teste de
      atomicidade em `DatabaseIntegrationTests`

## Interface

- [x] `asp-action` de cada formulário aponta para uma ação que existe de fato
      — `EsqueceuSenha` verificado; o restante é inalterado nesta feature
- [x] Erros de validação aparecem no campo (`asp-validation-for`) — preservado;
      a correção pós-T048 restaura também o resumo geral para erro inesperado
- [ ] Testado em largura de tela pequena — fora do escopo; nenhuma tela foi
      redesenhada nesta feature
- [x] Valores monetários e datas formatados em `pt-BR` — inalterado

## Segurança

- [x] Nenhum segredo commitado — `appsettings.json` destrackeado
      (`appsettings.Example.json` versionado no lugar)
- [x] Entrada do usuário não é interpolada em HTML sem escape — inalterado
      (Razor escapa por padrão; o único HTML manual, o link de redefinição de
      senha, já existia antes desta feature)
- [x] Mensagens de erro não vazam existência de conta — RN-05 mantém a mesma
      mensagem para login existente e inexistente, verificado ao vivo (CA-07/08)
