# Especificações

Uma pasta por feature, no formato `NNN-slug-em-portugues`, contendo `spec.md`,
`plan.md`, `tasks.md` e, ao final, `checklist.md`.

O nome da pasta é também o nome da branch. Numeração sequencial, nunca reaproveitada.

## Índice

| ID | Feature | Status | Artefatos |
|---|---|---|---|
| [000](./000-baseline/spec.md) | Baseline do sistema | Implementada (parcial) | spec |
| [001](./001-cadastro-produto-admin/spec.md) | Cadastro de produto pelo administrador | Implementada | spec · [plan](./001-cadastro-produto-admin/plan.md) · [tasks](./001-cadastro-produto-admin/tasks.md) · [checklist](./001-cadastro-produto-admin/checklist.md) |
| [002](./002-revisao-tecnica/spec.md) | Revisão técnica da base | Implementada | spec · [plan](./002-revisao-tecnica/plan.md) · [tasks](./002-revisao-tecnica/tasks.md) · [checklist](./002-revisao-tecnica/checklist.md) |
| [003](./003-modelo-de-dados-completo/spec.md) | Modelo de dados completo | Implementada | spec · [plan](./003-modelo-de-dados-completo/plan.md) · [tasks](./003-modelo-de-dados-completo/tasks.md) · [checklist](./003-modelo-de-dados-completo/checklist.md) |
| [004](./004-separar-pessoa-de-credencial/spec.md) | Separar pessoa de credencial | Implementada | spec · [plan](./004-separar-pessoa-de-credencial/plan.md) · [tasks](./004-separar-pessoa-de-credencial/tasks.md) · [checklist](./004-separar-pessoa-de-credencial/checklist.md) |
| [005](./005-gestao-de-administradores/spec.md) | Gestão de administradores | Implementada | spec · [plan](./005-gestao-de-administradores/plan.md) · [tasks](./005-gestao-de-administradores/tasks.md) · [checklist](./005-gestao-de-administradores/checklist.md) |
| [006](./006-duplicidade-unificada-no-cadastro/spec.md) | Duplicidade unificada no cadastro | Implementada | spec · [plan](./006-duplicidade-unificada-no-cadastro/plan.md) · [tasks](./006-duplicidade-unificada-no-cadastro/tasks.md) · [checklist](./006-duplicidade-unificada-no-cadastro/checklist.md) |
| [007](./007-testes-e2e-com-playwright/spec.md) | Testes E2E com Playwright | Implementada | spec · [plan](./007-testes-e2e-com-playwright/plan.md) · [tasks](./007-testes-e2e-com-playwright/tasks.md) · [checklist](./007-testes-e2e-com-playwright/checklist.md) |
| [008](./008-pagina-do-produto/spec.md) | Página do produto | Implementada | spec · [plan](./008-pagina-do-produto/plan.md) · [tasks](./008-pagina-do-produto/tasks.md) · [checklist](./008-pagina-do-produto/checklist.md) |
| [009](./009-paginas-institucionais/spec.md) | Páginas institucionais | Implementada | spec · [plan](./009-paginas-institucionais/plan.md) · [tasks](./009-paginas-institucionais/tasks.md) · [checklist](./009-paginas-institucionais/checklist.md) · [conteúdo](./009-paginas-institucionais/conteudo-politica.md) |
| [010](./010-organizacao-de-nomenclatura/spec.md) | Organização de nomenclatura | Implementada | spec · [plan](./010-organizacao-de-nomenclatura/plan.md) · [tasks](./010-organizacao-de-nomenclatura/tasks.md) · [checklist](./010-organizacao-de-nomenclatura/checklist.md) |
| [011](./011-area-administrativa/spec.md) | Área administrativa | Implementada | spec · [plan](./011-area-administrativa/plan.md) · [tasks](./011-area-administrativa/tasks.md) · [checklist](./011-area-administrativa/checklist.md) |
| [012](./012-catalogo/spec.md) | Catálogo | Implementada | spec · [plan](./012-catalogo/plan.md) · [tasks](./012-catalogo/tasks.md) · [checklist](./012-catalogo/checklist.md) |
| [013](./013-correcoes-da-pagina-inicial/spec.md) | Correções da página inicial | Rascunho | spec · [plan](./013-correcoes-da-pagina-inicial/plan.md) · [tasks](./013-correcoes-da-pagina-inicial/tasks.md) |

> **Ordem executada:** `002` → `003` → `001` → `004` → `005` → `006` → `007` → `008` → `009` → `010` → `011` → `012`.
> A `001` originalmente esperava a `004`/`005` para resolver papéis, mas a
> pendência foi resolvida com o mínimo viável embutido nela própria (papel
> `Administrador` + admin semeado) — ver a nota de atualização na spec `001`.
> A `004` separou `Usuario` (domínio) de `ContaDeAcesso` (credencial do
> Identity), encerrando a limitação de navegação que a `003` registrou como
> RQ-02 e reescrevendo a exceção que a constituição abre ao Princípio I
> (1.1.0 → 1.2.0). A `005` deu à área administrativa uma tela para listar e
> cadastrar administradores, reaproveitando a compensação da `004`. A auditoria
> da `005` encontrou uma divergência de mensagem entre o cadastro de cliente e
> o de administrador para dado repetido; a `006` unificou a regra num único
> lugar (`IUsuarioService.ContaJaExiste`) e fechou a lacuna nos dois cadastros.
> A `007` percorreu os fluxos das specs `001` a `006` num navegador real —
> emenda constitucional 1.2.0 → 1.3.0 (Princípio V) para admitir
> `Microsoft.Playwright` como driver de navegador, mantendo o xUnit como
> runner único. Encontrou e corrigiu, de passagem, a ausência de qualquer
> caminho de "Sair" na interface.
>
> **Nota de numeração:** a pasta `008-pagina-do-produto` nasceu como `006`,
> criada por uma sessão em paralelo a partir de um `main` mais antigo (antes
> da `004`/`005` fecharem). Como o número `006` já tinha sido atribuído nesta
> linha à `duplicidade-unificada-no-cadastro` — implementada e mergeada —,
> a `pagina-do-produto` foi renumerada para `008` por ser a que ainda não
> tinha código nenhum. Nenhuma emenda de constituição associada; é
> reorganização de numeração, não decisão técnica.
>
> A `008` deu a cada produto uma página própria — imagem, descrição,
> avaliações com nota média, histograma e voto de útil. De passagem, fechou
> uma lacuna que a `007` não tinha como ver: `FilterException` só tratava
> exceção em requisições `POST`, e produto inexistente ou inativo é lido por
> `GET` — sem o ajuste, os dois critérios mais básicos da spec (CA-04, CA-05)
> eram inatingíveis.
>
> A `009` publica as duas páginas institucionais — Quem Somos e Política de
> Privacidade — e liga os três links mortos que apontavam para `#` (dois no
> rodapé, um no modal de login), e removeu a página de privacidade de andaime
> em inglês que veio do template do ASP.NET. É a primeira feature do projeto
> inteiramente contida na `DocesCabana.MVC`: sem entidade, sem migration, sem
> repositório. Os blocos de Missão/Propósito/Visão do Quem Somos entram com
> texto e imagem de preenchimento, exatamente como a referência visual os
> define — trocar por conteúdo real da loja é entrega futura. De passagem,
> encontrou (sem corrigir, por estar fora do escopo declarado) um estouro
> horizontal pré-existente do cabeçalho a 375px, presente em toda página do
> site, não só nas duas desta feature.
>
> A `010` nasceu de uma auditoria de nomenclatura (`/analisar`, fora do ciclo
> normal de feature): `AdminController` (cadastro de produto) virou
> `CatalogoController`, deixando de colidir com `AdministradorController`
> (gestão de administradores); `_Carrossel`/`_Categorias`, de uso único,
> saíram de `Views/Shared/` para `Views/Home/`. A regra de onde mora uma tela
> parcial de uso único, que a `008` já praticava sem registrar, ganhou texto
> no Princípio IV — emenda 1.3.0 → 1.4.0 (**MINOR**, não PATCH como o plano
> chegou a prever: duas regras normativas novas são expansão material do
> princípio, mesmo padrão da emenda 1.1.0, achado corrigido antes do commit).
>
> A `011` juntou as duas telas administrativas (cadastro de produto e gestão
> de administradores) numa *Area* `Admin` do ASP.NET Core, liberando o nome
> "catálogo" na raiz para a `012`. Foi a segunda renomeação do mesmo arquivo em
> duas specs seguidas — `AdminController` → `CatalogoController` na `010`,
> `CatalogoController` → `Areas.Admin.ProdutoController` aqui — justificada na
> seção 11 da spec: o arquivo já ia ser movido de qualquer jeito, e a `010` não
> tinha como prever a colisão com a entrega seguinte. Emenda constitucional
> 1.4.0 → 1.4.1 (**PATCH**, dessa vez de fato): ressalva de que a unicidade de
> nome de classe do Princípio IV é escopada por *area*, não pela solução
> inteira — `Admin/Produto` e `/Produto` são públicos diferentes.
>
> A `012` deu à loja a página de catálogo — barra lateral de categorias, filtro
> por subcategoria, "sem açúcar", ordenação e paginação — sobre a taxonomia
> real: 4 categorias, 31 subcategorias. "Doces Caseiros" e "Doces Zero" se
> fundiram em "Doces"; a distinção virou `Produto.SemAcucar`, característica do
> produto em vez de lugar na hierarquia (uma migration, `AddProdutoSemAcucar`).
> Matou os 4 atalhos mortos do cabeçalho, que ganhou menu suspenso por
> subcategoria, e o bloco de categorias da home. Cem produtos de mock,
> proporcionais à distribuição real — o catálogo verdadeiro de 390 produtos
> fica no backlog, esperando a loja exportar os dados. Dois achados fechados
> durante a execução: `RF-26` (produto fora de estoque sinalizado) tinha
> ficado sem implementação — o produto aparecia, mas indistinguível de
> qualquer outro —, corrigido com uma etiqueta no card; e um teste E2E que
> marcava duas subcategorias em sequência falhava de forma intermitente
> porque `CheckAsync()` não espera a navegação que o `onchange` do formulário
> dispara — corrigido no objeto de página, não na aplicação.

## A cadeia da loja (011 → 018)

Traçada em 2026-08-18, a partir de três referências visuais — catálogo filtrado,
catálogo completo e carrinho com fechamento. As três telas parecem duas
entregas e são sete: o mockup do carrinho sozinho encosta em estoque, endereço,
pedido, pagamento e promoção. A ordem abaixo é de dependência, não de
preferência — cada uma só é construível depois da anterior.

| # | Entrega | Estado | O que destrava |
|---|---|---|---|
| [011](./011-area-administrativa/spec.md) | Área administrativa | Implementada | libera o nome "catálogo" para o cliente |
| [012](./012-catalogo/spec.md) | Catálogo | Implementada | os 4 atalhos mortos do cabeçalho e o bloco de categorias da home |
| 014 | Estoque | não especificada | substitui o `ProdutoStatus.ForaDeEstoque` marcado à mão |
| 015 | Carrinho | não especificada | os três controles do card, desabilitados pela `012` |
| 016 | Endereço do usuário | não especificada | o `EnderecoEntregaId` que `Pedido` exige no construtor |
| 017 | Fechamento de pedido | não especificada | "Mais vendidos" passa a ser ordenação possível |
| 018 | Pagamento | não especificada | — |

**Perguntas em aberto, a resolver na spec de cada uma** — nenhuma tem resposta
ainda, e por isso `014` em diante não foram especificadas:

- **Frete** (`015`): valor fixo, por região, ou calculado? O mockup mostra
  `R$ 11,94` no resumo do pedido **antes** de o cliente informar endereço.
- **Cupom de desconto** (`017` ou spec própria): a entidade `Promocao` existe
  desde a `003` e nunca foi usada. Cupom por código é a mesma coisa que
  promoção na vitrine, ou são dois conceitos?
- **Carrinho de visitante** (`015`): quem não está logado pode montar carrinho,
  ou o botão leva ao login?
- **Reserva de estoque** (`014`/`015`): item no carrinho segura estoque, ou só
  no fechamento?

> **Nota de numeração:** a cadeia era `013`–`017` quando foi traçada. A
> `013` foi tomada pelas correções da página inicial — defeitos da `012` que
> não davam para deixar para depois — e a cadeia deslocou em um. Segue a regra
> do topo deste arquivo: o número é atribuído quando a spec é criada, e as
> entradas abaixo ainda não têm spec.

## Backlog fora da cadeia

Derivado das tabelas do [`ModelagemBancoTCC.dbml`](../ModelagemBancoTCC.dbml) que
ainda não têm comportamento. **Sem número** — o número é atribuído quando a spec
é criada, para que a chegada de uma feature nova não renumere a lista inteira.

| Feature | Depende de |
|---|---|
| Listagem, edição e exclusão de produto (admin) | 001, 011 |
| Lista de favoritos | 003 — o coração do card está desabilitado desde a `012` |
| Busca por texto | 012 — o campo do cabeçalho segue sem função |
| Escrever avaliação de produto | 008, carrinho |
| Galeria de imagens do produto | 008 |
| Promoções na vitrine | 003 |
| Sem glúten e sem lactose | 012 — mesma porta que `Produto.SemAcucar` abriu |
| Catálogo real da loja (390 produtos) | 012 — hoje é mock proporcional, 100 produtos |
| Imagens novas do bloco de categorias da home | 012 — as atuais não correspondem mais às categorias |
| Revisão da ordenação inicial do catálogo | 012 — "Nome (A-Z)" por não empatar; "Mais vendidos" é o alvo natural quando a `017` existir |

## Como criar a próxima

```powershell
.\.specify\scripts\nova-feature.ps1 "listagem de produtos admin"
```

O script cria a pasta numerada, copia os templates e opcionalmente cria a branch.
Depois disso, o fluxo é `/especificar` → `/planejar` → `/tarefas` → `/implementar`.

Leia [`.specify/README.md`](../.specify/README.md) antes da primeira vez.
