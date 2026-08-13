# Plano Técnico — Duplicidade unificada no cadastro

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-13
**Status:** Rascunho

---

## 1. Resumo da abordagem

A regra "esse e-mail ou CPF já tem dono" desce um andar: sai dos controllers e
vira um método único no `IUsuarioService`, que os dois cadastros consultam antes
de gravar. A mensagem também deixa de ser literal repetido e vira constante
compartilhada na `Application`, visível para MVC e Infrastructure.

O `AdministradorController` ganha a guarda que hoje só o `AutenticacaoController`
tem — mas nenhum dos dois passa a *conhecer* a regra: ambos perguntam. Isso
mantém o erro esperado do usuário como `ModelState`, e não como exceção, que é o
que o Princípio VIII pede.

A exceção continua existindo, só que para o que é de fato excepcional: a corrida
entre duas requisições simultâneas com o mesmo CPF, que a consulta prévia não
pega por construção. Nesse caminho, hoje, o usuário recebe "erro interno" — o
plano traduz a falha do índice único para a mesma mensagem das outras, para que
a RF-04 valha também na corrida.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ✅ OK | A constante de mensagem vai para a `Application`, que MVC e Infrastructure já referenciam. `IUsuarioService` continua onde está, pela exceção já documentada |
| II | Domínio rico e auto-validante | ⬜ n/a | Nenhuma entidade nova nem invariante nova. A unicidade de CPF já é índice desde a `004` |
| III | Validação nas duas barreiras | ✅ OK | Duplicidade não é formato — é estado do banco, então não cabe no validator de entrada. Fica na barreira de aplicação, com o índice único como rede de baixo. É exatamente o par que o Princípio III descreve: a regra em duas camadas, não em uma só |
| IV | Nomenclatura em português | ✅ OK | `ContaJaExiste`, `MensagensCadastro.DadosJaAssociados` |
| V | Testes escritos antes | ✅ OK | Fase 2 das tarefas, antes de qualquer Fase 3 |
| VI | Repositório + commit via `UnitOfWork` | ✅ OK | Nenhuma escrita nova; a consulta usa `IUsuarioRepository`. Sem migration: o índice único de CPF existe desde a `004` |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ✅ OK | Nenhuma ação nova; as existentes mantêm atributos e fluxo |
| VIII | Tratamento de erro por camada | ✅ OK | **É o ponto da feature.** Duplicidade é erro esperado → `ModelState` no controller. Corrida é excepcional → exceção no serviço, traduzida pelo `FilterException` |

Nenhuma emenda constitucional necessária.

## 3. Impacto por camada

### `DocesCabana.Domain`

Nenhuma alteração.

### `DocesCabana.Application`

| Arquivo | Ação | O quê |
|---|---|---|
| `Mensagens/MensagensCadastro.cs` | **criar** | `public const string DadosJaAssociados = "Os dados informados já estão associados a uma conta existente.";`. Hoje o literal está escrito duas vezes, em projetos diferentes — é assim que mensagem diverge |

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `Identity/Services/IUsuarioService.cs` | alterar | Acrescentar `Task<bool> ContaJaExiste(string email, string cpf)` |
| `Identity/Services/UsuarioService.cs` | alterar | Implementar `ContaJaExiste` reaproveitando `BuscarPorLogin`; trocar o literal pela constante; traduzir a falha do índice único de CPF para a mesma mensagem (ver §4) |

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/AdministradorController.cs` | alterar | Guarda de duplicidade antes de cadastrar — é a correção do defeito |
| `Controllers/AutenticacaoController.cs` | alterar | Trocar a dupla chamada a `BuscarPorLogin` por `ContaJaExiste` e o literal pela constante. Comportamento idêntico ao de hoje |

### `DocesCabana.Tests`

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Services/UsuarioServiceCadastroTests.cs` | alterar | `ContaJaExiste` nos dois sentidos; corrida de CPF vira mensagem amigável, não erro interno |
| `Units/Controllers/AdministradorControllerTests.cs` | alterar | Duplicidade não chama o cadastro e devolve a view com o erro — o teste que faltava e que teria pego o defeito |
| `Units/Controllers/AutenticacaoControllerTests.cs` | alterar | Ajustar os dois testes que hoje mockam `BuscarPorLogin` para mockar `ContaJaExiste` |

### Documentação herdada da auditoria

| Arquivo | Ação | O quê |
|---|---|---|
| `specs/005-gestao-de-administradores/plan.md` | alterar | §3 credita RN-05 a `AdministradorServiceTests` (é `UsuarioServiceCadastroTests`); §6 nomeia dois testes que nunca existiram |

## 4. Contratos

```csharp
// Application/Mensagens/MensagensCadastro.cs — novo
public static class MensagensCadastro
{
    public const string DadosJaAssociados =
        "Os dados informados já estão associados a uma conta existente.";
}

// Infrastructure/Identity/Services/IUsuarioService.cs — alterado
Task<bool> ContaJaExiste(string email, string cpf);
```

### Por que a guarda fica no controller, e não só no serviço

O caminho mais curto seria deixar o serviço lançar e o `FilterException`
transformar em `ModelState`. Funciona — foi assim que o e-mail duplicado
continuou dando a mensagem certa no cadastro de administrador — mas trata um
erro rotineiro do usuário como exceção, e o Princípio VIII diz o contrário com
todas as letras: *"Erro esperado do usuário vira `ModelState.AddModelError`, não
exceção."* Na prática isso também enche o log de `LogError` para alguém que só
digitou um CPF que já existe.

A guarda no controller com a **pergunta** no serviço resolve os dois lados: a
regra continua num lugar só (RN-03), e a apresentação do erro fica onde a
constituição a quer.

### A corrida, que a consulta prévia não pega

`ContaJaExiste` é consulta-e-depois-age: entre a pergunta e a gravação, outra
requisição pode inserir o mesmo CPF. O índice único barra — e é ele quem garante
a integridade, não a consulta. O que muda é a mensagem:

```csharp
// UsuarioService.CadastrarUsuario, esboço da parte nova.
// `cpf` é o dígito puro — CpfHelper.ApenasDigitos(dto.CPF!) —, o mesmo formato
// que a entidade grava e que o índice único enxerga.
catch (DbUpdateException)
{
    // Confirma que foi mesmo colisão de CPF antes de rotular como duplicidade:
    // um DbUpdateException por outro motivo não pode virar "dados já usados".
    // A checagem é por consulta, não por código de erro do banco — o dev roda
    // SQLite e o deploy vai para SQL Server, e os códigos não coincidem.
    var colisaoDeCpf = await _usuarioRepository.BuscarPorCpf(cpf) is not null;

    await _userManager.DeleteAsync(conta);

    if (colisaoDeCpf)
        throw new InvalidOperationException(MensagensCadastro.DadosJaAssociados);

    throw;
}
```

A consulta vem **antes** do `DeleteAsync` de propósito: consultar não descarrega
o `ChangeTracker`, e `DeleteAsync` grava. Ver o risco correspondente na §8.

## 5. Modelo de dados

Nenhuma mudança de esquema e nenhuma migration. O índice único de `Usuario.CPF`
existe desde a `004` e é o que já barra a duplicidade no banco.

## 6. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — serviço | `Units/Services/UsuarioServiceCadastroTests.cs` | `ContaJaExiste` responde certo para e-mail, para CPF e para nenhum dos dois; a corrida de CPF vira mensagem amigável e não deixa credencial |
| Unidade — controller | `Units/Controllers/AdministradorControllerTests.cs` | RF-01 a RF-05 no cadastro de administrador |
| Unidade — controller | `Units/Controllers/AutenticacaoControllerTests.cs` | RF-01 a RF-05 no cadastro de cliente — prova a não-regressão |

Não há teste de integração novo: a feature não muda esquema, e o índice único de
CPF já é provado por
`Dado_DuasAlteracoesUmaInvalidaParaOBanco_Quando_SalvarAlteracoes_Entao_NenhumaDevePersistir`
em `DatabaseIntegrationTests`.

Mapeamento critério → teste:

| Critério | Teste que o prova |
|---|---|
| CA-01 | `Dado_CpfJaUsado_Quando_CadastroPost_Entao_DeveRetornarViewComErroSemCadastrar` (Administrador) |
| CA-02 | `Dado_EmailJaUsado_Quando_CadastroPost_Entao_DeveRetornarViewComErroSemCadastrar` (Administrador) |
| CA-03 | `Dado_CpfJaUsado_Quando_CadastroPost_Entao_DeveAdicionarErroERetornarView` (Autenticacao) |
| CA-04 | `Dado_EmailJaUsado_Quando_CadastroPost_Entao_DeveAdicionarErroERetornarView` (Autenticacao) |
| CA-05 | `Dado_CorridaDeCpf_Quando_CadastrarUsuario_Entao_DeveApagarAContaEDarMensagemAmigavel` |
| CA-06 | Testes de caminho feliz já existentes nos dois controllers, mantidos verdes |

## 7. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Só copiar a pré-checagem do `AutenticacaoController` para o `AdministradorController` | Conserta o sintoma e preserva a causa: duas cópias da mesma regra, que foi o que produziu a divergência. A RN-03 pede o contrário |
| Deixar a duplicidade sempre como exceção, sem guarda no controller | Contraria o Princípio VIII e transforma erro rotineiro de digitação em `LogError`. Ver §4 |
| Traduzir `DbUpdateException` para "dados já usados" sem confirmar a causa | Qualquer outra falha de gravação passaria a mentir para o usuário, dizendo que o dado é repetido quando não é |
| Identificar a colisão pelo código de erro do provedor (SQLite 19 / SQL Server 2601) | Amarra o código ao banco. O ambiente de desenvolvimento é SQLite e o de deploy é SQL Server; a consulta de confirmação funciona nos dois |
| Dizer qual campo está repetido | Fora de escopo por decisão de segurança da `002`, registrada na RN-02 da spec |

## 8. Riscos

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| `DeleteAsync` reprocessar o `Usuario` que ficou pendente no `ChangeTracker` após o `SalvarAlteracoes` falhar, e falhar também — deixando a credencial órfã que a compensação deveria remover | Média | **Alto** | Foi conferido no banco da fumaça da `005` que hoje a conta **é** removida, então o cenário não se manifesta no caminho atual. Como o novo `catch` insere uma consulta antes do `DeleteAsync`, o teste de corrida da T003 passa a exigir a remoção nesse caminho específico, e a fumaça da T014 confere no banco — não por inspeção da tela |
| Remover a pré-checagem do `AutenticacaoController` regredir o cadastro de cliente | Baixa | Alto | A guarda não é removida, é **trocada** por uma equivalente que pergunta ao serviço. Os testes de cadastro de cliente existentes são ajustados, não apagados, e a T014 confere ao vivo |
| A constante de mensagem mudar de texto e quebrar asserção de teste | Baixa | Baixo | Os testes passam a asseverar contra `MensagensCadastro.DadosJaAssociados`, não contra o literal |
| Escopo crescer para "revisar todas as mensagens do sistema" | Média | Médio | A §8 da spec fecha isso explicitamente |

## 9. Desvios constitucionais justificados

Nenhum.
