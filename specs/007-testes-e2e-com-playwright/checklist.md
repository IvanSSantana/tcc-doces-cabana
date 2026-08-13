# Checklist de conclusão — Testes E2E com Playwright

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente
- [x] Todo `CA-xx` foi verificado — pelos próprios testes E2E rodando contra
      a aplicação de verdade (não é fumaça manual à parte: os 17 testes
      *são* a verificação). CA-13 é a exceção — provado rodando a suíte duas
      vezes seguidas na T028, não por um teste que se auto-verificaria
- [x] Nada fora do escopo declarado entrou junto na entrega — a única
      produção fora do prometido no plano é o botão "Sair" no cabeçalho,
      registrado como achado da implementação (ver nota abaixo), não como
      escopo que cresceu sozinho
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

## Constituição

- [x] **I** — `EmailServiceArquivo` implementa `IEmailService` na
      Infrastructure, mesma direção do `EmailService` que já existia.
      `DocesCabana.Tests.E2E` referencia a MVC com
      `ReferenceOutputAssembly="false"` — não consome tipo nenhum dela
- [x] **II** — n/a; nenhuma entidade de domínio nova
- [x] **III** — n/a; nenhuma entrada de usuário nova (o formulário de e-mail
      não existe — é config interna)
- [x] **IV** — Nomes em português em toda a base nova: `AplicacaoEmExecucao`,
      `GeradorDeDados`, `CaixaDeEntrada`, `PaginaLogin` etc.; testes no
      formato `Dado_..._Quando_..._Entao_...`
- [x] **V** — Emenda 1.3.0 executada **antes** de qualquer pacote entrar
      (T003 antes de T012). `EmailServiceArquivoTests`/`RegistroDeEmailTests`
      (única produção nova de verdade) escritos vermelhos antes da
      implementação (T004-T006 antes de T007-T009). Os testes E2E nascem
      verdes por descreverem comportamento já existente — registrado como
      exceção deliberada no plano §2, não como testes pulados
- [x] **VI** — Nenhuma escrita nova fora do que os fluxos já provam;
      nenhuma migration — o banco do E2E nasce das migrations que a própria
      aplicação roda ao subir
- [x] **VII** — Nenhuma ação de controller nova. O botão "Sair" acrescentado
      usa a ação `Logout` já existente, que já tinha `[ValidateAntiForgeryToken]`;
      o formulário novo leva `@Html.AntiForgeryToken()`
- [x] **VIII** — n/a; nenhum controller novo

## Testes

- [x] `dotnet build` sem warnings novos — 0 avisos
- [x] `dotnet test` verde — 265/265 na suíte rápida (baseline T002: 257) +
      17/17 na suíte E2E, rodadas separadas e também juntas
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] Feature que toca persistência tem teste de integração — n/a para
      persistência nova (não há), mas o próprio E2E é o nível de teste que
      teria faltado até aqui; nenhum teste de integração dos existentes foi
      alterado

## Interface

- [x] `asp-action` de cada formulário aponta para uma ação que existe de
      fato — inclusive o formulário novo do botão "Sair"
- [x] Erros de validação aparecem no campo — confirmado pelos próprios
      testes E2E (`ErroDeSenha`, `ErroDePreco`), que leem o DOM real
- [ ] Testado em largura de tela pequena — fora de escopo (spec §8: "mais de
      um navegador" e viewport ficaram de fora deliberadamente)
- [x] n/a — nenhuma tela nova com valor monetário ou data além das que já
      existiam e já formatam em `pt-BR`

## Segurança

- [x] Nenhum segredo commitado — `appsettings.Example.json` documenta as
      chaves novas sem valor real; a senha do administrador do E2E é gerada
      em memória por `AplicacaoEmExecucao`, nunca persistida fora do banco
      descartável
- [x] Entrada do usuário não é interpolada em HTML sem escape — inalterado
- [x] Mensagens de erro não vazam existência de conta nem detalhe interno —
      `RecuperacaoDeSenhaTests` prova ao vivo que login existente e
      inexistente recebem a mesma mensagem (CA-07)

---

## Achado registrado durante a execução

Não havia, na interface, elemento nenhum que acionasse `Logout` — o link
"Conta" do cabeçalho também aponta para uma ação (`HomeController.Conta`)
que não existe. Sem isso, CA-06 ("Sair") não tinha como ser provado *pela
interface*, como a RF-02 exige. Perguntado ao usuário, que optou por corrigir
a lacuna: um botão "Sair" real foi acrescentado ao cabeçalho (form POST com
antiforgery para a ação `Logout` já existente), visível só quando autenticado.

O link "Conta" continua quebrado — não foi corrigido, por não bloquear
nenhum critério desta spec e por ser uma mudança maior (criar
`HomeController.Conta` e a view correspondente). Registrado aqui como dívida
para quem tocar essa área depois.
