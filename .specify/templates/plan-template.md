# Plano Técnico — [NOME DA FEATURE]

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** [AAAA-MM-DD]
**Status:** Rascunho | Aprovado | Executado

---

## 1. Resumo da abordagem

*3 a 5 frases: o caminho escolhido, em alto nível. Alguém que leu a spec deve
terminar este parágrafo sabendo por onde o código vai passar.*

## 2. Verificação constitucional

Preencha **antes** de detalhar o desenho. Um ❌ exige justificativa na seção 9 ou
mudança de abordagem.

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ⬜ OK / ❌ | |
| II | Domínio rico e auto-validante | ⬜ OK / ❌ / n/a | |
| III | Validação nas duas barreiras | ⬜ OK / ❌ / n/a | |
| IV | Nomenclatura em português | ⬜ OK / ❌ | |
| V | Testes escritos antes | ⬜ OK / ❌ | |
| VI | Repositório + commit via UnitOfWork | ⬜ OK / ❌ / n/a | |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ⬜ OK / ❌ / n/a | |
| VIII | Tratamento de erro por camada | ⬜ OK / ❌ | |

## 3. Impacto por camada

### `DocesCabana.Domain`
| Arquivo | Ação | O quê |
|---|---|---|
| `Entities/X.cs` | criar / alterar | |

### `DocesCabana.Application`
| Arquivo | Ação | O quê |
|---|---|---|
| `DTOs/XDTO.cs` | | |
| `Contracts/Services/IXService.cs` | | |
| `Services/XService.cs` | | |
| `Validators/XDTOValidator.cs` | | |
| `Mappings/XMapper.cs` | | |

### `DocesCabana.Infrastructure`
| Arquivo | Ação | O quê |
|---|---|---|
| `Repositories/XRepository.cs` | | |
| `DatabaseContext/Configurations/XConfiguration.cs` | | |
| `Migrations/` | | |
| `DependencyInjections/ApplicationDependencyInjection.cs` | | registro no contêiner |

### `DocesCabana.MVC`
| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/XController.cs` | | |
| `Views/X/Y.cshtml` | | |
| `ViewComponents/X.cs` | | |
| `wwwroot/css/pages/x.css` | | |

## 4. Contratos

*Assinaturas novas ou alteradas de interface. Só assinatura — a implementação é
tarefa, não plano.*

```csharp
public interface IXService
{
    Task<XDTO> Fazer(XDTO dto);
}
```

## 5. Modelo de dados

*Só se a feature toca o esquema.*

- **Entidade:** campos, tipos, obrigatoriedade
- **Relacionamentos:** cardinalidade e comportamento de exclusão
- **Migration:** nome proposto — `dotnet ef migrations add [Nome] --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC`
- **Impacto em dados existentes:** [nenhum | precisa de backfill | destrutivo]

## 6. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — entidade | `Units/Entities/XTests.cs` | invariantes RN-xx |
| Unidade — serviço | `Units/Services/XServiceTests.cs` | RF-xx com repositório mockado |
| Unidade — validator | `Units/Validators/XDTOValidatorTests.cs` | cada `RuleFor` |
| Unidade — controller | `Units/Controllers/XControllerTests.cs` | ModelState, redirecionamento |
| Integração | `Integration/Repositories/XRepositoryIntegrationTests.cs` | persistência real em SQLite |

Mapeamento critério → teste:

| Critério de aceite | Teste que o prova |
|---|---|
| CA-01 | `Dado_..._Quando_..._Entao_...` |

## 7. Alternativas descartadas

| Alternativa | Por que não |
|---|---|

## 8. Riscos

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|

## 9. Desvios constitucionais justificados

*Vazio é o resultado esperado. Se houver item aqui, ele precisa dizer por que a
alternativa conforme foi descartada, não só que o desvio existe.*

| Princípio | Desvio | Justificativa | Alternativa descartada |
|---|---|---|---|
