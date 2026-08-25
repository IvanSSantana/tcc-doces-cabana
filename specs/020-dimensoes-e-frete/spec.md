# Especificação — Dimensões do produto e cotação de frete

**ID:** `020-dimensoes-e-frete` · **Branch:** `020-dimensoes-e-frete`
**Criada em:** 2026-08-25 · **Status:** Rascunho

---

## 1. Contexto e problema

**A loja não sabe quanto pesa o que vende.** Nenhum produto tem peso ou medida
registrada. Sem isso, nenhuma transportadora consegue dizer quanto custa
entregar — e sem preço de entrega não existe fechamento de pedido.

**Quem chega ao carrinho não descobre o custo total.** A tela mostra o subtotal
das mercadorias e para aí. A pessoa decide se compra sem saber quanto vai pagar
no fim, que é justamente a informação de que ela precisa naquele momento.

**O botão de finalizar continua desligado.** Ele existe desde a entrega do
carrinho, sinalizado como indisponível. Continua assim ao fim desta entrega — o
fechamento é a entrega seguinte —, mas é o custo de entrega que falta para ele
poder ser ligado.

## 2. Objetivo

Registrar peso e medidas de cada produto, e mostrar a quem está no carrinho
quanto custa e quanto demora receber a compra no endereço dela.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Informa um CEP no carrinho e vê as opções de entrega com preço e prazo, sem precisar criar conta |
| Cliente autenticado | O mesmo — a cotação não depende de estar autenticado nem de ter endereço cadastrado |
| Administrador da loja | Informa peso e medidas ao cadastrar produto; sem eles o cadastro é recusado |
| Quem desenvolve o projeto | Passa a manter a primeira integração de rede feita pelo servidor, com segredo fora do versionamento |

## 4. Histórias de usuário

> **HU-01** — Como **cliente**, quero saber quanto custa a entrega antes de
> decidir comprar, e não só o preço das mercadorias.
>
> **HU-02** — Como **cliente**, quero saber em quantos dias recebo, para
> escolher entre pagar mais e receber antes ou pagar menos e esperar.
>
> **HU-03** — Como **cliente**, quero calcular o frete sem precisar criar conta
> nem cadastrar endereço.
>
> **HU-04** — Como **cliente**, quero que uma falha no cálculo do frete não me
> impeça de continuar mexendo no carrinho.
>
> **HU-05** — Como **administrador**, quero registrar peso e medidas do produto,
> para que a loja consiga cotar a entrega dele.

## 5. Requisitos funcionais

### Peso e medidas do produto

- **RF-01** — Todo produto DEVE ter peso, altura, largura e comprimento
  registrados.
- **RF-02** — O cadastro de produto DEVE exigir as quatro medidas e recusar
  valor que não seja maior que zero.
- **RF-03** — Os produtos já cadastrados DEVEM receber medidas compatíveis com o
  tipo de produto que são, e não um valor único para todos.

### Cotação de frete

- **RF-04** — A tela do carrinho DEVE oferecer o cálculo do frete a partir de um
  CEP informado por quem está vendo.
- **RF-05** — O cálculo DEVE funcionar para quem não está autenticado, sem
  exigir endereço cadastrado.
- **RF-06** — Cada opção de entrega apresentada DEVE informar a transportadora,
  o nome do serviço, o preço e o prazo em dias.
- **RF-07** — A cotação DEVE considerar o peso e as medidas de cada item do
  carrinho, e a quantidade de cada um.
- **RF-08** — A cotação NÃO DEVE considerar item indisponível, pelo mesmo
  critério com que o subtotal já não o considera.
- **RF-09** — CEP com formato inválido DEVE ser recusado com mensagem no próprio
  campo, sem nenhuma consulta ao serviço de entrega.
- **RF-10** — Falha ao cotar DEVE ser informada a quem está vendo, sem impedir
  que a pessoa continue usando o carrinho.
- **RF-11** — Carrinho sem nenhum item disponível NÃO DEVE oferecer o cálculo.
- **RF-12** — O cálculo DEVE funcionar com JavaScript desligado.

## 6. Regras de negócio

- **RN-01** — Produto sem peso e medidas não é despachável, e a loja não deve
  conseguir criar um. A recusa vale para qualquer caminho de criação, não só
  para o formulário.
- **RN-02** — Falha de serviço externo nunca impede usar a tela. Regra herdada
  da entrega de conta e endereços, onde a busca de CEP falhando deixa os campos
  digitáveis; aqui, a cotação falhando deixa o carrinho inteiro funcionando.
- **RN-03** — Item indisponível não entra em soma nenhuma. Regra herdada da
  entrega do carrinho, que já o exclui do subtotal; a cotação passa a respeitá-la
  também, porque o que não vai ser vendido não vai ser despachado.
- **RN-04** — O caminho sem JavaScript é o caminho real, não um consolo. Regra
  herdada: o que o script faz é melhorar uma interação que já funciona sem ele.
- **RN-05** — Credencial de serviço externo não é versionada. Regra
  constitucional, aplicada aqui pela primeira vez a um serviço consumido pelo
  servidor.
- **RN-06** — O preço e o prazo exibidos são os que o serviço de entrega
  devolveu, sem ajuste, arredondamento ou margem da loja. Anunciar número que a
  loja inventou seria dizer que veio da transportadora quando não veio.

## 7. Critérios de aceite

### CA-01 — Produto sem medidas é recusado no cadastro
- **Dado** que estou cadastrando um produto
- **Quando** deixo o peso, a altura, a largura ou o comprimento em branco ou com
  valor zero
- **Então** o cadastro é recusado com mensagem no campo correspondente

### CA-02 — Produto com medidas é aceito
- **Dado** que estou cadastrando um produto
- **Quando** informo peso e as três medidas com valores positivos
- **Então** o produto é cadastrado

### CA-03 — Os produtos existentes têm medidas
- **Dado** que a loja já tinha produtos cadastrados antes desta entrega
- **Quando** consulto qualquer um deles
- **Então** ele tem peso e as três medidas preenchidas, com valores compatíveis
  com o tipo de produto que é

### CA-04 — O carrinho oferece o cálculo do frete
- **Dado** que tenho itens disponíveis no carrinho
- **Quando** abro a tela do carrinho
- **Então** encontro um campo para informar o CEP e pedir o cálculo

### CA-05 — O cálculo devolve opções com preço e prazo
- **Dado** que estou na tela do carrinho
- **Quando** informo um CEP válido e peço o cálculo
- **Então** vejo as opções de entrega, cada uma com transportadora, serviço,
  preço e prazo em dias

### CA-06 — O visitante calcula sem conta
- **Dado** que não estou autenticado
- **Quando** informo um CEP no carrinho e peço o cálculo
- **Então** vejo as opções normalmente

### CA-07 — O que está no carrinho muda o frete
- **Dado** que calculei o frete para um CEP
- **Quando** acrescento mais itens e calculo de novo para o mesmo CEP
- **Então** o preço acompanha o que passou a estar no carrinho

### CA-08 — A distância muda o frete
- **Dado** que calculei o frete de um carrinho para um CEP próximo da loja
- **Quando** calculo o mesmo carrinho para um CEP distante
- **Então** o preço e o prazo do CEP distante são maiores

### CA-09 — O volume conta, não só o peso
- **Dado** um carrinho com um único produto leve e volumoso
- **Quando** comparo com um carrinho de um único produto pesado e compacto
- **Então** o produto volumoso pode custar mais para entregar, apesar de pesar
  menos

### CA-10 — CEP inválido é recusado antes de consultar
- **Dado** que estou na tela do carrinho
- **Quando** informo um CEP com formato inválido
- **Então** vejo a mensagem no campo do CEP e nenhuma consulta ao serviço de
  entrega é feita

### CA-11 — Serviço fora do ar não derruba o carrinho
- **Dado** que o serviço de entrega está indisponível
- **Quando** informo um CEP válido e peço o cálculo
- **Então** vejo uma mensagem dizendo que não foi possível calcular agora, e o
  carrinho continua inteiro e utilizável

### CA-12 — Item indisponível não é cotado
- **Dado** que meu carrinho tem um item disponível e um indisponível
- **Quando** peço o cálculo do frete
- **Então** a cotação corresponde apenas ao item disponível

### CA-13 — Carrinho sem item disponível não oferece cálculo
- **Dado** que meu carrinho está vazio, ou só tem item indisponível
- **Quando** abro a tela do carrinho
- **Então** o campo de cálculo de frete não é oferecido

### CA-14 — O cálculo funciona sem JavaScript
- **Dado** que estou com o JavaScript desligado
- **Quando** informo um CEP no carrinho e peço o cálculo
- **Então** vejo as opções de entrega

## 8. Fora de escopo

- **Escolher uma opção de entrega e guardá-la.** Cotar é informar; escolher é
  parte de fechar o pedido, e é a entrega seguinte. Aqui a pessoa vê as opções e
  nada é gravado.
- **Comprar etiqueta, imprimir etiqueta e rastrear entrega.** Nada nesta entrega
  gera obrigação de pagamento com a transportadora.
- **Frete grátis, cupom de frete e promoção sobre entrega.** O preço exibido é o
  que a transportadora cobra.
- **Cotação na página do produto.** Só no carrinho.
- **Mais de um endereço de origem.** A loja despacha de um lugar só.
- **Editar as medidas dos produtos já cadastrados pela tela.** Eles recebem
  medidas nesta entrega, mas a tela de edição de produto não existe — é item de
  backlog anterior a esta feature.
- **Escolher a embalagem.** Quantas caixas e de que tamanho é decisão do serviço
  de entrega, a partir do que a loja informa sobre cada item.

## 9. Dependências

- **Depende de:** a entrega do carrinho, que criou a tela e a noção de item
  disponível; e o cadastro de produto, que ganha os campos novos.
- **Bloqueia:** o fechamento de pedido, que precisa do custo de entrega para
  compor o valor do pedido, e que só então pode ligar o botão de finalizar.

## 10. Decisões e pendências

**O frete é cotado por um serviço externo real, sem simulador interno.**
Decisão do responsável, tomada ao especificar esta entrega, revendo um desenho
anterior que previa os dois. Um simulador interno seria uma segunda
implementação da mesma regra de negócio — o serviço externo já calcula peso
cubado por conta própria, e ter a nossa versão ao lado criaria duas respostas
para a mesma pergunta, das quais só uma é real, sem nenhum jeito de conferir uma
contra a outra. Existe uma implementação só, e é a que roda em produção.

**O serviço escolhido é o MelhorEnvio, em ambiente de sandbox.** A cotação é
gratuita nos dois ambientes; o sandbox foi escolhido por isolamento. O endereço
do serviço é configuração, então trocar para produção não muda código.

**O CEP de origem da loja é `17340-001`**, interior de São Paulo. Vai em
configuração versionada, por não ser segredo.

**A credencial fica em *user secrets*, nunca no repositório.** O arquivo de
exemplo de configuração ganha a seção com o campo vazio, para quem clonar o
projeto saber o que precisa configurar sem receber nada secreto.

**Os testes que tocam a rede ficam separados por categoria** e fora da execução
padrão, seguindo o que a suíte de ponta a ponta já faz. Quem tem a credencial os
executa explicitamente; quem não tem roda o resto da suíte sem nenhuma falha.

**As medidas dos produtos existentes são atribuídas por categoria**, não por um
valor único. Adega é pesada e compacta, Souvenir é leve e volumosa, Doces e
Empório ficam no meio. Um valor único faria toda cotação variar só pela
quantidade de itens, nunca pelo que eles são, e o CA-09 seria impossível de
satisfazer.

**⚠️ A credencial do MelhorEnvio ainda não foi obtida.** O cadastro exige etapas
de verificação que seguem em andamento. Consequência: as tarefas de integração e
os testes contra a API ficam **bloqueados até a credencial existir**; as tarefas
de peso e medidas não dependem dela e são executáveis desde já. Nenhuma decisão
desta spec depende da credencial — só a execução de parte dela.

**⚠️ Uma referência obsoleta foi encontrada ao especificar.** O botão desligado
de finalizar compra, na tela do carrinho, tem comentário dizendo que o
fechamento é a entrega `019`. Ficou errado duas vezes: pela renumeração da `019`
e pela divisão que criou esta entrega. Escapou da varredura da `019` porque o
texto não contém a palavra "spec". É corrigido aqui, e a varredura passa a
procurar número solto também.

**⚠️ A ordem das categorias no cabeçalho continua sendo a do banco** — pendência
herdada, repetida em todas as entregas desde a de correções da página inicial,
ainda sem critério definido pelo responsável. Segue fora de escopo.

---

## Checklist de qualidade da spec

- [x] Não há nenhum detalhe de implementação (classe, tabela, framework, rota)
      nos requisitos — os nomes técnicos aparecem só na seção 10, como decisão
      tomada
- [x] Todo requisito funcional é verificável por um teste
- [x] Todo requisito tem ao menos um critério de aceite correspondente
- [x] Os caminhos de erro estão especificados, não só o caminho feliz — CA-01
      cobre cadastro sem medida; CA-10, CEP inválido; CA-11, serviço fora do ar;
      CA-12, item indisponível; CA-13, carrinho sem o que cotar
- [x] Mensagens visíveis ao usuário estão escritas em português, no texto final
- [x] A seção "Fora de escopo" foi preenchida de verdade
- [x] Não restam marcações `[NECESSITA ESCLARECIMENTO]` — as três pendências da
      seção 10 são de execução ou herdadas, nenhuma bloqueia a especificação
- [x] Nada aqui conflita com `.specify/memory/constitution.md`
