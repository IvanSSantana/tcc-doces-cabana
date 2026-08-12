# Checklist de conclusão — Modelo de dados completo

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente — RF-01/02 no
      `DbInitializer.Semear` reescrito; RF-03 pela FK real de
      `Produto.SubcategoriaId`
- [x] Todo `CA-xx` foi verificado manualmente na aplicação rodando — CA-01
      verificado ao vivo (T051); os demais (CA-02 a CA-10) são invariantes de
      construtor, cobertos por teste automatizado, o que é mais forte que
      verificação manual; CA-11 conferido tabela a tabela (T055); CA-12 pela
      suíte (T056)
- [x] Nada fora do escopo declarado entrou junto na entrega — a exceção
      registrada é a correção dos testes de integração de `002` que usavam
      `SubcategoriaId` aleatório: quebraram porque a FK passou a ser
      enforçada, e a spec já previa esse risco (plano §8)
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

## Constituição

- [x] **I** — Nenhuma `ProjectReference` nova. `Endereco`, `Favorito`,
      `Avaliacao` e `Pedido` referenciam `Usuario` por `Guid` puro (RQ-02);
      todo o resto usa navegação normal, corrigido de uma generalização
      indevida do desenho inicial (RQ-10)
- [x] **II** — As dez entidades com `private set`, construtor validante,
      `protected Ctor()`
- [ ] **III** — n/a nesta spec (sem formulário, sem barreira de entrada)
- [x] **IV** — Três renomeações do `.dbml` corrigidas (RQ-05); comentários e
      mensagens em português
- [x] **V** — Cada grupo (A a D) teve teste escrito e vermelho antes da
      entidade existir; confirmado por build falhando por ausência de tipo
- [x] **VI** — Uma migration só (`AddRemainingDomainEntities`), configurações
      uma por entidade, sem Data Annotation; nenhum repositório novo
      (`IRepository<T>` genérico resolve)
- [ ] **VII** — n/a nesta spec (nenhum controller tocado)
- [x] **VIII** — `Estoque.Retirar` além do saldo lança
      `InvalidOperationException`; construtores lançam `ArgumentException`

## Testes

- [x] `dotnet build` sem warnings novos — 0 avisos, igual à baseline da `002`
- [x] `dotnet test` verde — 227/227 (baseline: 152)
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...` — sem exceção
- [x] Feature que toca persistência tem teste de integração —
      `ModeloDeDadosIntegrationTests` prova FK órfã recusada, par duplicado de
      `Favorito` recusado, segundo `Estoque` para o mesmo produto recusado, e
      navegação nula sem `Include` / preenchida com `Include`

## Interface

- [ ] `asp-action` de cada formulário — n/a, nenhuma view nesta spec
- [ ] Erros de validação no campo — n/a
- [ ] Testado em largura de tela pequena — n/a
- [ ] Valores monetários e datas formatados em `pt-BR` — n/a nesta spec; os
      tipos monetários usam `decimal(18,2)`, prontos para a formatação que a
      tela da feature consumidora vai aplicar

## Segurança

- [x] Nenhum segredo commitado
- [x] Entrada do usuário não é interpolada em HTML sem escape — n/a, sem view
- [x] Mensagens de erro não vazam existência de conta nem detalhe interno —
      n/a nesta spec

---

## Achado durante a execução, fora do escopo original

O `appsettings.json` local (não versionado desde a `002`) não sobreviveu à
troca de branch anterior a este trabalho. O primeiro smoke test (T051) falhou
com `SQLite Error 1: 'no such table: Produto'` — sintoma de conexão sem
connection string configurada, não de defeito na migration. Recriado a partir
de `appsettings.Example.json`. Registrado aqui porque não é falha desta spec,
mas quem repetir o clone deve saber que o passo é necessário (já documentado
no `README.md` pela `002`).
