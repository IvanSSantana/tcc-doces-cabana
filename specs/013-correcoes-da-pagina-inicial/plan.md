# Plano Técnico — Correções da página inicial

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-19
**Status:** Rascunho

---

## 1. Resumo da abordagem

Duas correções independentes, que só se encontram na mesma página.

O menu suspenso é correção de CSS, sem tocar em marcação: o painel não
consegue atravessar a faixa porque o contexto de posicionamento é o próprio
item de categoria, que mede 75px. Movendo o contexto para a `section` da faixa
— que mede a largura do conteúdo — o painel passa a poder esticar com
`left: 0; right: 0`. A aba bege exige que o item de categoria ocupe a altura
inteira da barra, o que hoje não acontece porque o espaçamento vertical está na
`section` e não no link; mover esse espaçamento para dentro do link faz o item
encostar no painel.

A vitrine é correção de entrada, não de contagem: a view gera um ponto por
produto e o JavaScript esconde os que passam da última posição alcançável — o
mecanismo está certo, o que está errado é receber 99 produtos. Limitando a
entrada no próprio *view component*, a quantidade de pontos volta a ser
contável sem tocar no JavaScript. O título da seção muda por decisão de
negócio, registrada na spec §10.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ⬜ OK | Nenhuma `ProjectReference` tocada; tudo em `DocesCabana.MVC` e nos testes |
| II | Domínio rico e auto-validante | n/a | Nenhuma entidade tocada |
| III | Validação nas duas barreiras | n/a | Nenhuma entrada de usuário. O limite da vitrine é regra de apresentação, não validação de dado |
| IV | Nomenclatura em português | ⬜ OK | Classes CSS e parâmetro novo em português (`limite`) |
| V | Testes escritos antes | ⬜ OK | Fase 2 vermelha antes de qualquer correção — os testes medem o estado defeituoso atual e falham nele |
| VI | Repositório + commit via UnitOfWork | n/a | Nenhuma persistência tocada, nenhuma migration |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | n/a | Nenhum `POST`, nenhuma ação nova |
| VIII | Tratamento de erro por camada | n/a | Nenhum caminho de erro novo |

## 3. Direção visual

A referência visual manda; esta feature existe justamente porque a
implementação se afastou dela. Abaixo, a leitura da referência em medidas
verificáveis, ao lado do que existe hoje (medido no navegador, viewport 1440).

```
REFERÊNCIA                                    ATUAL
┌─────────────────────────────────────┐      ┌─────────────────────────────────────┐
│ ▛▀▀▀▀▜                              │      │                                     │
│ ▌Doces▐ Empório  Adega  Souvenir    │verde │  Doces  Empório  Adega  Souvenir    │verde
├─┴─────┴─────────────────────────────┤      ├──────┬──────┐                       │
│                                     │      │      │coral │                       │
│   ┌──────────┐                      │bege  │ bege │cartão│                       │
│   │  coral   │                      │      │      │      │                       │
│   │  cartão  │                      │      │      └──────┘                       │
│   └──────────┘                      │      └──────────────┘                      │
└─────────────────────────────────────┘                                            │
   painel = largura do conteúdo               painel = 200px (largura do item)
   aba bege encostada no painel               categoria sem fundo, solta da barra
   cartão recuado, folga nos 4 lados          cartão preenchendo o painel
```

| Medida | Referência | Atual |
|---|---|---|
| Largura do painel | igual à faixa de conteúdo (1400) | 200 |
| Fundo da categoria aberta | bege, igual ao painel | `rgba(0, 0, 0, 0)` |
| Folga do cartão dentro do painel | visível nos quatro lados | 12px, cartão quase preenche |
| Topo do painel | encostado na base da barra verde | 12px acima da base da barra |

Nenhuma cor nova entra: a aba usa o mesmo bege que o painel já usa, e o cartão
segue coral. Nenhuma animação nova — o menu já abre com a transição que tem.

## 4. Impacto por camada

### `DocesCabana.Domain`, `DocesCabana.Application`, `DocesCabana.Infrastructure`

Nenhum arquivo.

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `wwwroot/css/components/header.css` | alterar | Contexto de posicionamento sobe para `.cabecalho-inferior section`; painel estica com `left: 0; right: 0`; item de categoria ocupa a altura da barra e ganha fundo bege quando aberto; cartão recuado dentro do painel |
| `ViewComponents/VitrineProdutos.cs` | alterar | Parâmetro `limite` (padrão 8) aplicado dentro do componente — RF-07 exige que o corte valha para qualquer chamador, não só para a página inicial |
| `Views/Home/Index.cshtml` | alterar | Título da seção: "Mais Vendidos" → "Conheça a loja" (spec §10) |

**Nenhuma marcação muda.** O painel continua sendo filho do item de categoria,
o que é o que faz `:hover`/`:focus-within` funcionarem sem JavaScript (RF-04) —
mudar o contexto de posicionamento não exige mudar de lugar no DOM.

### `DocesCabana.Tests` / `DocesCabana.Tests.E2E`

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/ViewComponents/VitrineProdutosTests.cs` | **criar** | O componente corta a lista no limite, qualquer que seja a entrada |
| `E2E/Paginas/PaginaInicial.cs` | **criar** | Objeto de página: cards da vitrine, pontos visíveis, título da seção |
| `E2E/Fluxos/PaginaInicialTests.cs` | **criar** | CA-01 a CA-08 |

## 5. Contratos

```csharp
// Alteração de assinatura — o limite tem padrão, então os chamadores atuais
// continuam compilando sem mudança.
public IViewComponentResult Invoke(IEnumerable<ProdutoDTO> produtos, int limite = 8);
```

Nenhuma interface de `Application` muda.

## 6. Modelo de dados

Não se aplica. Nenhuma mudança de esquema, nenhuma migration.

## 7. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — componente | `Units/ViewComponents/VitrineProdutosTests.cs` | RF-06, RF-07 — o corte acontece dentro do componente |
| E2E — geometria | `E2E/Fluxos/PaginaInicialTests.cs` | RF-01 a RF-03, medindo o navegador de verdade: só ele sabe a largura resolvida do painel e a cor de fundo computada |
| E2E — comportamento | idem | RF-04, RF-05, RF-08, RF-09 |

Mapeamento critério → teste:

| Critério | Teste que o prova |
|---|---|
| CA-01 | `Dado_MenuAberto_Quando_CompararFundos_Entao_CategoriaDeveTerOMesmoFundoDoPainel` |
| CA-02 | `Dado_MenuAberto_Quando_MedirOPainel_Entao_DeveTerALarguraDaFaixaDeConteudo` |
| CA-03 | `Dado_MenuAberto_Quando_CompararCartaoEPainel_Entao_CartaoDeveEstarRecuadoNosQuatroLados` |
| CA-04 | `Dado_NavegacaoPorTeclado_Quando_OFocoChegaNaCategoria_Entao_OMenuDeveAbrir` |
| CA-05 | `Dado_CatalogoComDezenasDeProdutos_Quando_AbrirAPaginaInicial_Entao_AVitrineDeveRespeitarOLimite` |
| CA-06 | `Dado_PaginaInicial_Quando_ContarOsPontosVisiveis_Entao_DeveHaverUmPorPosicaoAlcancavel` |
| CA-07 | `Dado_PaginaInicial_Quando_LerOTituloDaSecao_Entao_NaoDeveDizerMaisVendidos` |
| CA-08 | `Dado_TelaDe375px_Quando_AbrirAPaginaInicial_Entao_NaoDeveHaverRolagemHorizontal` |

CA-08 mede o conteúdo da página inicial, não o documento — o cabeçalho
compartilhado estoura a 375px por conta própria desde antes desta feature
(achado registrado no checklist da `009`), e essa correção está fora de escopo.

**Verificação por captura de tela é obrigatória nesta feature**, não opcional:
os testes provam largura, cor e contenção, mas nenhum deles prova que o
resultado *se parece* com a referência. A tarefa de verificação ao vivo compara
captura contra o desenho original lado a lado.

## 8. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Mover o painel para fora do item no DOM, como filho da `section` | Quebraria `:hover`/`:focus-within`, que dependem do painel ser descendente do item — e devolveria o menu à dependência de JavaScript que a `012` evitou de propósito |
| Largura fixa em pixels no painel | Casaria com a referência num viewport e erraria em todos os outros. `left: 0; right: 0` sobre o contexto certo acompanha a faixa sozinho |
| Gerar os pontos em JavaScript, a partir das posições alcançáveis | Seria o modelo mais correto — mas o mecanismo atual (renderizar N e esconder os que sobram) já produz a contagem certa assim que a entrada é limitada, e trocar o mecanismo é reescrita de um componente que não é o defeito |
| Limitar a vitrine no `HomeController` em vez de no componente | RF-07 pede que o corte valha para qualquer chamador. No controlador, a próxima página que usar a vitrine repete o defeito |
| Manter "Mais Vendidos" e ordenar por nota média | O título continuaria afirmando venda enquanto o dado é avaliação. Foi oferecido ao responsável e descartado (spec §10) |

## 9. Riscos

| Risco | Prob. | Impacto | Mitigação |
|---|---|---|---|
| **Mover o espaçamento vertical da `section` para o link desloca o resto da barra.** O botão "Favoritos" é irmão do `nav` na mesma `section` e hoje depende do alinhamento central dela | Alta | Médio | `align-self: center` explícito no botão; captura de tela comparada antes e depois é tarefa própria, não conferência de passagem |
| **A altura da barra muda.** Tirar `padding` da `section` e devolver no link precisa dar exatamente a mesma altura, senão o cabeçalho inteiro se desloca e o carrossel abaixo pula | Alta | Médio | Medir a altura da barra antes da correção e conferir depois; o teste de geometria registra o número |
| **Algum ancestral com `position` inesperado captura o painel** e ele estica para a largura errada | Baixa | Alto | O teste de CA-02 mede a largura resolvida no navegador, não a regra CSS — se o contexto for outro, ele falha |
| **`overflow` no cabeçalho corta o painel esticado** | Baixa | Alto | CA-03 falha se o cartão não estiver visível dentro do painel; a captura de tela mostra o corte |
| **O limite de 8 esconde produtos que alguém esperava ver** na página inicial | Certa (é o objetivo) | Baixo | É requisito (RF-06). O catálogo completo continua a um clique, pelo cabeçalho ou pelo bloco de categorias |

## 10. Desvios constitucionais justificados

*Nenhum.*
