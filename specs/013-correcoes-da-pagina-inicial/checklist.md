# Checklist de conclusão — Correções da página inicial

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente — os 9 requisitos,
      verificados um a um contra `header.css`, `VitrineProdutos.cs` e
      `Views/Home/Index.cshtml`
- [x] Todo `CA-xx` foi verificado — CA-01 a CA-08, todos por teste E2E contra
      a aplicação rodando de verdade, mais captura de tela comparada à
      referência visual (T016)
- [x] Nada fora do escopo declarado entrou junto na entrega — nenhuma
      marcação mudou, nenhuma cor nova, nenhuma migration
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou — a pendência da
      seção 10 (ordem das categorias no cabeçalho) continua registrada como
      decisão em aberto do responsável pelo negócio, não indecisão técnica

## Constituição

- [x] **I** — nenhuma `ProjectReference` tocada
- [x] **II/III/VI/VII** — n/a: nenhuma entidade, nenhuma entrada de usuário,
      nenhuma persistência, nenhum `POST` nesta feature
- [x] **IV** — `limite` (parâmetro novo) em português; nenhuma classe CSS nova
- [x] **V** — Fase 2 vermelha antes de qualquer correção: `VitrineProdutosTests`
      falhava por `CS1501` (sem o parâmetro `limite`); `PaginaInicialTests`
      falhava com painel de 200px, fundo transparente e 8 pontos (não 5) —
      todos pelo motivo medido na spec, não por erro de teste
- [x] **VIII** — n/a: nenhum caminho de erro novo

## O que foi provado, e como

| Requisito | Prova |
|---|---|
| RF-01 (aba bege) | `Dado_MenuAberto_Quando_CompararFundos...` (E2E, cor computada) + captura de tela |
| RF-02 (largura do painel) | `Dado_MenuAberto_Quando_MedirOPainel...` (E2E, `BoundingBoxAsync` comparado à faixa) |
| RF-03 (cartão recuado) | `Dado_MenuAberto_Quando_CompararCartaoEPainel...` (E2E, quatro comparações de borda) |
| RF-04 (sem JavaScript) | Nenhuma marcação mudou — verificado por inspeção do diff; `:hover`/`:focus-within` inalterados |
| RF-05 (teclado) | `Dado_NavegacaoPorTeclado_Quando_OFocoChegaNaCategoria...` (E2E, `FocusAsync`) |
| RF-06/RF-07 (limite da vitrine) | `VitrineProdutosTests` (unidade, 3 casos: 99→8, 3→3, limite explícito) |
| RF-08 (um ponto por posição) | `Dado_PaginaInicial_Quando_ContarOsPontosVisiveis...` (E2E, filtra pontos com `display:none` via seletor `:visible`) |
| RF-09 (título) | `Dado_PaginaInicial_Quando_LerOTituloDaSecao...` (E2E) |
| CA-08 (375px) | E2E mede `scrollWidth` do `<main>`, não do documento — o estouro do cabeçalho a 375px é defeito pré-existente da `009`, fora de escopo, e continua lá (confirmado por captura de tela) |

## Achado durante a implementação, registrado aqui em vez de corrigido em silêncio

**`ToHaveCountAsync` conta elementos ocultos por `display:none`.** O primeiro
rodar de `Dado_PaginaInicial_Quando_ContarOsPontosVisiveis...` contra o código
já corrigido (vitrine limitada a 8) continuou vermelho — 8 pontos contados,
não 5 — porque o carrossel gera um `<button>` por produto e só esconde os
excedentes com `display:none` via `vitrine-produtos.js`, sem removê-los do
DOM. O seletor do teste passou a usar `:visible` (pseudo-classe própria do
motor de seleção do Playwright, não CSS padrão). Isso não é uma correção de
aplicação: o mecanismo já fazia o que RF-08 pede (visualmente, 5 bolinhas
aparecem), o teste é quem precisava enxergar do jeito certo.

## Verificado ao vivo (T016/T017), não só por teste

- Captura de tela do menu aberto a 1440px comparada lado a lado com a
  referência visual: aba bege presa ao painel, painel na largura da faixa,
  cartão recuado nos quatro lados — os três defeitos da spec §1 não aparecem
  mais.
- Altura da barra de navegação e posição do botão "Favoritos": o padding
  vertical que saiu da `section` (12px) foi devolvido integralmente ao
  `.link-nav` — mesmo valor, outro elemento. Comparação visual da barra antes
  e depois não mostra deslocamento; `.botao-favoritos` recebeu
  `align-self: center` explícito para não esticar com o novo
  `align-items: stretch` da `section`.
- Vitrine da página inicial: 8 cards, 5 pontos, título "Conheça a loja" —
  confirmado na captura de tela, não só no teste.
- 375px: o conteúdo da página inicial (vitrine, categorias) não estoura;
  o cabeçalho compartilhado continua estourando, como já registrado na `009`
  e mantido fora de escopo pela spec §8.

## Não verificado

- Viewports intermediários (768px–1024px) do menu suspenso não foram
  conferidos manualmente — o CSS não introduz breakpoint novo para essa
  faixa (o `@media (max-width: 900px)` que desliga o menu já existia), então
  o risco é baixo, mas não há captura de tela desses tamanhos.
