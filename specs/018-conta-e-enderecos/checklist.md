# Checklist de conclusão — Conta e endereços

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente (ver tabela de Rastreabilidade em `tasks.md`)
- [ ] Todo `CA-xx` foi verificado manualmente na aplicação rodando — **não verificado nesta execução**: todos os 21 CA têm cobertura E2E automatizada (Playwright, `ContaTests.cs`/`CatalogoTests.cs`, todas verdes), mas nenhum foi olhado ao vivo num navegador por um humano. Ver seção "Verificação manual pendente" abaixo — **CA-18 em especial nunca foi provado contra o ViaCEP de verdade**, só contra a interceptação de rota dos testes.
- [x] Nada fora do escopo declarado entrou junto na entrega
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

## Constituição

- [x] **I** — Nenhuma referência nova entre projetos que inverta a direção de dependência (`IEnderecoService` na `Application`; `EnderecoRepository` na `Infrastructure`; `ContaController` conhece `IUsuarioService`, mesma exceção documentada que `AutenticacaoController` já usa)
- [x] **II** — `Endereco` ganhou `Padrao`/`DataCadastro` com `private set`, e `AtualizarDados`/`MarcarComoPadrao`/`DesmarcarComoPadrao` como métodos de intenção — nenhuma atribuição direta
- [x] **III** — `EnderecoDTOValidator`/`DadosPessoaisDTOValidator` protegem o usuário; construtor e `AtualizarDados` de `Endereco`, e `AtualizarDados` de `Usuario`, protegem o dado. Diferente da `017`, aqui os dois formulários têm campo de texto de verdade — o princípio se aplica inteiro, sem ressalva (plano §10)
- [x] **IV** — Nomes, mensagens e comentários em português em toda a feature
- [x] **V** — Todas as fases seguiram vermelho → verde, confirmado fase a fase (ver `tasks.md`, T006/T013/T021/T027/T032/T045)
- [x] **VI** — Toda escrita passa por `IEnderecoRepository`/`IUsuarioRepository` + `IUnitOfWork.SalvarAlteracoes`; migration `AddEnderecoPadraoEDataCadastro` criada e aplicada
- [x] **VII** — `[ValidateAntiForgeryToken]` em todas as ações de escrita de `ContaController`; `await` em toda chamada assíncrona; `[Authorize]` na classe (RF-03); POST-Redirect-Get no sucesso de todas as escritas
- [x] **VIII** — Sem `try/catch` em ação de controller; endereço alheio propaga `KeyNotFoundException`, que só o `FilterException` global traduz

## Testes

- [x] `dotnet build` sem warnings novos (só o `NU1903` pré-existente do pacote `Microsoft.Data.Sqlite`, alheio a esta feature)
- [x] `dotnet test` verde — 534/534 unidade, 157/157 E2E (rodada limpa, do zero)
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] Feature que toca persistência tem teste de integração (`EnderecoIntegrationTests.cs`, 5 testes contra SQLite em memória)

## Interface

- [x] `asp-action` de cada formulário aponta para uma ação que existe de fato (confirmado pelas suítes E2E, que exercitam cada envio de verdade)
- [x] Erros de validação aparecem no campo (`asp-validation-for`) — provado por `Dado_CelularInvalido_Quando_Salvar_...` (CA-07)
- [ ] Testado em largura de tela pequena — **não verificado nesta execução**, ver seção abaixo
- [x] Datas formatadas em `pt-BR` (`dd/MM/yyyy`, cultura fixa do `Program.cs`; `[DisplayFormat]` acrescentado a `DadosPessoaisDTO.DataNascimento` — achado registrado abaixo)

## Segurança

- [x] Nenhum segredo commitado
- [x] Entrada do usuário não é interpolada em HTML sem escape (Razor escapa por padrão; nenhum `Html.Raw` usado nesta feature)
- [x] Mensagens de erro não vazam existência de conta nem detalhe interno (endereço alheio devolve 404 genérico, não "existe mas não é seu")

---

## Achados corrigidos durante a execução (não estavam no plano)

Três bugs reais, encontrados pelos próprios testes E2E, corrigidos na aplicação
(não nos testes, exceto o terceiro):

1. **`DadosPessoaisDTO.DataNascimento` sem `[DisplayFormat]`** fazia o Input
   Tag Helper renderizar a data com hora (`06/06/1994 00:00:00`) ao
   pré-preencher o formulário de dados pessoais — só apareceu aqui porque,
   diferente do cadastro (`004`), este campo nasce com valor.
2. **`DadosPessoaisDTO.CPF`** (string não anulável, nunca postado pelo
   formulário) era tratado como implicitamente `[Required]` pelo ASP.NET
   Core, invalidando o `ModelState` em silêncio — sem span nenhum para
   mostrar o erro, porque CPF não é um `<input>`. Corrigido com
   `[ValidateNever]`.
3. **O teste de "corrigir celular"** esperava o valor formatado de volta,
   mas `Usuario.AtualizarDados` grava só os dígitos (mesma convenção do
   CPF) — ajustado o teste, não a aplicação.

## Verificação manual pendente

Automatizado (Playwright) cobre a funcionalidade dos 21 critérios de aceite —
todos verdes, suíte inteira rodada do zero. O que **não** foi verificado
nesta execução, porque exige julgamento visual ou uma dependência externa
real que só um navegador com um humano resolve:

- Aparência do menu lateral da conta, dos cartões de endereço e do CPF como
  texto ao lado dos campos editáveis — contra a referência visual do plano
  §3.
- Largura de tela pequena (celular) em `/Conta`, `/Conta/Enderecos` e no
  formulário de endereço.
- **A busca por CEP contra o ViaCEP de verdade** (T054) — os testes
  automatizados interceptam a rota de propósito (plano §7), então nenhum
  deles prova que a integração real funciona hoje. Esta é a única lacuna
  desta lista que os testes automatizados **não podem** fechar, por desenho.
- O ciclo inteiro do endereço principal ao vivo (T053): cadastrar dois,
  trocar qual é o principal, excluir o principal e ver o outro assumir,
  excluir o último e ver o convite voltar — coberto por `ContaTests.cs`
  (CA-08 a CA-16), mas não repetido manualmente num navegador.

Um smoke test rodou a aplicação de verdade (`dotnet run`) e confirmou que
`/Conta` devolve `302` (desafio de login) para um visitante anônimo — prova
que a rota e o `[Authorize]` funcionam, não que a tela esteja bonita ou
usável.
