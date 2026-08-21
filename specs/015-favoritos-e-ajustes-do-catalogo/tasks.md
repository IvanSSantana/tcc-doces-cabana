# Tarefas — Favoritos e ajustes do catálogo

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

---

## Convenções

- `T001`, `T002`... — ordem de execução. Uma tarefa por commit lógico.
- `[P]` — pode rodar em paralelo com as `[P]` vizinhas (arquivos distintos).
- Toda tarefa nomeia o arquivo exato que cria ou altera.
- Tarefa de implementação sempre vem **depois** da tarefa de teste que a cobre —
  e o teste precisa ter falhado antes (Princípio V).
- Marque `[x]` só depois de `dotnet test` verde.

> **A ordem das fases 2 a 6 não é negociável.** O retorno no login (Fase 3) tem
> de existir antes da intenção do visitante (Fase 6), e o favoritar simples
> (Fase 4) antes dela também — senão uma falha na Fase 6 tem três causas
> possíveis e nenhuma forma de distinguir. As fases 7, 8 e 9 são independentes
> entre si e podem ser reordenadas.

---

## Fase 1 — Preparação

- [x] **T001** — Criar branch `015-favoritos-e-ajustes-do-catalogo` a partir de `main`.
- [x] **T002** — Rodar `dotnet build`, `dotnet test DocesCabana.Tests` e `dotnet test DocesCabana.Tests.E2E`; registrar o estado inicial (esperado: 361 e 73 verdes, herdados da `014`).
- [x] **T003** — Subir a aplicação e **capturar o cartão do catálogo e o cartão do carrossel, lado a lado**, a 1440px. São a linha de base: a Fase 7 tem de mudar o primeiro e o teste da `014` prova que o segundo não mudou (RF-17).

## Fase 2 — Favoritos: repositório e serviço

- [x] **T004** `[P]` — `DocesCabana.Tests/Units/Services/FavoritoServiceTests.cs` (criar): alternar liga e desliga o mesmo par (RN-01); produto inexistente é recusado; produto fora do catálogo público é recusado.
- [x] **T005** `[P]` — `DocesCabana.Tests/Integration/Repositories/FavoritoIntegrationTests.cs` (criar): a chave composta recusa o par repetido (RN-01); a lista de um usuário não traz favorito de outro (RN-02).
- [x] **T006** — Confirmar que T004 e T005 falham por não existir contrato nem serviço, e não por outro motivo.
- [x] **T007** — `DocesCabana.Application/Contracts/Repositories/IFavoritoRepository.cs` (criar): `BuscarPorUsuario`, `Buscar`, `IdsPorUsuario` (plano §5).
- [x] **T008** — `DocesCabana.Infrastructure/Repositories/FavoritoRepository.cs` (criar). `IdsPorUsuario` recebe os identificadores da página e responde **uma vez** — nunca uma consulta por cartão (plano §5).
- [x] **T009** — `DocesCabana.Application/Contracts/Services/IFavoritoService.cs` e `Services/FavoritoService.cs` (criar): regra do interruptor, recusa por `KeyNotFoundException`, commit por `IUnitOfWork.SalvarAlteracoes`.
- [x] **T010** — `DocesCabana.Infrastructure/DependencyInjections/ApplicationDependencyInjection.cs`: registrar repositório e serviço.
- [x] **T011** — Rodar `dotnet test DocesCabana.Tests`: T004 e T005 passam.

## Fase 3 — Retorno depois de entrar

- [x] **T012** — `DocesCabana.Tests/Units/Controllers/AutenticacaoControllerTests.cs`: endereço de retorno local é honrado (RF-13); endereço externo é descartado e cai na página inicial (RF-14, RN-04). Ver falhar.
- [x] **T013** — Confirmar que T012 falha porque `Login` redireciona sempre para a página inicial, e não por erro de compilação.
- [x] **T014** — `DocesCabana.MVC/Controllers/AutenticacaoController.cs`: `Login` (GET e POST) aceita `returnUrl` e só o honra se `Url.IsLocalUrl` aprovar.
- [x] **T015** — `DocesCabana.MVC/Views/Autenticacao/Login.cshtml`: campo oculto que devolve o `returnUrl` no envio, para ele sobreviver ao POST.
- [x] **T016** — Rodar as duas suítes: T012 passa e **nenhum fluxo de autenticação existente regride** — login, cadastro, logout e recuperação de senha passam pela mesma ação.

## Fase 4 — Marcar e desmarcar no cartão

- [x] **T017** — `DocesCabana.Tests/Units/Services/CatalogoServiceTests.cs`: com identificador de usuário, os produtos favoritados vêm marcados; sem identificador (visitante), nenhum vem marcado (RF-02). Ver falhar.
- [x] **T018** — Confirmar que T017 falha por `Montar` não receber usuário.
- [x] **T019** — `DocesCabana.Application/Mappings/ProdutoMapper.cs`: sobrecarga que recebe o conjunto de identificadores favoritados e preenche `EstaFavorito` — o campo existe desde a `012` e nunca foi preenchido.
- [x] **T020** — `Contracts/Services/ICatalogoService.cs` e `Services/CatalogoService.cs`: `Montar` passa a receber o identificador de quem vê, opcional.
- [x] **T021** — `DocesCabana.MVC/Controllers/CatalogoController.cs`: repassa o identificador do usuário autenticado, ou nulo para visitante.
- [x] **T022** — `DocesCabana.Tests.E2E/Paginas/PaginaCatalogo.cs` e `Fluxos/FavoritosTests.cs` (criar): CA-01 a CA-06 — marcar, desmarcar, sobreviver à recarga, não recarregar a página, funcionar sem JavaScript, e o controle visível no toque.
- [x] **T023** — Confirmar que T022 falha pelo motivo certo: o controle está desabilitado desde a `012`, nada é gravado.
- [x] **T024** — `DocesCabana.MVC/Controllers/FavoritoController.cs` (criar): `Alternar` em `[HttpPost]` com `[ValidateAntiForgeryToken]`, redirecionando no pedido comum e devolvendo o estado no assíncrono. **Sem `[Authorize]`** — a ação verifica autenticação por conta própria para poder responder 401 ao script (plano §8).
- [x] **T025** — `DocesCabana.MVC/Views/Shared/Components/CardProduto/Default.cshtml`: o coração deixa de ser botão morto e vira botão de envio dentro de um formulário com antiforgery.
- [x] **T026** — `DocesCabana.MVC/wwwroot/js/components/favorito.js` (criar): intercepta o envio, posta por `fetch` e troca o ícone no lugar (RF-03).
- [x] **T027** — `DocesCabana.MVC/wwwroot/css/pages/catalogo.css`: coração sobre a imagem; visível por passagem de mouse onde há ponteiro fino, **sempre visível onde não há** — `@media (hover: hover)`, não largura de tela (RF-05, plano §3).
- [x] **T028** — Rodar `dotnet test DocesCabana.Tests.E2E`: Fase 4 verde.

## Fase 5 — Lista de favoritos

- [x] **T029** — `DocesCabana.Tests.E2E/Paginas/PaginaFavoritos.cs` (criar) e `Fluxos/FavoritosTests.cs`: CA-09 a CA-13 — a lista mostra o que foi guardado, produto indisponível não aparece e volta se reativado, desfavoritar tira da lista, lista vazia convida, visitante não entra.
- [x] **T030** — Confirmar que T029 falha por não existir a tela.
- [x] **T031** — `DocesCabana.MVC/Controllers/FavoritoController.cs`: `Index` com `[Authorize]` (RF-12).
- [x] **T032** — `DocesCabana.MVC/Views/Favorito/Index.cshtml` (criar): grade dos favoritos e o estado vazio com caminho para o catálogo (RF-11).
- [x] **T033** — `DocesCabana.MVC/wwwroot/css/pages/favoritos.css` (criar).
- [x] **T034** — `favorito.js`: na lista, desfavoritar remove o cartão da grade na hora, e a grade vazia passa a mostrar o convite (RF-10).
- [x] **T035** — Rodar `dotnet test DocesCabana.Tests.E2E`: Fase 5 verde.

## Fase 6 — Intenção do visitante

*Só começa com as fases 3, 4 e 5 verdes — ver a nota do topo.*

- [x] **T036** — `DocesCabana.Tests.E2E/Fluxos/FavoritosTests.cs`: CA-07 e CA-08 — visitante é convidado a entrar sem nada ser gravado, e o produto pretendido fica favoritado depois do login.
- [x] **T037** — Confirmar que T036 falha pelo motivo certo, e que **CA-07 falha separado de CA-08** — se os dois falharem juntos, o convite é a causa e a intenção nem foi exercitada.
- [x] **T038** — `favorito.js`: sem autenticação, guardar o produto pretendido em `sessionStorage`, abrir o modal e acrescentar o endereço atual como retorno no atalho de entrar.
- [x] **T039** — `favorito.js`: ao carregar já autenticado, consumir a intenção pendente, favoritar e **limpar o registro** — intenção consumida não pode disparar de novo (plano §9).
- [x] **T040** — Rodar `dotnet test DocesCabana.Tests.E2E`: Fase 6 verde. Rodar **três vezes seguidas**: é o fluxo mais frágil da feature, com três navegações e estado no navegador.

## Fase 7 — Cartão do catálogo

- [x] **T041** — `DocesCabana.Tests.E2E/Fluxos/CatalogoTests.cs`: CA-16 e CA-17 — o arranjo da referência (imagem com fundo próprio, preço e seletor na mesma linha, botão largo na base) e o nome em caixa normal.
- [x] **T042** — Confirmar que T041 falha com o arranjo atual, medido em T003.
- [x] **T043** — `Views/Shared/Components/CardProduto/Default.cshtml` e `wwwroot/css/components/card-produto.css`: tirar `ToUpper()` da view e pôr `text-transform: uppercase` na base. **Apresentação sai da marcação**; o carrossel continua idêntico porque herda a base (RF-16, RF-17).
- [x] **T044** — `DocesCabana.MVC/ViewComponents/CardProduto.cs` e a view: parâmetro do rótulo do botão, para o catálogo dizer "Adicionar ao carrinho" e o carrossel seguir com "Adicionar" no seu cartão estreito.
- [x] **T045** — `wwwroot/css/pages/catalogo.css`: `display: contents` no bloco de ações e o cartão como grade — imagem e nome atravessando, preço e seletor dividindo a linha, botão largo na base; fundo próprio atrás da imagem e borda visível em repouso. **Toda regra escopada sob a grade do catálogo** (plano §9, risco 2).
- [x] **T046** — Conferir contra T003: o cartão do catálogo mudou, **o do carrossel não**. O teste de não-regressão da `014` (CA-18) tem de continuar verde.

## Fase 8 — Trilha e revelar subcategorias

- [x] **T047** — `DocesCabana.Tests.E2E/Fluxos/CatalogoTests.cs`: CA-20 a CA-22 — trilha em caixa alta com o último item destacado; controle indo para o fim da lista e oferecendo recolher; tudo isso com JavaScript desligado.
- [x] **T048** — Confirmar que T047 falha pelo motivo certo.
- [x] **T049** — `Views/Catalogo/Index.cshtml` e `wwwroot/css/pages/catalogo.css`: caixa alta na trilha e cor de destaque no item mais à direita (RF-19, RF-20).
- [x] **T050** — `Views/Catalogo/_BarraLateral.cshtml`: dois rótulos no controle de revelar — "Ver todas" e "Ver menos" —, ambos na marcação, para o alternar não depender de script (RF-22, RF-23).
- [x] **T051** — `wwwroot/css/pages/catalogo.css`: o bloco vira coluna flexível e o controle recebe `order: 1`, o que o desloca para depois das subcategorias reveladas; o rótulo alterna pelo estado `[open]` (RF-21).
- [x] **T052** — Rodar `dotnet test DocesCabana.Tests.E2E`: Fase 8 verde, **incluindo o caminho sem JavaScript**.

## Fase 9 — Pendências encontradas

- [x] **T053** — `DocesCabana.Tests.E2E/Fluxos/`: CA-23 e CA-24 — nenhuma requisição termina em 404 ao percorrer as telas; o administrador alcança o cadastro de produto navegando. Ver falhar.
- [x] **T054** — Confirmar que T053 falha: hoje `~/js/modal-login.js` responde 404 em toda página, e não há caminho até o cadastro de produto.
- [x] **T055** — `Views/Shared/Components/Header/Default.cshtml`: remover o `<script>` para o arquivo inexistente e o `<dialog id="dropdown-header">` vazio (RF-24, RF-25). A função `abrirModal` vem de `~/js/components/modal-login.js`, que o layout já carrega — **confirmar que o modal continua abrindo** depois da remoção.
- [x] **T056** — `Views/Shared/Components/Header/Default.cshtml`: atalho para o cadastro de produto ao lado de "Administradores", visível só para quem tem o papel (RF-26).
- [x] **T057** — `Views/Shared/Components/VitrineProdutos/Default.cshtml`: parar de passar `estaFavorito`, que o componente não recebe desde sempre (RF-27).
- [x] **T058** — Rodar `dotnet test DocesCabana.Tests.E2E`: Fase 9 verde.

## Fase 10 — Verificação ao vivo

- [x] **T059** — Percorrer favoritos à mão: favoritar do catálogo, abrir a lista, desfavoritar de lá, esvaziar a lista, e favoritar como visitante passando pelo login.
- [x] **T060** — **Comparar captura do cartão com a referência visual, lado a lado.** Os testes provam arranjo; só o olho prova semelhança.
- [x] **T061** — Desligar o JavaScript no navegador de verdade e repetir: favoritar, desfavoritar, revelar e recolher subcategorias (CA-05, CA-22).
- [x] **T062** — Percorrer o catálogo só com o teclado: alcançar o coração, acioná-lo, e conferir que o foco não é jogado para o começo do documento — o mesmo cuidado que a RF-18 da `014` estabeleceu.
- [x] **T063** — Conferir a 375px: cartão, coração, trilha, controle de revelar e lista de favoritos sem rolagem horizontal no conteúdo.

## Fase 11 — Fechamento

- [x] **T064** — `dotnet build` sem warnings novos; as duas suítes verdes.
- [x] **T065** — Preencher `checklist.md`: o que ficou provado por teste, o que por comparação visual, o que não foi verificado, e **quais testes existentes mudaram de premissa**.
- [x] **T066** — Atualizar o status da spec para *Implementada*, o do plano para *Executado*, a linha em `specs/README.md`, e **a renumeração da cadeia da loja para `016` estoque, `017` carrinho, `018` endereço, `019` fechamento, `020` pagamento** — varrendo `spec 0NN` e `` `0NN` `` na base inteira, **inclusive nesta spec**, que foi o que escapou na `013`. Fechar no backlog o item "Lista de favoritos".

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T004, T009, T022, T024, T025 |
| RF-02 | T017, T019, T020, T021 |
| RF-03 | T022, T026 |
| RF-04 | T022, T024, T061 |
| RF-05 | T022, T027, T063 |
| RF-06 | T036, T038 |
| RF-07 | T036, T038, T039 |
| RF-08 | T029, T031, T032 |
| RF-09 | T029, T009 |
| RF-10 | T029, T034 |
| RF-11 | T029, T032 |
| RF-12 | T029, T031 |
| RF-13 | T012, T014, T015 |
| RF-14 | T012, T014 |
| RF-15 | T041, T045, T060 |
| RF-16 | T041, T043 |
| RF-17 | T003, T043, T046 |
| RF-18 | T045 |
| RF-19 | T047, T049 |
| RF-20 | T047, T049 |
| RF-21 | T047, T051 |
| RF-22 | T047, T050 |
| RF-23 | T047, T051, T061 |
| RF-24 | T053, T055 |
| RF-25 | T053, T055 |
| RF-26 | T053, T056 |
| RF-27 | T057 |
| RN-01 | T004, T005, T009 |
| RN-02 | T005, T031 |
| RN-03 | T029, T009 |
| RN-04 | T012, T014 |
| RN-05 | T045 |
| CA-01 | T022, T024 |
| CA-02 | T022, T024 |
| CA-03 | T022, T019 |
| CA-04 | T022, T026 |
| CA-05 | T022, T024, T061 |
| CA-06 | T022, T027 |
| CA-07 | T036, T038 |
| CA-08 | T036, T039, T040 |
| CA-09 | T029, T031 |
| CA-10 | T029, T009 |
| CA-11 | T029, T034 |
| CA-12 | T029, T032 |
| CA-13 | T029, T031 |
| CA-14 | T012, T014, T015 |
| CA-15 | T012, T014 |
| CA-16 | T041, T045, T060 |
| CA-17 | T041, T043 |
| CA-18 | T046 |
| CA-19 | T045 |
| CA-20 | T047, T049 |
| CA-21 | T047, T050, T051 |
| CA-22 | T047, T051, T061 |
| CA-23 | T053, T055 |
| CA-24 | T053, T056 |
