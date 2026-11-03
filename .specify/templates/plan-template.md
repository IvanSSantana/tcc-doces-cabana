# Plano — [NOME DA FEATURE]

**ID:** [NNN-slug] · **Criado em:** [AAAA-MM-DD]
**Status:** Rascunho | Aprovado | Executado

---

## 1. Objetivo

*2 a 4 frases: o problema, o que muda para quem usa ou mantém o sistema e por
que agora. Quem não participou do debate deve entender o pedido só por aqui.*

## 2. Decisões

*Tudo o que foi fechado no debate, mais o que o plano decidiu ao ler o código.
Cada decisão diz o que se escolheu e por quê. Uma decisão sem "por quê" é
palpite registrado.*

| # | Decisão | Por quê | Alternativa descartada |
|---|---|---|---|
| D1 | | | |

**Fora de escopo:** *o que foi discutido e ficou de fora, para não voltar
pela porta dos fundos na execução.*

## 3. Verificação constitucional

Preencha **antes** de detalhar o desenho. Um ❌ exige justificativa na seção 8
ou mudança de abordagem.

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

## 4. Impacto arquivo por arquivo

*Um arquivo por linha, com caminho exato. "Ajustar a aplicação" não é plano;
`Application/Services/ProdutoService.cs: injetar IUnitOfWork` é. Omita as
camadas que a feature não toca.*

### `DocesCabana.Domain`
| Arquivo | Ação | O quê |
|---|---|---|

### `DocesCabana.Application`
| Arquivo | Ação | O quê |
|---|---|---|

### `DocesCabana.Infrastructure`
| Arquivo | Ação | O quê |
|---|---|---|

### `DocesCabana.MVC`
| Arquivo | Ação | O quê |
|---|---|---|

### Testes
| Arquivo | Ação | O quê |
|---|---|---|

## 5. Contratos e modelo de dados

*Só assinaturas novas ou alteradas. Migration, se houver:*
`dotnet ef migrations add [Nome] --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC`

## 6. Ordem de execução

*Passos em ordem de dependência (Domínio → Aplicação → Infraestrutura → MVC).
Teste antes da implementação quando o passo muda comportamento. Marque `[x]`
na execução, depois de a suíte ficar verde.*

- [ ] 1.
- [ ] N. `dotnet build` e `dotnet test` completos

## 7. Testes e verificação

| O que precisa ser provado | Como |
|---|---|
| | teste `Dado_..._Quando_..._Entao_...` / verificação manual |

## 8. Riscos e desvios

| Risco ou desvio constitucional | Impacto | Mitigação ou justificativa |
|---|---|---|

## 9. Resultado da execução

*Preenchido na execução: o que passou, o que não foi feito e por quê, o que
precisa de verificação manual.*
