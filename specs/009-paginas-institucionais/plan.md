# Plano Técnico — Páginas institucionais

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-17
**Status:** Rascunho

---

## 1. Resumo da abordagem

Esta é a primeira feature do projeto que vive inteiramente na `DocesCabana.MVC`:
não há entidade, DTO, repositório, migration nem regra de aplicação. Um
controller novo, `InstitucionalController`, expõe duas ações `GET` sem
dependência nenhuma injetada, e duas views entregam conteúdo fixo. O texto da
política é copiado do [Anexo A](./conteudo-politica.md) direto para a view — não
existe fonte de dados a consultar. O Quem Somos ganha uma partial reutilizada
três vezes com um *view model* de apresentação, para que o ziguezague de RF-14
seja uma propriedade do dado e não três blocos de HTML copiados. O andaime de
privacidade que veio com o template do ASP.NET (`HomeController.Privacidade` +
`Views/Home/Privacidade.cshtml`, texto em inglês) é removido no mesmo passo, e a
rota antiga passa a cair no 404 que a `008` construiu. Por fim, os três links
mortos (dois no rodapé, um no modal de login) são ligados.

Como não há camada de negócio para testar, o peso da prova cai onde ela é real:
teste de unidade de controller para o contrato das ações, e teste de ponta a
ponta (Playwright, `007`) para navegação, ordem do conteúdo e — pela primeira
vez no projeto — a ausência de rolagem horizontal a 375px, que a `008` teve de
registrar no checklist como verificada só por leitura de CSS.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ⬜ OK | Nenhuma `ProjectReference` nova. O controller não injeta nada — nem `Application`. |
| II | Domínio rico e auto-validante | n/a | Nenhuma entidade de domínio. `BlocoInstitucionalViewModel` é modelo de apresentação da MVC, não domínio: `record` imutável, sem regra de negócio. |
| III | Validação nas duas barreiras | n/a | Nenhuma entrada de usuário. Não há formulário (RF-17), logo não há o que validar. |
| IV | Nomenclatura em português | ⬜ OK | `InstitucionalController`, `QuemSomos`, `Privacidade`, `BlocoInstitucionalViewModel`, classes CSS em português. RF-07 **corrige** uma violação existente: a view de andaime está em inglês. |
| V | Testes escritos antes | ⬜ OK | Fase 2 inteira vermelha antes da Fase 3. Unidade de controller + E2E. |
| VI | Repositório + commit via UnitOfWork | n/a | Não toca persistência. Sem migration, sem `DbSet`, sem alteração no `.dbml`. |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | n/a (parcial) | Não existe `POST` nesta feature, logo não há antiforgery nem redirecionamento a garantir. A ausência de `[Authorize]` **é requisito** (RF-06), não esquecimento: são páginas públicas, e nenhuma delas é rota administrativa. |
| VIII | Tratamento de erro por camada | ⬜ OK | Nenhum `try/catch`. Nenhuma exceção esperada: as ações não recebem parâmetro. A rota antiga de RF-07 cai no `UseStatusCodePagesWithReExecute` que a `008` instalou. |

Nada a justificar na seção 9.

## 3. Direção visual

A referência visual fixa a direção, então ela manda. O que segue é a leitura
dela em decisões executáveis — e os pontos onde a referência é omissa e o plano
precisa escolher.

### Cor

Todas derivadas do que o site já usa. Nenhuma cor nova é inventada.

| Token | Valor | Papel |
|---|---|---|
| `--institucional-verde` | `var(--cor-primaria)` `#055C40` | O eixo do Quem Somos. Aparece **uma vez** por página. |
| `--institucional-coral` | `var(--cor-destaque)` `#D93B26` | Título de seção da política, a palavra "infância" |
| `--institucional-fundo` | `#FDF6F4` | Fundo rosado do Quem Somos, herdado de `produto.css` |
| `--institucional-tinta` | `#1E1E1E` | Corpo de texto |
| `--institucional-regua` | `#E8A79A` | O fio fino entre as seções da política. Coral rebaixado — separa sem competir com os títulos |

### Tipografia

Nenhuma família nova: as duas já estão importadas em `site.css`.

- **Manuscrita** — `Nothing You Could Do`. Usada exatamente **quatro vezes** em
  toda a feature: "infância", "Missão", "Propósito", "Visão". Fora daí, nunca.
  É a voz da loja e perde força se virar decoração.
- **Corpo e estrutura** — `Inter`. 700 para os títulos dos blocos e para a frase
  da faixa de destaque; 600 para título de seção da política; 400 para o corpo.
- A política usa corpo menor e entrelinha larga (`1.7`): é um documento para
  ler, não para varrer.

### Layout

**Política** — coluna única, largura de leitura limitada. Cada seção tem o
título encostado na margem esquerda e o conteúdo recuado sob ele. Esse recuo é
o que torna 11 seções longas escaneáveis sem sumário: o olho desce pela coluna
dos títulos, que é a única coisa na margem.

```
Política de Privacidade
  ┌ intro ────────────────────────────┐
  └───────────────────────────────────┘
Definições                              ← coral, na margem
    Dado Pessoal                        ← subtítulo, recuado
    Informação relacionada a...         ← corpo, recuado
────────────────────────────────────    ← régua coral fina
Quais dados pessoais coletamos?
    ...
```

**Quem Somos** — faixa de destaque em largura total, e abaixo dela três blocos
alternando em torno de um eixo vertical.

```
┌──────────────────────────────────────┐
│  [imagem]   Revivendo os sabores da  │
│             nossa infância.          │
└──────────────────────────────────────┘
   ┌────────┐  │
   │ imagem │  │      Missão
   └────────┘  │      texto...
               │  ┌────────┐
    Propósito  │  │ imagem │
    texto...   │  └────────┘
   ┌────────┐  │
   │ imagem │  │      Visão
   └────────┘  │      texto...
               ▲
        eixo verde contínuo
```

### Assinatura, e o risco assumido

**O eixo verde.** É um fio de 2px que corre por trás dos três blocos, e é a
única coisa que amarra o ziguezague — sem ele, os blocos são três linhas soltas
de uma tabela. O risco é que um fio central é frágil: some, some errado, ou
sobra pendurado quando o layout colapsa. A mitigação é tratá-lo como estrutura,
não como enfeite (RN-05): ele é um `::before` do contêiner do ziguezague, com a
mesma altura da grade, e é retirado junto com a segunda coluna abaixo de 900px.
Não vira uma linha vertical decorativa à esquerda numa página que já não faz
ziguezague.

**O que ficou de fora, por escolha.** Nenhuma animação — nem de entrada, nem de
rolagem, nem em passagem de mouse além do sublinhado de link. Uma política de
privacidade com blocos surgindo ao rolar é exatamente o efeito que denuncia
página gerada, e o Quem Somos tem quatro elementos: revelar quatro coisas em
sequência é cerimônia para pouco conteúdo. A página se sustenta na tipografia e
no eixo. Também não entra sumário lateral (spec §8) — a referência não tem, e
o recuo dos títulos já resolve a varredura.

## 4. Impacto por camada

### `DocesCabana.Domain`, `DocesCabana.Application`, `DocesCabana.Infrastructure`

Nenhum arquivo. Nenhum `.csproj` alterado.

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/InstitucionalController.cs` | criar | `Privacidade()` e `QuemSomos()`, `IActionResult` síncrono, sem dependência injetada, sem parâmetro |
| `Controllers/HomeController.cs` | alterar | remover a ação `Privacidade()` de andaime (RF-07) |
| `Views/Home/Privacidade.cshtml` | remover | view de andaime, texto em inglês (RF-07, e Princípio IV) |
| `Views/Institucional/Privacidade.cshtml` | criar | texto integral do Anexo A, 11 seções separadas por régua |
| `Views/Institucional/QuemSomos.cshtml` | criar | faixa de destaque + contêiner do ziguezague, invocando a partial 3× |
| `Views/Institucional/_BlocoInstitucional.cshtml` | criar | um bloco Missão/Propósito/Visão |
| `Models/BlocoInstitucionalViewModel.cs` | criar | `record` de apresentação do bloco |
| `Views/Shared/_Footer.cshtml` | alterar | ligar "Quem Somos" e "Política de Privacidade" (RF-03, RF-04) |
| `Views/Shared/_ModalLogin.cshtml` | alterar | ligar "Política de Privacidade" (RF-05) |
| `wwwroot/css/pages/institucional.css` | criar | tokens escopados, as duas páginas, colapso em coluna única |
| `wwwroot/images/institucional/` | criar | 4 imagens (faixa + 3 blocos) — pendente da spec §10 |

### `DocesCabana.Tests` / `DocesCabana.Tests.E2E`

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Controllers/InstitucionalControllerTests.cs` | criar | as duas ações devolvem `ViewResult` |
| `Units/Controllers/HomeControllerTests.cs` | alterar | remover o teste da ação apagada por RF-07 |
| `E2E/Paginas/PaginaPrivacidade.cs` | criar | *page object* |
| `E2E/Paginas/PaginaQuemSomos.cs` | criar | *page object* |
| `E2E/Fluxos/PaginasInstitucionaisTests.cs` | criar | navegação, ordem do conteúdo, 375px |

## 5. Contratos

Nenhuma interface nova. O controller não implementa nem consome contrato algum.

```csharp
public class InstitucionalController : Controller
{
    [HttpGet] public IActionResult Privacidade();
    [HttpGet] public IActionResult QuemSomos();
}

// DocesCabana.MVC/Models — modelo de apresentação, não domínio.
public record BlocoInstitucionalViewModel(
    string Titulo,
    string Texto,
    string ImagemUrl,
    string ImagemAlt,
    bool Invertido);
```

`Invertido` é o que produz RF-14: `false` põe a imagem à esquerda, `true`
inverte. Os três blocos são declarados na view como `false, true, false`.

Rotas resultantes da rota padrão, sem configuração nova:
`/Institucional/Privacidade` e `/Institucional/QuemSomos`.

## 6. Modelo de dados

Não se aplica. Nenhuma tabela, nenhuma migration, nenhuma alteração em
`ModelagemBancoTCC.dbml`.

## 7. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — controller | `Units/Controllers/InstitucionalControllerTests.cs` | cada ação devolve `ViewResult` e não exige autenticação |
| E2E — navegação | `E2E/Fluxos/PaginasInstitucionaisTests.cs` | os links do rodapé e do modal chegam nas páginas certas |
| E2E — conteúdo | idem | as 11 seções da política na ordem; os 3 blocos na ordem |
| E2E — responsivo | idem | `scrollWidth <= innerWidth` a 375px nas duas páginas |

Mapeamento critério → teste:

| Critério | Teste que o prova |
|---|---|
| CA-01 | `Dado_VisitanteNaPaginaInicial_Quando_ClicarNaPoliticaDoRodape_Entao_DeveAbrirAPolitica` |
| CA-02 | `Dado_ModalDeLoginAberto_Quando_ClicarNaPolitica_Entao_DeveAbrirAMesmaPagina` |
| CA-03 | `Dado_VisitanteNaPaginaInicial_Quando_ClicarEmQuemSomosNoRodape_Entao_DeveAbrirQuemSomos` |
| CA-04 | `Dado_PaginaDePolitica_Quando_ListarOsTitulosDeSecao_Entao_DeveTrazerAsOnzeNaOrdem` |
| CA-05 | `Dado_SecaoDeContato_Quando_InspecionarOEmail_Entao_DeveSerUmLinkMailto` |
| CA-06 | `Dado_PaginaQuemSomos_Quando_ListarOsBlocos_Entao_DeveTrazerMissaoPropositoEVisao` |
| CA-07 | `Dado_PaginaQuemSomosEmTelaLarga_Quando_CompararOsBlocos_Entao_OPropositoDeveEstarInvertido` |
| CA-08 | `Dado_VisitanteNaoAutenticado_Quando_AbrirCadaPaginaPelaUrl_Entao_NaoDeveRedirecionarParaLogin` + os dois testes de unidade de controller |
| CA-09 | `Dado_RotaAntigaDePrivacidade_Quando_Acessada_Entao_DeveResponder404` |
| CA-10 | `Dado_TelaDe375px_Quando_AbrirCadaPagina_Entao_NaoDeveHaverRolagemHorizontal` |
| CA-11 | verificação manual (T0nn) — foco de teclado visível |
| CA-12 | `Dado_CadaPaginaInstitucional_Quando_ProcurarFormulario_Entao_NaoDeveHaverNenhum` |

CA-11 não vira teste automatizado: `:focus-visible` é regra de CSS e o
Playwright só provaria que a classe existe, não que o contorno é perceptível.
Fica como verificação manual declarada, e não marcada como automatizada.

## 8. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Manter as ações no `HomeController` | `HomeController` já acumula `Index`, `AcessoNegado`, `NaoEncontrado` e `Error`. Duas páginas de conteúdo com identidade própria merecem um controller que as nomeie, e RF-07 pede remover a ação existente de qualquer modo |
| Guardar o texto da política no banco, editável pelo administrador | Fora de escopo declarado (spec §8). Traria entidade, repositório, migration e tela administrativa para publicar um texto que muda uma vez por ano |
| Ler o Markdown do Anexo A em tempo de execução e renderizar | Acrescentaria uma biblioteca de Markdown e um ponto de falha em tempo de execução para evitar uma cópia única de texto. A cópia é o custo menor |
| Três blocos de HTML repetidos no Quem Somos, sem partial | O ziguezague viraria três decisões de layout escritas à mão e passíveis de divergir. Com a partial, RF-14 é uma linha de dado (`Invertido`) |
| `<hr>` semântico entre as seções da política | A régua é separação visual, e o título de seção já separa semanticamente. Fica como borda em CSS, fora da árvore de acessibilidade |
| Animação de revelação ao rolar | Ver §3 — a página não precisa e o efeito trabalha contra a seriedade do documento |

## 9. Riscos

| Risco | Prob. | Impacto | Mitigação |
|---|---|---|---|
| **`@` literal no Razor.** O e-mail `privacidade@docecabana.com` e a menção a `@media` viram diretiva Razor e quebram a compilação da view | Alta | Médio | Escapar como `@@`, como o `_Footer.cshtml` já faz em `@@edocecabana`. Conferido na primeira execução da view |
| **Colisão de seletor no E2E.** O texto "Política de Privacidade" existe no rodapé **e** no modal de login, que está no DOM de toda página — a mesma armadilha que a `007` documentou para a logo e para "Entrar" | Alta | Médio | Todo *locator* escopado em `footer` ou em `.modal-login`, nunca na página inteira |
| **Vazamento de token CSS.** `produto.css` já teve esse risco registrado | Média | Médio | Tokens declarados em `.pagina-institucional`, nunca em `:root` |
| **Sem `RenderSection("Styles")` no layout.** `_Layout.cshtml` não tem seção de estilos | Certa | Baixo | Declarar o `<link>` no corpo da view, exatamente como `Views/Produto/Detalhes.cshtml` já faz. Não é o ideal, mas é o padrão vigente — mudar o layout é feature de outra spec |
| **Erro de transcrição do texto legal.** 11 seções copiadas à mão | Média | Alto | O Anexo A é a fonte; a conferência é tarefa própria (T0nn), lida lado a lado, e CA-04 trava a ordem das seções |
| **Imagens ausentes** (spec §10) | Alta | Baixo | O ziguezague é definido pela grade, não pela imagem: com lugar reservado de proporção fixa o layout fecha igual. A troca depois é substituição de arquivo |
| **Eixo verde sobrando no colapso** | Média | Médio | RN-05 vira regra explícita de CSS e a inspeção entra no checklist |

## 10. Desvios constitucionais justificados

*Nenhum.*
