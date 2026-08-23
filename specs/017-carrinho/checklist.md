# Checklist de conclusão — Carrinho

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente (ver tabela de Rastreabilidade em `tasks.md`)
- [ ] Todo `CA-xx` foi verificado manualmente na aplicação rodando — **não verificado nesta execução**: todos os 23 CA têm cobertura E2E automatizada (Playwright, `CarrinhoTests.cs`/`CatalogoTests.cs`, todas verdes), mas nenhum foi olhado ao vivo num navegador por um humano. Ver seção "Verificação manual pendente" abaixo.
- [x] Nada fora do escopo declarado entrou junto na entrega
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

## Constituição

- [x] **I** — Nenhuma referência nova entre projetos que inverta a direção de dependência (`ItemCarrinho` no `Domain`; contratos na `Application`; repositório na `Infrastructure`; sessão só na `MVC`)
- [x] **II** — `ItemCarrinho` tem `private set`, valida no construtor (`ValidarUsuario`/`ValidarProduto`/`ValidarQuantidade`) e tem `protected Ctor()`
- [x] **III** — Quantidade validada nas duas pontas (borda: `<input min max>`/saneamento no serviço; domínio: `ItemCarrinho.AlterarQuantidade`/`Acrescentar`). Desvio registrado e justificado no plano §10: sem `CarrinhoDTOValidator`, porque não há campo de texto livre nesta feature (só identificador impresso pelo sistema + número)
- [x] **IV** — Nomes, mensagens e comentários em português em toda a feature
- [x] **V** — Todas as fases seguiram vermelho → verde, confirmado fase a fase (ver `tasks.md`, T005/T025/T032/T040)
- [x] **VI** — Toda escrita passa por `IItemCarrinhoRepository` + `IUnitOfWork.SalvarAlteracoes`; migration `AddItemCarrinho` criada e aplicada
- [x] **VII** — `[ValidateAntiForgeryToken]` nas três ações de escrita de `CarrinhoController`; `await` em toda chamada assíncrona; POST-Redirect-Get no caminho comum
- [x] **VIII** — Sem `try/catch` em ação de controller; `FilterException` é o único lugar que traduz exceção em resposta HTTP

## Testes

- [x] `dotnet build` sem warnings novos (só o `NU1903` pré-existente do pacote `Microsoft.Data.Sqlite`, alheio a esta feature)
- [x] `dotnet test` verde — 475/475 unidade, 136/136 E2E (rodada limpa, do zero)
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] Feature que toca persistência tem teste de integração (`CarrinhoIntegrationTests.cs`, 8 testes contra SQLite em memória)

## Interface

- [x] `asp-action` de cada formulário aponta para uma ação que existe de fato (confirmado pelas suítes E2E, que exercitam cada envio de verdade)
- [ ] Erros de validação aparecem no campo (`asp-validation-for`) — **não aplicável**: nenhuma tela desta feature tem campo de texto livre (plano §10); a única entrada numérica é saneada silenciosamente (satura em 99, remove abaixo de 1), não rejeitada com mensagem de campo
- [ ] Testado em largura de tela pequena — **não verificado nesta execução**, ver seção abaixo
- [x] Valores monetários formatados em `pt-BR` (`R$ 8,70`, `ToString("N2")`, mesmo padrão do resto do sistema)

## Segurança

- [x] Nenhum segredo commitado
- [x] Entrada do usuário não é interpolada em HTML sem escape (Razor escapa por padrão; nenhum `Html.Raw` usado nesta feature)
- [x] Mensagens de erro não vazam existência de conta nem detalhe interno (mensagem de produto indisponível é genérica, sem detalhe de implementação)

---

## Verificação manual pendente

Automatizado (Playwright) cobre a funcionalidade dos 23 critérios de aceite —
todos verdes, suíte inteira rodada do zero mais de uma vez. O que **não** foi
verificado nesta execução, porque exige julgamento visual que só um navegador
com um humano olhando resolve:

- Aparência da tela do carrinho (`/Carrinho`) — espaçamento, alinhamento,
  cores, contra a referência visual do plano §3.
- A bolha do contador no cabeçalho — tamanho, posição, contraste, em cima do
  ícone do carrinho.
- O item indisponível sinalizado — se a mensagem e o esmaecimento comunicam
  bem "fora de estoque" vs. "saiu do catálogo" à primeira vista.
- O cartão do catálogo com os controles de quantidade/carrinho vivos ao lado
  do coração de favoritar — se os três não brigam por espaço nem confundem o
  olho.
- Largura de tela pequena (celular) em `/Carrinho`, no cartão e na página do
  produto.
- O fluxo do visitante inteiro ao vivo (T061): montar carrinho deslogado,
  entrar, ver as quantidades somarem, sair, e ver o carrinho avulso vazio —
  coberto por `CarrinhoTests.cs` (CA-12/13/14), mas não repetido manualmente.

Um smoke test rodou a aplicação de verdade (`dotnet run`) e confirmou `200 OK`
sem exceção em `/`, `/Catalogo` e `/Carrinho` — prova que as views renderizam
sem estourar, não que estejam bonitas ou usáveis.
