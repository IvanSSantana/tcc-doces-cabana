# Plano Técnico — Redesenho do carrinho

**Spec:** [`spec.md`](./spec.md) · **Status:** Rascunho
**Criado em:** 2026-08-25

---

## 1. Resumo da abordagem

Entrega de apresentação, com uma ação nova atrás dela. Quase tudo acontece em
`DocesCabana.MVC`; a `Application` ganha um método; o `Domain` não muda.

**A descoberta que define o plano:** o protótipo mostra duas colunas, mas isso é
**decisão de layout, não de estrutura**. O parcial atual já mantém itens e
resumo dentro do mesmo `#itens-carrinho`, e o comentário no arquivo diz por quê:

> *"itens, resumo e subtotal vivem juntos aqui, porque alterar quantidade muda
> os três ao mesmo tempo"*

Separar o resumo num container irmão obrigaria o script a trocar dois pedaços em
vez de um, e a troca deixaria de ser atômica — por um instante a tela mostraria
itens novos com subtotal velho. **As duas colunas saem de `display: grid` sobre
o container que já existe.** Consequência: `carrinho.js` não muda uma linha, e a
troca sem recarga da `017` continua valendo de graça.

## 2. Verificação constitucional

| Princípio | Situação |
|---|---|
| **I — Direção de dependência** | ✅ Nenhuma referência nova |
| **II — Domínio se defende** | ✅ Nenhuma entidade muda |
| **III — Duas barreiras** | ✅ Esvaziar não recebe dado do usuário — não há o que validar |
| **IV — Português** | ✅ `Esvaziar`, `EsvaziarAvulso`, `ConfirmarEsvaziar` |
| **V — Teste antes** | ✅ Ciclo vermelho-verde. O redesenho visual é coberto por E2E; o serviço, por unidade |
| **VI — Persistência escondida** | ✅ Esvaziar remove pelo repositório e comita uma vez pelo `IUnitOfWork` — ver §4 sobre por que não `ExecuteDelete` |
| **VII — Seguro na borda** | ✅ `Esvaziar` é `[HttpPost]` com `[ValidateAntiForgeryToken]`, aguardado, redirecionando no sucesso |
| **VIII — Dono do erro** | ✅ Nenhum caminho de erro novo |

## 3. Direção visual

Do protótipo, o que é normativo:

| Elemento | Decisão |
|---|---|
| Estrutura | Duas colunas: itens à esquerda sobre fundo claro, resumo à direita sobre fundo cinza |
| Item | Cartão de cantos arredondados com borda coral; miniatura à esquerda, nome, e três colunas rotuladas — preço unitário, quantidade, subtotal |
| Rótulos | Repetidos em **cada** cartão, não uma vez como cabeçalho de tabela — é o que faz o cartão se sustentar sozinho ao empilhar em tela estreita |
| Valores | Preço unitário e subtotal em verde e negrito |
| Remover | Pílula vermelha vazada, com ícone de lixeira, sob o nome |
| Resumo | Cartão com borda: cupom, régua, contagem de produtos com ícone, entrega com ícone, régua, valor em destaque |
| Finalizar | Botão verde de largura total, **fora** do cartão do resumo |
| Rodapé do passo | "Esvaziar Carrinho" à esquerda com lixeira; "Continuar Comprando →" à direita, em verde |

As cores saem das variáveis que o projeto já define — nada de paleta nova.

**O que não é normativo:** o menu do protótipo diz "Doce, Salgado, Adega,
Outros"; a taxonomia real é "Doces, Empório, Adega, Souvenir". O cabeçalho não
muda aqui.

## 4. Impacto por camada

### `DocesCabana.Domain`

Nenhuma mudança.

### `DocesCabana.Application`

| Arquivo | Mudança |
|---|---|
| `Contracts/Services/ICarrinhoService.cs` | `Esvaziar(Guid usuarioId)` |
| `Services/CarrinhoService.cs` | Implementação |
| `DTOs/CarrinhoDTO.cs` | `Cotacao` anulável — **ver §6** |

**Sem método de repositório novo.** `Esvaziar` busca os itens com
`BuscarPorUsuario`, chama `Remover` em cada um e comita uma vez:

```csharp
public async Task Esvaziar(Guid usuarioId)
{
    var itens = await _itemCarrinhoRepository.BuscarPorUsuario(usuarioId);
    foreach (var item in itens)
        _itemCarrinhoRepository.Remover(item);

    await _unitOfWork.SalvarAlteracoes();
}
```

`ExecuteDeleteAsync` faria uma consulta só, mas **contorna o `ChangeTracker` e
grava sozinho** — sem passar pelo `IUnitOfWork`, que o Princípio VI define como
o único ponto de gravação. Num carrinho de dezenas de itens, o laço custa nada e
respeita a regra. O carrinho avulso não precisa de nada: `CarrinhoDaSessao.Limpar`
já existe desde a `017`.

### `DocesCabana.MVC`

| Arquivo | Mudança |
|---|---|
| `Views/Carrinho/_ItensDoCarrinho.cshtml` | Reescrito: cartões e resumo em coluna, **mantendo o `#itens-carrinho` como raiz** |
| `Views/Carrinho/_ConfirmarEsvaziar.cshtml` | **nova** — a pergunta |
| `Controllers/CarrinhoController.cs` | `ConfirmarEsvaziar` (GET) e `Esvaziar` (POST) |
| `wwwroot/css/pages/carrinho.css` | Reescrito |
| `wwwroot/js/components/carrinho.js` | **Só acrescenta** o diálogo de confirmação; a troca sem recarga não muda |
| `Views/Carrinho/Index.cshtml` | Inalterado |

## 5. A confirmação, nos dois caminhos

A RF-11 exige perguntar, e a RF-13 exige funcionar sem script. Um `<dialog>`
sozinho não abre sem JavaScript, então a pergunta precisa existir como página:

```
sem script:  GET  /Carrinho/ConfirmarEsvaziar   → tela com a pergunta
             POST /Carrinho/Esvaziar            → esvazia e redireciona

com script:  o link é interceptado, um <dialog> abre na própria tela
             e envia para o mesmo POST /Carrinho/Esvaziar
```

Um destino de escrita só, alcançado por dois caminhos — o mesmo desenho de
"o caminho degradado é o código real" que o projeto aplica desde a `014`.

## 6. Sobre a fronteira com a entrega de cotação de frete

A linha de entrega e a troca de rótulo (RF-05 a RF-07) nascem **aqui**, com o
valor sempre ausente, porque quem o calcula é a entrega de cotação.

Isso significa que **`CarrinhoDTO.Cotacao` é criado por esta entrega**, não por
aquela — o plano da cotação previa criá-lo, e a decomposição inverteu a ordem.
Ao executar, quem chegar primeiro cria; o segundo confere que já existe.

O estado com entrega calculada (CA-05) **não é verificável nesta entrega
isoladamente** — não há de onde vir um valor. O teste de unidade cobre os dois
estados injetando a cotação diretamente; o teste de ponta a ponta cobre só o
estado sem entrega. O outro é coberto quando a cotação existir. Está registrado
como risco em §8.

## 7. Estratégia de teste

| Camada | Casos |
|---|---|
| `Units/Services/CarrinhoServiceTests.cs` | `Esvaziar` remove todos os itens do usuário e comita **uma vez**; carrinho já vazio não quebra nem comita à toa |
| `Units/Controllers/CarrinhoControllerTests.cs` | `Esvaziar` chama o serviço e redireciona; visitante limpa a sessão em vez do banco; `ConfirmarEsvaziar` devolve a view |
| `Units/Mappings/CarrinhoMapperTests.cs` | Com cotação, o total inclui a entrega; sem cotação, o valor em destaque é o subtotal (CA-04/CA-05) |
| `E2E/Fluxos/CarrinhoTests.cs` | Cartões com os cinco elementos (CA-01); esvaziar pede confirmação (CA-08); confirmar esvazia (CA-09); desistir não remove (CA-10); voltar ao catálogo preserva (CA-11); sem script tudo funciona (CA-12); 375px empilha (CA-13); cupom e finalizar desabilitados (CA-06/CA-07) |

**Os 19 testes E2E da `017` estão protegidos pelo objeto de página.** Nenhum
deles fixa seletor: os onze seletores vivem em `Paginas/PaginaCarrinho.cs`. O
redesenho atualiza **esse arquivo**, e os 19 testes seguem valendo sem edição —
que é exatamente para isso que o objeto de página existe. Os 41 testes
unitários de carrinho testam comportamento, não marcação, e não são afetados.

## 8. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| **Resumo num container irmão de `#itens-carrinho`** | A troca sem recarga deixaria de ser atômica: por um instante, itens novos com subtotal velho. As duas colunas saem de CSS sobre o container único |
| **`ExecuteDeleteAsync` para esvaziar** | Contorna o `ChangeTracker` e grava fora do `IUnitOfWork`, contra o Princípio VI |
| **Método `RemoverTodos` no repositório** | O laço sobre `BuscarPorUsuario` resolve com o que já existe |
| **`confirm()` do navegador** | Sem JavaScript não pergunta nada, e a RF-11 exige perguntar sempre |
| **Manter a tela atual e desenhar só o fechamento** | Duas telas de carrinho com aparências diferentes no mesmo sistema |
| **Trazer o indicador de passos já** | Três das quatro etapas não levariam a lugar nenhum — RN-01 |
| **Campo de cupom funcional** | A regra de desconto é decisão de negócio ainda não tomada |
| **Rótulos de coluna uma vez, como cabeçalho de tabela** | Ao empilhar em tela estreita, o cartão perderia a referência do que é cada número |

## 9. Riscos

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| **CA-05 fica sem prova de ponta a ponta** | Alta | Baixo | Coberto por unidade injetando a cotação; o E2E chega com a entrega de cotação. Registrado em §6, não descoberto depois |
| **O redesenho quebra seletor que o objeto de página não cobre** | Média | Baixo | Os 19 testes passam pelo objeto; conferir com a suíte inteira ao fim, não por arquivo |
| **A troca sem recarga quebra ao mudar o parcial** | Média | Médio | O `#itens-carrinho` permanece como raiz e `carrinho.js` não muda — é a decisão do §1 que existe para evitar isto |
| **Empilhamento a 375px** | Média | Baixo | Teste próprio (CA-13). O estouro do cabeçalho a 375px é dívida herdada e segue fora de escopo |
| **`CarrinhoDTO.Cotacao` criado em duplicidade** | Média | Baixo | §6 declara quem cria; quem executar em segundo lugar confere em vez de criar |

## 10. Desvios constitucionais justificados

*Nenhum.*

Entrega de apresentação com um método de aplicação novo. Não cria entidade, não
altera esquema, não introduz caminho de erro, e a única escrita passa pelo
`IUnitOfWork` — inclusive ao custo de um laço em vez de uma consulta só, que é a
escolha registrada em §4.
