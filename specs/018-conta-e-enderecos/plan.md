# Plano Técnico — Conta e endereços

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-23
**Status:** Rascunho

---

## 1. Resumo da abordagem

**Metade desta feature é ligação, não construção.** `Usuario.AtualizarDados`,
`IUsuarioService.AlterarDadosUsuario` e `UsuarioMapper` existem desde a `004`,
com teste de unidade passando, e nenhum controlador os chama. A seção de dados
pessoais é um controlador, uma view e um validator — nada de domínio, nada de
aplicação.

**A outra metade é um CRUD comum sobre uma entidade que precisa aprender a
mudar.** `Endereco` é hoje só-criação: nove campos, um construtor validante e
nenhum `Alterar*`. Ganha os métodos de intenção que o Princípio II exige, mais
`Padrao` e `DataCadastro`.

**A invariante do endereço principal não cabe na entidade.** `Endereco` sozinho
não sabe quantos irmãos tem, e "exatamente um principal" é propriedade da
coleção. A RN-01, a RN-03 e a RN-04 vivem em `EnderecoService`, que enxerga
todos os endereços da pessoa; a entidade guarda apenas o próprio `Padrao` e os
métodos que o alteram. Esse é o mesmo limite que a `015` respeitou ao pôr a
regra do interruptor de favorito no serviço e não em `Favorito`.

**A área de conta é um controlador com duas ações, não duas telas soltas.**
`/Conta` traz dados pessoais; `/Conta/Enderecos`, a lista. Um layout parcial
compartilhado desenha o menu lateral, e ele nasce preparado para a terceira
seção que a `019` vai acrescentar.

**A busca por CEP roda no navegador.** Um `fetch` para o ViaCEP preenche os
campos e nada mais. Sem JavaScript não há busca, e os campos já são
preenchíveis à mão — a RN-07 sai de graça, sem cliente HTTP, sem tempo limite
configurado e sem rota de proxy.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ⬜ OK | Nenhuma `ProjectReference` nova. `IEnderecoService` na `Application`; repositório na `Infrastructure`. O ViaCEP é chamado pelo navegador, então não entra em camada nenhuma |
| II | Domínio rico e auto-validante | ⬜ OK | `Endereco` ganha `AtualizarDados`, `MarcarComoPadrao` e `DesmarcarComoPadrao` — estado deixa de mudar só por construtor. `Padrao` e `DataCadastro` com `private set` |
| III | Validação nas duas barreiras | ⬜ OK | `EnderecoDTOValidator` e `DadosPessoaisDTOValidator` protegem o usuário; os construtores e métodos de `Endereco` e `Usuario` protegem o dado. Os dois formulários têm campos de texto de verdade — aqui o princípio se aplica inteiro, diferente da `016` e da `017` |
| IV | Nomenclatura em português | ⬜ OK | `IEnderecoService`, `ContaController`, `EnderecoDTO`, `MarcarComoPadrao`, `DataCadastro`, `endereco.js` |
| V | Testes escritos antes | ⬜ OK | Cada fase tem fase vermelha própria |
| VI | Repositório + commit via UnitOfWork | ⬜ OK | Toda escrita passa por `IEnderecoRepository` e fecha com `IUnitOfWork.SalvarAlteracoes`. Migration `AddEnderecoPadraoEDataCadastro` |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ⬜ OK | `[Authorize]` no controlador inteiro; `[HttpPost]` + `[ValidateAntiForgeryToken]` em todas as escritas; guarda de `ModelState`; PRG no sucesso |
| VIII | Tratamento de erro por camada | ⬜ OK | Endereço inexistente **ou de outra pessoa** vira `KeyNotFoundException` — ver §9, risco 1 |

## 3. Direção visual

Nenhuma cor e nenhuma fonte nova. A conta reaproveita o desenho de formulário
que a `016` extraiu para `components/formulario.css`, e a lista de endereços
reaproveita o cartão com borda que `administradores.css` já usa na tabela.

```
Minha conta
┌──────────────────┬────────────────────────────────────────┐
│ ▸ Dados pessoais │  Dados pessoais                        │
│   Endereços      │                                        │
│                  │  Nome completo                         │
│  (Meus pedidos   │  [__________________________]          │
│   entra na 019)  │                                        │
│                  │  CPF              Celular              │
│                  │  529.982.247-25   [_____________]      │
│                  │  (não editável)                        │
│                  │                                        │
│                  │  Data de nascimento                    │
│                  │  [__/__/____]                          │
│                  │              [ Salvar alterações ]     │
└──────────────────┴────────────────────────────────────────┘
```

O CPF aparece **como texto, não como campo desabilitado** — um campo apagado
convida a tentar, e a RN-06 diz que ele não muda. Mostrar o valor e não oferecer
o controle é mais honesto que oferecer um controle morto (RN-08).

```
Endereços                              [ + Novo endereço ]
┌────────────────────────────────────────────────────────┐
│ ★ PRINCIPAL                                            │
│ Rua das Acácias, 128 — Apto 42                         │
│ Centro · Lençóis Paulista/SP · 18680-000               │
│                              [ Editar ]  [ Excluir ]   │
├────────────────────────────────────────────────────────┤
│ Av. Brasil, 900                                        │
│ Jardim Europa · Bauru/SP · 17010-000                   │
│        [ Tornar principal ]  [ Editar ]  [ Excluir ]   │
└────────────────────────────────────────────────────────┘
```

O endereço principal traz a marcação e **não oferece** "Tornar principal" — o
botão só aparece onde faz sentido acionar.

**O formulário de endereço tem o CEP como primeiro campo**, porque é ele que
preenche os demais. Enquanto a busca acontece, os campos que serão preenchidos
ficam em estado de espera; se ela falhar, voltam a ser campos normais e vazios,
sem mensagem alarmante — falhar em preencher automaticamente não é erro do
usuário (RN-07).

## 4. Impacto por camada

### `DocesCabana.Domain`

| Arquivo | Ação | O quê |
|---|---|---|
| `Entities/Endereco.cs` | alterar | `Padrao` e `DataCadastro` com `private set`; `AtualizarDados`, `MarcarComoPadrao`, `DesmarcarComoPadrao` |

### `DocesCabana.Application`

| Arquivo | Ação | O quê |
|---|---|---|
| `DTOs/EnderecoDTO.cs` | **criar** | Os nove campos mais `Padrao`; usado na listagem e nos dois formulários |
| `DTOs/DadosPessoaisDTO.cs` | **criar** | Nome, celular, data de nascimento — e CPF só para exibir |
| `Validators/EnderecoDTOValidator.cs` | **criar** | Obrigatoriedade, tamanho, CEP de 8 dígitos, número positivo |
| `Validators/DadosPessoaisDTOValidator.cs` | **criar** | Reaproveita as regras de nome, celular e data que `CadastroDTOValidator` já tem |
| `Contracts/Repositories/IEnderecoRepository.cs` | **criar** | Ver §5 |
| `Contracts/Services/IEnderecoService.cs` | **criar** | Ver §5 |
| `Services/EnderecoService.cs` | **criar** | O CRUD e as invariantes de coleção (RN-01 a RN-05) |
| `Mappings/EnderecoMapper.cs` | **criar** | `ToDTO` e `ToEntity` |

`IUsuarioService.AlterarDadosUsuario` **não muda** — já faz o necessário.

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `Repositories/EnderecoRepository.cs` | **criar** | Consulta por usuário ordenada por `DataCadastro`; busca por par `(enderecoId, usuarioId)` |
| `DatabaseContext/Configurations/EnderecoConfiguration.cs` | alterar | Mapear `Padrao` (padrão `false`) e `DataCadastro` |
| `Migrations/` | **criar** | `AddEnderecoPadraoEDataCadastro` |
| `DependencyInjections/ApplicationDependencyInjection.cs` | alterar | Registro do repositório e do serviço |

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/ContaController.cs` | **criar** | `Index`, `AlterarDados`, `Enderecos`, `NovoEndereco`, `EditarEndereco`, `ExcluirEndereco`, `TornarPrincipal` |
| `Views/Conta/Index.cshtml` | **criar** | Dados pessoais |
| `Views/Conta/Enderecos.cshtml` | **criar** | Lista |
| `Views/Conta/FormularioEndereco.cshtml` | **criar** | Cadastro e edição, na mesma view |
| `Views/Conta/_MenuDaConta.cshtml` | **criar** | Menu lateral. Tela parcial de uso único → mora com o controlador dono (Princípio IV) |
| `Views/Shared/Components/Header/Default.cshtml` | alterar | O atalho "Conta" deixa de estar apagado e passa a apontar para `/Conta` |
| `wwwroot/js/pages/conta.js` | **criar** | Busca por CEP e máscaras de celular, data e CEP |
| `wwwroot/css/pages/conta.css` | **criar** | Menu lateral e cartões de endereço |

### `DocesCabana.Tests` / `DocesCabana.Tests.E2E`

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Entities/EnderecoTests.cs` | alterar | Os três métodos novos e as invariantes que eles preservam |
| `Units/Services/EnderecoServiceTests.cs` | **criar** | RN-01 a RN-05 — o coração desta feature |
| `Units/Validators/EnderecoDTOValidatorTests.cs` | **criar** | Um caso válido e um inválido por regra |
| `Units/Validators/DadosPessoaisDTOValidatorTests.cs` | **criar** | Idem |
| `Units/Controllers/ContaControllerTests.cs` | **criar** | `ModelState`, redirecionamento, e a recusa de endereço alheio |
| `Integration/Repositories/EnderecoIntegrationTests.cs` | **criar** | Persistência real, ordenação por `DataCadastro`, isolamento entre pessoas |
| `E2E/Paginas/PaginaConta.cs` | **criar** | Objeto de página |
| `E2E/Fluxos/ContaTests.cs` | **criar** | CA-01 a CA-21 |

## 5. Contratos

```csharp
// ── Domínio ────────────────────────────────────────────────────────────
public class Endereco
{
    public bool Padrao { get; private set; }
    public DateTime DataCadastro { get; private set; }

    public void AtualizarDados(string estado, string cidade, string bairro,
                               string cep, string rua, int numero, string? complemento);
    public void MarcarComoPadrao();
    public void DesmarcarComoPadrao();
}
```

`AtualizarDados` roda **as mesmas validações do construtor** — é o mesmo
conjunto de invariantes, e duplicá-las com regras diferentes seria a porta para
um endereço editado ficar num estado que o construtor recusaria.

```csharp
// ── Aplicação ──────────────────────────────────────────────────────────
public interface IEnderecoRepository
{
    Task<List<Endereco>> BuscarPorUsuario(Guid usuarioId);          // ordenado por DataCadastro
    Task<Endereco?> Buscar(Guid enderecoId, Guid usuarioId);        // o par, nunca só o id
    Task Adicionar(Endereco endereco);
    void Remover(Endereco endereco);
}

public interface IEnderecoService
{
    Task<List<EnderecoDTO>> ListarDoUsuario(Guid usuarioId);
    Task<EnderecoDTO> BuscarDoUsuario(Guid enderecoId, Guid usuarioId);
    Task Cadastrar(EnderecoDTO dto, Guid usuarioId);
    Task Editar(EnderecoDTO dto, Guid usuarioId);
    Task Excluir(Guid enderecoId, Guid usuarioId);
    Task TornarPrincipal(Guid enderecoId, Guid usuarioId);
}
```

**Todo método recebe `usuarioId`, e o repositório busca sempre pelo par.** Não
existe `BuscarPorId(enderecoId)` sozinho no contrato — é o desenho que torna a
RN-05 difícil de violar por esquecimento, em vez de depender de o controlador
lembrar de conferir o dono depois de buscar. Ver §9, risco 1.

## 6. Modelo de dados

- **Entidade:** `Endereco` ganha `Padrao` (bit, obrigatório, padrão `false`) e
  `DataCadastro` (datetime2, obrigatório).
- **Por que duas colunas e não uma:** `Padrao` é o que a RN-01 exige.
  `DataCadastro` se justifica sozinha por dois motivos — sem ordem estável a
  lista apareceria na ordem arbitrária do banco, e a RN-04 não teria critério
  para escolher **qual** endereço promover ao excluir o principal.
- **Relacionamentos:** nenhum novo. O `Endereco → Usuario` com `Restrict` já
  existe.
- **Migration:** `dotnet ef migrations add AddEnderecoPadraoEDataCadastro --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC`
- **Impacto em dados existentes:** **nenhum na prática.** A tabela `Endereco`
  está vazia — nunca houve tela para cadastrar um. As colunas nascem com padrão
  e não há linha para preencher retroativamente. É a diferença entre esta
  migration e a `AddProdutoNomeNormalizado` da `016`, que precisou de correção
  em C#.
- **`ModelagemBancoTCC.dbml`:** as duas colunas entram no diagrama. Entregável
  do TCC, e desatualizá-lo é dívida silenciosa — tarefa própria.

## 7. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — entidade | `EnderecoTests` | `AtualizarDados` valida igual ao construtor; os dois métodos de marcação |
| Unidade — serviço | `EnderecoServiceTests` | RN-01 a RN-05: o primeiro vira principal; marcar desmarca o anterior; excluir o principal promove o mais antigo; excluir o último não deixa órfão; endereço de outra pessoa não é alcançado |
| Unidade — validator | `EnderecoDTOValidatorTests`, `DadosPessoaisDTOValidatorTests` | Cada `RuleFor` com um caso válido e um inválido |
| Unidade — controller | `ContaControllerTests` | Guarda de `ModelState`, redirecionamento no sucesso, e `KeyNotFoundException` para endereço alheio |
| Integração | `EnderecoIntegrationTests` | Persistência real; ordenação por `DataCadastro`; a consulta de um usuário não traz endereço de outro |
| E2E | `ContaTests` | O resto — inclusive o piso sem JavaScript e a falha do ViaCEP, que só o navegador exercita |

Mapeamento critério → teste:

| Critério | Teste |
|---|---|
| CA-01 | `Dado_ClienteAutenticado_Quando_AcionarOAtalhoConta_Entao_DeveChegarNaAreaDeConta` |
| CA-02 | `Dado_AreaDeConta_Quando_OlharATela_Entao_DeveReunirDadosPessoaisEEnderecos` |
| CA-03 | `Dado_Visitante_Quando_TentarAbrirAConta_Entao_DeveSerLevadoAEntrar` |
| CA-04 | `Dado_ContaRecemCriada_Quando_AbrirOsDadosPessoais_Entao_DevemVirPreenchidos` |
| CA-05 | `Dado_DadosPessoais_Quando_CorrigirOCelular_Entao_DevePersistir` |
| CA-06 | `Dado_DadosPessoais_Quando_TentarAlterarOCpf_Entao_NaoDeveConseguir` |
| CA-07 | `Dado_CelularInvalido_Quando_Salvar_Entao_DeveVoltarComMensagemNoCampoEOsDemaisPreenchidos` |
| CA-08 | `Dado_NenhumEndereco_Quando_CadastrarOPrimeiro_Entao_DeveNascerPrincipal` |
| CA-09 | `Dado_NenhumEndereco_Quando_AbrirASecao_Entao_DeveConvidarACadastrar` |
| CA-10 | `Dado_UmEnderecoPrincipal_Quando_CadastrarOSegundo_Entao_OPrimeiroDeveContinuarPrincipal` |
| CA-11 | `Dado_DoisEnderecos_Quando_MarcarOSegundo_Entao_OPrimeiroDeveDeixarDeSerPrincipal` |
| CA-12 | `Dado_EnderecosCadastrados_Quando_OlharALista_Entao_DeveIndicarOPrincipal` |
| CA-13 | `Dado_EnderecoCadastrado_Quando_EditarONumero_Entao_DevePersistir` |
| CA-14 | `Dado_DoisEnderecos_Quando_ExcluirOQueNaoEPrincipal_Entao_OPrincipalDeveContinuar` |
| CA-15 | `Dado_DoisEnderecos_Quando_ExcluirOPrincipal_Entao_ORestanteDeveAssumir` |
| CA-16 | `Dado_UnicoEndereco_Quando_Excluir_Entao_AListaDeveFicarVaziaEConvidar` |
| CA-17 | `Dado_EnderecoDeOutraPessoa_Quando_TentarAbrirEditarOuExcluir_Entao_NaoDeveConseguir` |
| CA-18 | `Dado_FormularioDeEndereco_Quando_InformarCepValido_Entao_DevePreencherOsDemaisCampos` |
| CA-19 | `Dado_CamposPreenchidosPeloCep_Quando_AlterarARua_Entao_ODigitadoDevePrevalecer` |
| CA-20 | `Dado_BuscaDeCepIndisponivel_Quando_PreencherAMaoESalvar_Entao_DeveCadastrarNormalmente` |
| CA-21 | `Dado_JavaScriptDesligado_Quando_CadastrarEndereco_Entao_DeveFuncionar` |

**CA-18 e CA-20 não podem bater no ViaCEP de verdade.** Um teste que depende de
serviço de terceiro falha quando a internet oscila, e passa a ser ignorado — que
é o pior destino de um teste. Os dois usam interceptação de rota do Playwright
(`Page.RouteAsync`) para responder no lugar do ViaCEP: CA-18 devolve um endereço
conhecido, CA-20 devolve falha. **É o único jeito de CA-20 existir**, aliás — não
há como derrubar o ViaCEP de propósito.

**CA-17 é o critério mais importante desta feature.** Ele é o único que separa
"funciona" de "é seguro", e é exercitado em dois níveis: unidade no controlador
e ponta a ponta com duas contas de verdade.

## 8. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Rota própria `/Endereco`, sem área de conta | Decisão explícita do responsável em contrário. E deixaria o atalho "Conta" apagado, adiando de novo a dívida que a `014` registrou |
| Conta nascendo só com endereços | Um atalho chamado "Conta" que leva a uma página só de endereços confunde. E a segunda seção custa uma tela, porque domínio e aplicação já existem desde a `004` |
| Invariante do principal dentro de `Endereco` | A entidade não conhece os irmãos. "Exatamente um principal" é propriedade da coleção, e forçá-la na entidade exigiria passar a lista inteira para ela — o que é o serviço disfarçado |
| Sem `DataCadastro`, promovendo um qualquer ao excluir o principal | "Um qualquer" não é regra, é acaso. E a lista ficaria sem ordem estável, mudando de posição entre visitas sem motivo |
| CPF como campo desabilitado | Campo apagado convida a tentar. Texto simples diz "isto não muda" sem oferecer um controle morto (RN-08) |
| ViaCEP consultado pelo servidor | Exigiria cliente HTTP, tempo limite configurado, tratamento de falha em duas camadas e uma rota nova — para entregar o mesmo preenchimento, e ainda assim precisar do mesmo piso manual |
| Validar se o endereço existe de verdade | Fora de escopo declarado. O CEP é conferido no formato; conferir se o número existe naquela rua exige serviço pago e não foi pedido |
| `IEnderecoRepository.BuscarPorId(enderecoId)` sozinho | Deixaria a RN-05 dependente de o controlador lembrar de conferir o dono. Buscar sempre pelo par torna o esquecimento impossível |

## 9. Riscos

| Risco | Prob. | Impacto | Mitigação |
|---|---|---|---|
| **Endereço de outra pessoa alcançável pelo identificador** — a falha clássica de CRUD por id | Média | **Alto** | O contrato não oferece busca só por id: todo método recebe `usuarioId` e o repositório busca pelo par. CA-17 exercita em dois níveis. É o risco de segurança desta feature |
| **A invariante do principal quebra em caminho não previsto** — dois principais, ou nenhum com endereços existindo | Média | Alto | As quatro regras (RN-01 a RN-04) têm teste de serviço dedicado, e o teste de integração confere o estado final no banco depois de cada operação |
| **O ViaCEP muda de formato ou sai do ar** | Baixa | Baixo | A RN-07 já o torna dispensável por desenho. Os testes não dependem dele (interceptação de rota), então a suíte não quebra junto |
| **A máscara de CEP atrapalha a busca** — o campo formatado com hífen sendo enviado ao serviço | Média | Baixo | O script tira a formatação antes de consultar, e `CepHelper.ApenasDigitos` já normaliza na gravação. Um teste de unidade fixa isso |
| **A migration falha por `DataCadastro` obrigatória sem valor** | Baixa | Médio | A tabela está vazia; não há linha para preencher. Conferido antes de aplicar |
| **Reabilitar o atalho "Conta" quebra o teste da `014`** que prova que ele está desabilitado | Alta | Baixo | Quebra esperada. O teste é **reescrito**, não removido: passa a provar que o atalho leva à conta |
| **A validação de data de nascimento diverge entre cadastro e edição** | Média | Médio | `DadosPessoaisDTOValidator` reaproveita as mesmas regras de `CadastroDTOValidator` em vez de reescrevê-las. Um teste compara os dois comportamentos para o mesmo valor inválido |

## 10. Desvios constitucionais justificados

*Nenhum.*

Diferente da `016` e da `017`, esta feature tem dois formulários com campos de
texto de verdade, preenchidos por pessoas — então o Princípio III se aplica
inteiro, sem ressalva: `EnderecoDTOValidator` e `DadosPessoaisDTOValidator`
protegem quem digita, os construtores e métodos de `Endereco` e `Usuario`
protegem o dado, e a duplicação entre as duas barreiras é a esperada.

O Princípio VII também se aplica sem desvio: não há caminho assíncrono nesta
feature. Toda escrita é POST comum com antiforgery, guarda de `ModelState` e
redirecionamento no sucesso. A única coisa que o JavaScript faz é preencher
campos — e a RN-07 garante que ele nunca é necessário.

---

## Sobre a spec seguinte

**`019` — Fechamento.** Decidida na mesma conversa que originou a `017` e esta.
`Produto` ganha `Peso`, `Altura`, `Largura` e `Comprimento`, com migration,
campos no cadastro e valores para os 100 produtos semeados. Frete cotado pelo
MelhorEnvio, com CEP de origem em configuração e token como segredo — calculável
apenas **depois** do endereço escolhido, que é o que esta feature entrega. O
fechamento grava `Pedido` (`Status = Pendente`, `PagamentoAprovado = false`,
`Valor = itens + frete`), um `ItemPedido` por linha com `PrecoUnitario`
congelado, e um `Pagamento` com o método escolhido e status `Pendente`. O
carrinho é esvaziado.

Duas coisas que esta feature deixa explicitamente para lá: **o que a tela faz
quando se tenta excluir um endereço já usado por um pedido** (spec §10), e **a
terceira seção da conta, "Meus pedidos"**, que só faz sentido quando houver
pedido para listar.
