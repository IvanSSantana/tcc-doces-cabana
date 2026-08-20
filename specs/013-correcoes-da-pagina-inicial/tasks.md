# Tarefas — Correções da página inicial

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

---

## Convenções

- `T001`, `T002`... — ordem de execução. Uma tarefa por commit lógico.
- `[P]` — pode rodar em paralelo com as `[P]` vizinhas (arquivos distintos).
- Toda tarefa nomeia o arquivo exato que cria ou altera.
- Tarefa de implementação sempre vem **depois** da tarefa de teste que a cobre —
  e o teste precisa ter falhado antes (Princípio V).
- Marque `[x]` só depois de `dotnet test` verde.

> **Correção, não feature.** Sem Domínio, Aplicação, Infraestrutura nem
> migration. Duas correções independentes (menu e vitrine) que só dividem a
> página; podem ser feitas e conferidas em qualquer ordem depois da Fase 2.

---

## Fase 1 — Preparação

- [ ] **T001** — Criar branch `013-correcoes-da-pagina-inicial` a partir de `main`.
- [ ] **T002** — Rodar `dotnet build`, `dotnet test DocesCabana.Tests` e `dotnet test DocesCabana.Tests.E2E`; registrar o estado inicial (esperado: 346 e 49 verdes, herdados da `012`).
- [ ] **T003** — Subir a aplicação e **registrar a altura da barra verde inferior e a posição do botão "Favoritos"** antes de qualquer mudança. São os dois números que o plano §9 aponta como risco de deslocamento; sem a medida de antes, não há como provar que não mudaram.

## Fase 2 — Testes (devem falhar)

*Estes testes medem o estado defeituoso atual. Rode e veja vermelho — se algum
passar agora, a medição está errada, não o código.*

- [ ] **T004** `[P]` — `DocesCabana.Tests/Units/ViewComponents/VitrineProdutosTests.cs` (criar): o componente devolve no máximo `limite` produtos, e devolve todos quando a lista é menor que o limite (RF-06, RF-07).
- [ ] **T005** `[P]` — `DocesCabana.Tests.E2E/Paginas/PaginaInicial.cs` (criar): objeto de página com `Abrir`, `TituloDaVitrine`, `CardsDaVitrine`, `PontosVisiveis`, `AbrirMenuDaCategoria`, `PainelDoMenu`, `CartaoDoMenu`, `CategoriaAberta`, `FaixaDeConteudo`.
- [ ] **T006** — `DocesCabana.Tests.E2E/Fluxos/PaginaInicialTests.cs` (criar): CA-01 a CA-08 do mapeamento do plano §7. CA-02 compara a largura resolvida do painel com a da faixa; CA-01 compara cores de fundo computadas; CA-06 conta pontos **visíveis**, não renderizados.
- [ ] **T007** — Confirmar que T004 e T006 falham pelo motivo certo — largura de 200 onde se espera 1400, fundo transparente onde se espera bege, 96 pontos onde se esperam 5, título "Mais Vendidos" — e não por erro de compilação ou seletor errado.

## Fase 3 — Menu suspenso

- [ ] **T008** — `DocesCabana.MVC/wwwroot/css/components/header.css`: mover o contexto de posicionamento — `.cabecalho-inferior section` passa a `position: relative` e `.item-categoria-nav` a `position: static`; o painel estica com `left: 0; right: 0` (RF-02).
- [ ] **T009** — `header.css`: fazer o item de categoria ocupar a altura da barra — tirar o espaçamento vertical da `section` e devolvê-lo dentro do `.link-nav`, com `align-items: stretch` na `section` e no `nav`. **`align-self: center` explícito no `.botao-favoritos`** (plano §9, risco 1).
- [ ] **T010** — `header.css`: fundo bege no item aberto (`:hover`/`:focus-within`), o mesmo do painel, encostando na base da barra (RF-01).
- [ ] **T011** — `header.css`: recuar o cartão coral dentro do painel, com folga nos quatro lados (RF-03).
- [ ] **T012** — Conferir contra T003: **a altura da barra e a posição do botão "Favoritos" não mudaram** (plano §9, riscos 1 e 2).

## Fase 4 — Vitrine

- [ ] **T013** — `DocesCabana.MVC/ViewComponents/VitrineProdutos.cs`: parâmetro `limite` com padrão 8, aplicado dentro do componente (RF-06, RF-07).
- [ ] **T014** `[P]` — `DocesCabana.MVC/Views/Home/Index.cshtml`: título "Mais Vendidos" → "Conheça a loja" (RF-09).
- [ ] **T015** — Rodar `dotnet test DocesCabana.Tests`: T004 passa.

## Fase 5 — Verificação ao vivo

- [ ] **T016** — Subir a aplicação e **comparar captura do menu aberto com a referência visual original, lado a lado** (plano §7). Os testes provam número; só o olho prova semelhança. Conferir os quatro pontos da tabela do plano §3.
- [ ] **T017** — Conferir ao vivo o resto: vitrine com 8 cards e 5 pontos no desktop (CA-05, CA-06); título novo (CA-07); menu abrindo por teclado (CA-04); 375px sem rolagem horizontal (CA-08).
- [ ] **T018** — Rodar `dotnet test DocesCabana.Tests.E2E` inteiro: verde, incluindo os fluxos da `007` a `012` que passam pelo cabeçalho — ele foi alterado aqui e aparece em toda tela.

## Fase 6 — Fechamento

- [ ] **T019** — `dotnet build` sem warnings novos; as duas suítes verdes.
- [ ] **T020** — Preencher o checklist em `checklist.md`, registrando o que ficou provado por teste, o que por comparação visual, e o que não foi verificado.
- [ ] **T021** — Atualizar o status da spec para *Implementada*, o do plano para *Executado*, e a linha da feature em `specs/README.md` — **incluindo a renumeração da cadeia da loja**, que passa a ser `014` estoque, `015` carrinho, `016` endereço, `017` fechamento, `018` pagamento.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T006, T010 |
| RF-02 | T006, T008 |
| RF-03 | T006, T011 |
| RF-04 | T006, T017 |
| RF-05 | T006, T017 |
| RF-06 | T004, T013 |
| RF-07 | T004, T013 |
| RF-08 | T006, T013 |
| RF-09 | T006, T014 |
| RN-01 | T006, T013 |
| RN-02 | T014 |
| CA-01 | T006, T010, T016 |
| CA-02 | T006, T008, T016 |
| CA-03 | T006, T011, T016 |
| CA-04 | T006, T017 |
| CA-05 | T006, T013, T017 |
| CA-06 | T006, T013, T017 |
| CA-07 | T006, T014, T017 |
| CA-08 | T006, T017 |
