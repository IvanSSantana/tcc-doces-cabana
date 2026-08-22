# Tarefas — Busca e endereços do catálogo

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

---

## Convenções

- `T001`, `T002`... — ordem de execução. Uma tarefa por commit lógico.
- `[P]` — pode rodar em paralelo com as `[P]` vizinhas (arquivos distintos).
- Toda tarefa nomeia o arquivo exato que cria ou altera.
- Tarefa de implementação sempre vem **depois** da tarefa de teste que a cobre —
  e o teste precisa ter falhado antes (Princípio V).
- Marque `[x]` só depois de `dotnet test` verde.

> **Duas ordens não são negociáveis.**
>
> **T004 é a primeira tarefa de código, antes de qualquer arquivo de CSS ser
> tocado.** Ele guarda o estado atual das cinco telas de formulário lido do
> navegador. Escrito depois da extração, confirmaria o resultado da mudança em
> vez de guardar o que existia antes — e RF-18 deixaria de ser verificável.
>
> **A Fase 3 (normalização) vem antes da Fase 4 (esquema), que vem antes da
> Fase 5 (busca).** Cada uma depende do derivado da anterior existir. Falhar na
> Fase 5 com as duas anteriores verdes significa "a consulta está errada"; sem
> essa ordem, significa qualquer uma de três coisas.
>
> As Fases 2, 7 e 8 são independentes entre si e das demais — podem ser
> reordenadas se houver motivo.

---

## Fase 1 — Preparação e linha de base

- [ ] **T001** — Criar branch `016-busca-e-enderecos-do-catalogo` a partir de `main`. *(feita ao criar a pasta da spec)*
- [ ] **T002** — Rodar `dotnet build`, `dotnet test DocesCabana.Tests` e `dotnet test DocesCabana.Tests.E2E`; registrar o estado inicial (esperado: 376 e 93 verdes, herdados da `015`).
- [ ] **T003** — Subir a aplicação e **capturar as cinco telas de formulário** — `Autenticacao/Login`, `Cadastro`, `EsqueceuSenha`, `RedefinirSenha` e `Admin/Administrador/Cadastro` — mais o `Admin/Produto/Cadastro` como está hoje, a 1440px e a 375px. São a linha de base da Fase 7.
- [ ] **T004** — `DocesCabana.Tests.E2E/Fluxos/FormularioTests.cs` (criar): CA-19 — nas cinco telas acima, largura e altura do campo de texto, raio da borda, cor do rótulo e espaçamento entre campos, lidos do navegador. **Precisa ficar verde agora**, contra o código intocado: é o que ele guarda.

## Fase 2 — Endereço legível de subcategoria

- [ ] **T005** `[P]` — `DocesCabana.Tests/Units/Servicos/ApelidoTests.cs`: RN-03 — percorrer a taxonomia real e verificar, **categoria por categoria**, que os apelidos das subcategorias dela são distintos entre si. Ver falhar (o teste atual só cobre as quatro categorias).
- [ ] **T006** `[P]` — `DocesCabana.Tests/Units/Services/CatalogoServiceTests.cs`: apelido de subcategoria vira identificador antes de chegar ao repositório; apelido desconhecido na categoria é ignorado e o catálogo dela é montado inteiro (RN-04); mesmo apelido em categorias diferentes não se confunde (RN-03). Ver falhar.
- [ ] **T007** — Confirmar que T005 e T006 falham pelo motivo certo — o apelido de subcategoria não existe ainda —, e não por erro de compilação alheio.
- [ ] **T008** — `DocesCabana.Application/DTOs/SubcategoriaDTO.cs` e `Mappings/CategoriaMapper.cs`: subcategoria ganha apelido derivado do nome, como a categoria já tem desde a `012`.
- [ ] **T009** — `DocesCabana.Application/DTOs/CriteriosDoCatalogoDTO.cs` (criar): o que veio do endereço, sem identificador nenhum (plano §5). Ainda **sem** termo — ele entra na Fase 5.
- [ ] **T010** — `DocesCabana.Application/DTOs/CatalogoDTO.cs`: `SubcategoriasMarcadas` passa a carregar apelidos, que é o que a barra lateral e a paginação precisam para remontar o endereço.
- [ ] **T011** — `Contracts/Services/ICatalogoService.cs` e `Services/CatalogoService.cs`: `Montar` recebe `CriteriosDoCatalogoDTO` e resolve os apelidos contra a categoria atual, produzindo o `FiltroCatalogoDTO` de sempre. **O repositório não muda.**
- [ ] **T012** — `DocesCabana.MVC/Helpers/EnderecoDoCatalogo.cs` (criar): único lugar que monta endereço de catálogo — categoria, subcategorias, sem açúcar, ordenação, página. Recebe a página como sobreposição opcional.
- [ ] **T013** — `DocesCabana.MVC/Controllers/CatalogoController.cs`: `subcategorias` passa a ser texto; monta `CriteriosDoCatalogoDTO`.
- [ ] **T014** `[P]` — `DocesCabana.MVC/Views/Catalogo/_BarraLateral.cshtml`: as caixas passam a valer o apelido; a marcação continua sendo decidida por comparação com o que veio do endereço.
- [ ] **T015** `[P]` — `DocesCabana.MVC/Views/Catalogo/_Paginacao.cshtml`: passa a usar `EnderecoDoCatalogo`, em vez de montar a consulta à mão.
- [ ] **T016** `[P]` — `DocesCabana.MVC/Views/Shared/Components/Header/Default.cshtml`: o menu suspenso passa a apontar para o apelido da subcategoria (RF-15).
- [ ] **T017** — `DocesCabana.Tests.E2E/Fluxos/CatalogoTests.cs`: CA-12 a CA-16 — o endereço mostra o nome, duas subcategorias cabem, apelido desconhecido não quebra, "Cappuccino" das duas categorias não se confunde, o menu do cabeçalho leva ao endereço legível.
- [ ] **T018** — Rodar as duas suítes. Além do verde: **conferir que nenhum teste da `012`/`014`/`015` construía endereço com identificador à mão** — se algum construía, ele quebra aqui, e é correção de teste, não de aplicação.

## Fase 3 — Normalização de texto no domínio

- [ ] **T019** `[P]` — `DocesCabana.Tests/Units/Helpers/TextoHelperTests.cs` (criar): acento sai, caixa baixa, espaço das pontas some — com os nomes reais da loja (Café, Cachaça, Empório, Pelúcia). Ver falhar.
- [ ] **T020** `[P]` — `DocesCabana.Tests/Units/Entities/ProdutoTests.cs`: o nome normalizado nasce do nome no construtor e acompanha `AlterarNome`; nenhum caminho o deixa divergir. Ver falhar.
- [ ] **T021** — Confirmar que T019 e T020 falham por não existir o auxiliar nem o derivado.
- [ ] **T022** — `DocesCabana.Domain/Helpers/TextoHelper.cs` (criar): recebe o corpo que hoje está em `Apelido.RemoverAcentos`. Só BCL, ao lado de `CepHelper` e `CpfHelper` (Princípio I).
- [ ] **T023** — `DocesCabana.Application/Servicos/Apelido.cs`: passa a consumir `TextoHelper`. **É extração, não reescrita** — os testes de `Apelido` existentes têm de continuar verdes sem uma linha alterada.
- [ ] **T024** — `DocesCabana.Domain/Entities/Produto.cs`: `NomeNormalizado` com `private set`, derivado nos dois únicos pontos que mudam o nome.
- [ ] **T025** — Rodar `dotnet test DocesCabana.Tests`: T019 e T020 passam, e nada mais mudou.

## Fase 4 — Esquema e preenchimento retroativo

- [ ] **T026** — `DocesCabana.Tests/Integration/Repositories/CatalogoRepositoryIntegrationTests.cs`: produto gravado com o derivado vazio (como ficaria numa base anterior a esta migration) passa a ser encontrável depois do preenchimento retroativo. Ver falhar.
- [ ] **T027** — `DocesCabana.Infrastructure/DatabaseContext/Configurations/ProdutoConfiguration.cs`: mapeia o nome normalizado — obrigatório, 255, padrão vazio para as linhas que já existem.
- [ ] **T028** — Migration: `dotnet ef migrations add AddProdutoNomeNormalizado --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC`. Conferir o arquivo gerado antes de seguir.
- [ ] **T029** — `DocesCabana.MVC/Helpers/DbInitializer.cs`: rotina idempotente que preenche o derivado das linhas antigas via `AlterarNome(produto.Nome)` e fecha com `IUnitOfWork.SalvarAlteracoes` (plano §6). Numa base recém-criada não faz nada.
- [ ] **T030** — Rodar `dotnet test DocesCabana.Tests`: T026 passa. Depois, **apagar `docescabana.db` local, subir a aplicação e conferir que a base nasce correta** — e, num segundo teste, subir sobre a base antiga e conferir que ela foi corrigida.

## Fase 5 — Busca no repositório e na aplicação

- [ ] **T031** `[P]` — `DocesCabana.Tests/Integration/Repositories/CatalogoRepositoryIntegrationTests.cs`: encontra por acento trocado, por caixa trocada, por trecho no meio do nome; **não** encontra produto fora do catálogo público (RN-06); termo com `%` e `_` não vira curinga (plano §9). Ver falhar.
- [ ] **T032** `[P]` — `DocesCabana.Tests/Units/Services/CatalogoServiceTests.cs`: o termo chega ao repositório **já normalizado**, e termo vazio ou só espaços não vira filtro nenhum (RF-09). Ver falhar.
- [ ] **T033** `[P]` — `DocesCabana.Tests/Units/Controllers/CatalogoControllerTests.cs`: o termo do endereço chega ao serviço sem deformação. Ver falhar.
- [ ] **T034** — Confirmar que T031 a T033 falham por não existir filtro de termo.
- [ ] **T035** — `DocesCabana.Application/DTOs/FiltroCatalogoDTO.cs` e `CriteriosDoCatalogoDTO.cs`: o termo entra nos dois — cru no que veio do endereço, normalizado no que vai à consulta.
- [ ] **T036** — `DocesCabana.Infrastructure/Repositories/ProdutoRepository.cs`: um `Where` a mais em `ConstruirConsulta`, **dentro do mesmo método** que já compõe categoria, subcategoria e sem açúcar (RN-01).
- [ ] **T037** — `DocesCabana.Application/Services/CatalogoService.cs`: normaliza o termo e o repassa; `CatalogoDTO` passa a devolvê-lo para a tela reexibir.
- [ ] **T038** — `DocesCabana.MVC/Controllers/CatalogoController.cs` e `Helpers/EnderecoDoCatalogo.cs`: o termo entra no endereço e sobrevive a ele.
- [ ] **T039** — Rodar `dotnet test DocesCabana.Tests`: Fase 5 verde.

## Fase 6 — Busca na tela

- [ ] **T040** — `DocesCabana.Tests.E2E/Paginas/PaginaInicial.cs` e `Paginas/PaginaCatalogo.cs`: barra de pesquisa, etiqueta do termo e mensagem de vazio da busca.
- [ ] **T041** — `DocesCabana.Tests.E2E/Fluxos/BuscaTests.cs` (criar): CA-01 a CA-11. Ver falhar — hoje a caixa não pertence a formulário nenhum.
- [ ] **T042** — `DocesCabana.MVC/ViewComponents/Header.cs` e `Views/Shared/Components/Header/Default.cshtml`: a caixa vira `<form method="get">` para o catálogo e reexibe o termo vigente (RF-01, RF-06).
- [ ] **T043** — `DocesCabana.MVC/Views/Catalogo/Index.cshtml`: campo oculto do termo **dentro** de `#formulario-catalogo` — sem ele a submissão `GET` reescreve o endereço inteiro e o termo se perde (RF-05); e o item de resultado na trilha.
- [ ] **T044** `[P]` — `DocesCabana.MVC/Views/Catalogo/_BarraLateral.cshtml`: os links de categoria carregam o termo (RF-05).
- [ ] **T045** `[P]` — `DocesCabana.MVC/Views/Catalogo/_ResultadoCatalogo.cshtml`: etiqueta removível do termo (RF-07, um link, não script) e mensagem de vazio que menciona o termo em vez de aconselhar sobre filtros não usados (RF-08).
- [ ] **T046** `[P]` — `DocesCabana.MVC/wwwroot/css/pages/catalogo.css` e `components/header.css`: etiqueta do termo e a caixa de pesquisa dentro do formulário, **sem alterar a aparência em repouso** (plano §3).
- [ ] **T047** — Rodar `dotnet test DocesCabana.Tests.E2E`: Fase 6 verde. Conferir explicitamente que **`catalogo.js` não precisou de uma linha** — ele serializa o formulário, e o termo é um campo dele.

## Fase 7 — Estilização do cadastro de produto

- [ ] **T048** — `DocesCabana.MVC/wwwroot/css/components/formulario.css` (criar): as regras compartilhadas movidas de `pages/autenticacao.css`, **literalmente, sem alteração de valor**.
- [ ] **T049** — `DocesCabana.MVC/wwwroot/css/pages/autenticacao.css`: fica só com o que é da tela de login.
- [ ] **T050** — As seis telas de formulário passam a linkar `components/formulario.css`: `Views/Autenticacao/{Login,Cadastro,EsqueceuSenha,RedefinirSenha}.cshtml`, `Areas/Admin/Views/Administrador/Cadastro.cshtml` e `Areas/Admin/Views/Produto/Cadastro.cshtml`.
- [ ] **T051** — Rodar `dotnet test DocesCabana.Tests.E2E`: **T004 tem de continuar verde**. Se ele falhar aqui, a extração mudou alguma tela — corrigir a extração, nunca o teste.
- [ ] **T052** — `DocesCabana.Tests.E2E/Fluxos/CadastroDeProdutoTests.cs`: CA-17 e CA-18 — título e contenção, e nada transbordando a 375px. Ver falhar.
- [ ] **T053** — `DocesCabana.MVC/Areas/Admin/Views/Produto/Cadastro.cshtml`: contêiner, título e os pares em linha dupla — Preço + Status, Subcategoria + Sem açúcar (plano §3).
- [ ] **T054** — `DocesCabana.MVC/wwwroot/css/pages/cadastro_produto.css`: o arranjo próprio da tela; a regra da caixa de seleção da `012` continua.
- [ ] **T055** — Rodar `dotnet test DocesCabana.Tests.E2E`: Fase 7 verde, T004 incluído.

## Fase 8 — Renumeração da cadeia da loja

- [ ] **T056** — `grep -rn "spec 0[0-9][0-9]"` na base inteira — código, comentário, spec antiga, README — e corrigir toda referência que a renumeração tornou obsoleta. Inclui **esta spec e este plano**: foi exatamente a autorreferência que escapou da primeira vez.
- [ ] **T057** — `specs/README.md`: a cadeia passa a ser Estoque `017`, Carrinho `018`, Endereço do usuário `019`, Fechamento `020`, Pagamento `021`; a nota de numeração registra o quarto deslocamento.
- [ ] **T058** — Conferir os comentários de código que citam a cadeia por número — o `SanearOrdenacao` do `CatalogoController` cita a `019`, e os controles desabilitados do cartão citam o carrinho.

## Fase 9 — Fechamento

- [ ] **T059** — `dotnet build` sem aviso e as duas suítes verdes, do zero.
- [ ] **T060** — Subir a aplicação e percorrer **cada critério de aceite** no navegador. Especialmente os que nenhum teste alcança bem: a aparência da etiqueta do termo, o cadastro de produto lado a lado com o cadastro de administrador, e as cinco telas de formulário comparadas às capturas de T003.
- [ ] **T061** — Buscar de verdade por "cafe", "CACHAÇA" e "pelucia" na loja rodando — os três casos que motivaram a coluna normalizada.
- [ ] **T062** — Preencher `checklist.md`, registrando **o que foi provado por teste e o que só a verificação ao vivo mostrou**.
- [ ] **T063** — Atualizar o status da spec para *Implementada*, o do plano para *Executado*, e a linha da feature em `specs/README.md`, com o link do checklist.
- [ ] **T064** — Riscar do backlog de `specs/README.md` a linha "Busca por texto — o campo do cabeçalho segue sem função", que esta feature encerra.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T041, T042 |
| RF-02 | T032, T037, T041 |
| RF-03 | T019, T024, T031, T036 |
| RF-04 | T041, T043 |
| RF-05 | T038, T043, T044 |
| RF-06 | T042 |
| RF-07 | T045 |
| RF-08 | T045 |
| RF-09 | T032, T037 |
| RF-10 | T041, T045 |
| RF-11 | T031, T036 |
| RF-12 | T008, T013, T014 |
| RF-13 | T011, T012, T017 |
| RF-14 | T006, T011 |
| RF-15 | T014, T015, T016 |
| RF-16 | T050, T053, T054 |
| RF-17 | T052, T053 |
| RF-18 | T004, T048, T049, T051 |
| RN-01 | T036 |
| RN-02 | T019, T020, T024, T031 |
| RN-03 | T005, T008, T017 |
| RN-04 | T006, T011 |
| RN-05 | T038, T043, T044 |
| RN-06 | T031, T036 |
| CA-01 a CA-11 | T041 |
| CA-12 a CA-16 | T017 |
| CA-17, CA-18 | T052 |
| CA-19 | T004, T051, T055 |
