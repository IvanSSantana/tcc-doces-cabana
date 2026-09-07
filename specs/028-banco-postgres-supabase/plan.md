# Plano Técnico — Banco de dados no Postgres do Supabase

**Spec:** [`spec.md`](./spec.md) · **Status:** Implementado — ver `checklist.md`
**Criado em:** 2026-09-07

---

## 1. Resumo da abordagem

**Um provider só, em todo lugar.** `Microsoft.EntityFrameworkCore.Sqlite` sai,
`Npgsql.EntityFrameworkCore.PostgreSQL` entra, e a troca em si é uma linha:
`UseSqlite` → `UseNpgsql` em `DbContextDependencyInjection`. Nenhum provider
condicional, nenhuma migration duplicada, nenhum `if` por ambiente.

**A suíte ganha um Postgres descartável, não um Postgres compartilhado.**
`Testcontainers.PostgreSql` sobe um contêiner por projeto de teste; os testes
de integração criam um banco novo por teste dentro dele, e o E2E entrega a
connection string ao processo filho pela variável de ambiente que
`AplicacaoEmExecucao` **já usa** para apontar o SQLite descartável de hoje.

**As 14 migrations são apagadas e uma nasce no lugar.** Foram geradas pelo
provider do SQLite e não podem ser aplicadas no Postgres. Não há dado a
preservar (spec §10).

**Três coisas escritas em specs passadas atravessam a troca sem alteração, e
uma some.** `ProdutoStatus` ficou deliberadamente sem `HasColumnType` ("o
provider mapeia o enum de byte sozinho"); a busca normaliza caixa e acento em
C# via `TextoHelper`, em vez de depender do `LIKE` do banco; e o domínio usa
`DateTime.UtcNow` em toda parte, que é exatamente o que o `timestamptz` do
Npgsql exige. A que some é o `UPPER()` dos testes crus de E2E — ele existia
porque o SQLite guarda `Guid` como texto maiúsculo, e no Postgres a coluna é
`uuid` de verdade.

## 2. Verificação constitucional

| Princípio | Situação |
|---|---|
| **I — Direção de dependência** | ✅ A troca é toda dentro de `Infrastructure`. Nenhum projeto passa a referenciar outro; `Npgsql` entra exatamente onde o `Sqlite` estava |
| **II — Domínio se defende** | ✅ Nenhuma entidade muda |
| **III — Duas barreiras** | ✅ Nenhum validator e nenhuma invariante mudam |
| **IV — Português** | ✅ `InfraestruturaPostgresDescartavel` substitui `InfraestruturaSqliteEmMemoria` — o nome volta a dizer o que a classe é |
| **V — Teste antes** | ⚠️ Parcialmente inaplicável, e o desvio está justificado por escrito no §11 |
| **VI — Persistência escondida** | ✅ Nenhum `DbContext` sai de `Infrastructure`. Mapeamentos seguem em `Configurations`. Migration nova, versionada, com nome descritivo em inglês (`InitialCreatePostgres`) |
| **VII — Seguro na borda** | ✅ Nenhuma ação de controller muda. A connection string carrega senha e vai para *user secrets*, com o exemplo em branco no versionamento |
| **VIII — Dono do erro** | ✅ Nenhum caminho de erro da aplicação muda |

## 3. Direção visual

Nenhuma. Esta entrega não tem interface — e é a primeira do projeto de que se
pode dizer isso literalmente. Se qualquer pixel mudar, é regressão (RN-01).

## 4. Impacto por camada

### `DocesCabana.Domain`

Nenhuma mudança.

### `DocesCabana.Application`

Nenhuma mudança.

### `DocesCabana.Infrastructure`

| Arquivo | Mudança |
|---|---|
| `DocesCabana.Infrastructure.csproj` | `EntityFrameworkCore.Sqlite` → `Npgsql.EntityFrameworkCore.PostgreSQL`; some o pin de `SQLitePCLRaw.lib.e_sqlite3` |
| `DependencyInjections/DbContextDependencyInjection.cs` | `UseSqlite` → `UseNpgsql` |
| `Migrations/` | as 14 (mais os `.Designer.cs` e o snapshot) apagadas; uma `InitialCreatePostgres` gerada |

**O único aviso de build do projeto morre nesta entrega.** O pin de
`SQLitePCLRaw.lib.e_sqlite3 2.1.12` na `Infrastructure` existe só para tapar a
vulnerabilidade `GHSA-2m69-gcr7-jv3q` da versão transitiva que o provider do
SQLite arrasta. O `DocesCabana.Tests.E2E` **não tem esse pin** — ele referencia
`Microsoft.Data.Sqlite` (só o driver, sem EF Core), que arrasta a mesma cadeia
na versão vulnerável, e é exatamente por isso que o `NU1903` aparece hoje
atribuído àquele projeto em todo build. Removido o SQLite dos dois, some a
cadeia inteira: some o aviso e some a necessidade do pin.

### `DocesCabana.MVC`

| Arquivo | Mudança |
|---|---|
| `appsettings.Example.json` | connection string no formato Npgsql, **com a senha em branco** |
| `Helpers/DbInitializer.cs` | nenhuma. `Database.Migrate()` e a semeadura seguem iguais |

`DbInitializer` não muda uma linha, e isso é resultado, não sorte: ele já
gatilha a semeadura por `if (!context.Produtos.Any())` e já cria as datas com
`DateTime.UtcNow`.

### `DocesCabana.Tests`

| Arquivo | Mudança |
|---|---|
| `DocesCabana.Tests.csproj` | `EntityFrameworkCore.Sqlite` → `Npgsql.EntityFrameworkCore.PostgreSQL` + `Testcontainers.PostgreSql` |
| `Integration/InfraestruturaSqliteEmMemoria.cs` | vira `InfraestruturaPostgresDescartavel.cs` |
| `Integration/PostgresDeTeste.cs` | **novo** — fixture de coleção que sobe o contêiner uma vez |

Os 60 testes de integração **não mudam**, e essa é a razão da escolha de
isolamento descrita no §5.

### `DocesCabana.Tests.E2E`

| Arquivo | Mudança |
|---|---|
| `DocesCabana.Tests.E2E.csproj` | ganha `Npgsql` + `Testcontainers.PostgreSql`; perde `Microsoft.Data.Sqlite` |
| `Infraestrutura/AplicacaoEmExecucao.cs` | `CaminhoDoBanco` (caminho de arquivo) vira `ConexaoDoBanco` (connection string) |
| `Fluxos/CarrinhoTests.cs` | 1 `SqliteConnection` → `NpgsqlConnection` |
| `Fluxos/PaginaInicialTests.cs` | 3 `SqliteConnection` → `NpgsqlConnection` |

## 5. O isolamento entre testes

**Um contêiner por projeto de teste, um banco novo por teste.**

Subir um contêiner por teste seria absurdo (60 contêineres). Compartilhar um
banco entre os 60 quebraria a premissa com que eles foram escritos — hoje cada
teste recebe um SQLite em memória virgem, porque o xUnit instancia a classe de
teste uma vez por teste e o `InitializeAsync` roda junto.

Criar um **banco** novo por teste dentro do mesmo contêiner preserva essa
premissa exatamente. É a opção com menor raio de explosão: **nenhum dos 60
testes precisa ser reescrito**, nem repensado.

Descartadas: schema por teste (exigiria mexer em `HasDefaultSchema` e no
`EnsureCreated`) e `TRUNCATE` entre testes (mais rápido, mas obriga a conhecer
e resetar toda tabela e sequência — otimização antes de haver problema). Se os
~20s estimados incomodarem depois, o `TRUNCATE` é o plano B, registrado aqui e
não construído agora.

O E2E é diferente e mais simples: ele já compartilha **uma** instância da
aplicação entre a suíte inteira (`ColecaoE2E`), então precisa de um contêiner e
um banco só. A entrega da connection string ao processo filho já existe:

```csharp
infoProcesso.Environment["ConnectionStrings__DefaultConnection"] = /* … */;
```

É literalmente a mesma linha que hoje aponta para o arquivo SQLite.

## 6. A mensagem quando falta o Docker

O `Testcontainers` lança exceção de conexão com o daemon quando o Docker
Desktop está desligado — mensagem que descreve o sintoma (`npipe`, `daemon`) e
não a causa. O fixture captura e relança dizendo o que fazer, e o que a suíte
**não** faz:

> Não foi possível subir o Postgres de teste. O Docker Desktop precisa estar em
> execução — a suíte sobe um contêiner descartável e não usa banco nenhum da sua
> máquina nem do Supabase.

Mesmo padrão de `AplicacaoEmExecucao.MontarMensagemDeFalha`, que já anexa
`stdout`/`stderr` do processo filho em vez de deixar o timeout falar sozinho.
É a RN-05 aplicada: das duas vezes em que este projeto pagou caro por erro de
ambiente — `UserAgent` vazio na `020`, `UrlBase` vazia na `027` —, o custo foi
sempre a mensagem descrever o sintoma.

## 7. Os quatro acessos crus do E2E

Três testes cutucam o banco por fora, para montar cenário que não tem tela
(mudar status de produto) ou para conferir agregação (nota média, quantidade
vendida). Hoje são assim:

```csharp
"UPDATE Produto SET Status = $status WHERE UPPER(ProdutoId) = UPPER($id)"
```

O `UPPER()` dos dois lados tem motivo escrito no comentário: o EF grava o
`Guid` como texto maiúsculo no SQLite, e a comparação de texto é sensível a
caixa. No Postgres `ProdutoId` é coluna `uuid`, e a comparação passa a ser
direta, com o `Guid` como parâmetro. **Some a ginástica.**

Em troca, entram aspas: identificador sem aspas no Postgres vira minúsculo, e
o EF cria `"Produto"`, `"Status"`, `"ItemPedido"`. E os parâmetros trocam de
`$nome` (Microsoft.Data.Sqlite) para `@nome` (Npgsql).

## 8. A conexão com o Supabase

A tarefa mais arriscada da entrega, e por isso a primeira — antes de qualquer
código, no mesmo formato da `T003` da `027`: **provar o serviço externo antes
de escrever a linha que depende dele.**

O host direto (`db.<ref>.supabase.co`) resolve para IPv6 em projetos novos, e
boa parte de rede doméstica no Windows não alcança. O caminho que funciona é o
**pooler em modo Session** (IPv4), com TLS exigido. A forma é esta — **o host e
o usuário exatos saem do painel**, não deste documento, porque a Supabase já
mudou o formato do hostname do pooler mais de uma vez:

```
Host=<host-do-pooler>;Port=5432;Database=postgres;
Username=postgres.<ref>;Password=<senha>;SSL Mode=Require
```

Modo Session e não Transaction (porta 6543) de propósito: o modo Transaction
não suporta *prepared statements*, que o Npgsql usa por padrão, e exigiria
desligá-los na connection string. Menos configuração é menos coisa para
explicar depois.

A senha vai para *user secrets* (`ConnectionStrings:DefaultConnection`), ao
lado de `SupabaseSettings:ChaveDeServico` que a `027` já colocou lá.

**A versão do Postgres do contêiner deve ser a mesma do Supabase.** Sem isso a
suíte valida um motor e a loja roda outro — que é o defeito que esta entrega
existe para fechar, reintroduzido pela porta dos fundos. Descobre-se com
`select version()` e fixa-se a imagem (`postgres:17-alpine`, ou o que
corresponder).

## 9. Estratégia de teste

| Camada | O que muda |
|---|---|
| `Units/` (619 testes) | Nada. Não tocam banco |
| `Integration/` (60 testes) | O motor por baixo. As asserções seguem, **exceto** as de ordenação alfabética, que podem legitimamente mudar (§10) |
| `E2E/` (185 testes) | O motor por baixo, mais os quatro acessos crus reescritos |
| Migration | Sem teste próprio. `DbInitializer.Migrate()` roda na subida do E2E — se a migration nova estiver quebrada, **185 testes falham de uma vez**, o que é sinal suficiente |

**Sobre o Princípio V nesta entrega.** Não há teste novo a escrever antes do
código: o que se está fazendo é trocar o substrato de 245 testes que já
existem e já são a especificação executável do comportamento. O ciclo
vermelho-verde aqui é invertido e vale mais: a suíte inteira está verde antes,
tem que ficar verde depois, e **cada teste que ficar vermelho no meio é um
achado** — ou uma diferença real entre os motores (registra-se e decide-se), ou
um defeito da migração (conserta-se). A justificativa constitucional está no
§11.

Exceção: a mensagem de Docker ausente (RF-09) **é** comportamento novo, e tem
teste próprio escrito antes — verificando que a mensagem cita o que ligar.

## 10. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| **Manter os testes no SQLite** (híbrido) | Suíte continuaria em 2s e sem pré-requisito. Recusado porque obrigaria a aplicação a suportar dois providers em tempo de execução e **dois conjuntos de migrations** — complexidade permanente, paga em toda mudança de esquema futura — e porque deixaria os testes de persistência provando um motor que ninguém executa (spec §10) |
| **Testes contra o Supabase remoto** | 60 testes criando e destruindo esquema pela rede, mais 185 de E2E com ida e volta por clique. Lento, frágil, e exigiria credencial para `dotnet test` rodar — mata o "clonou, rodou" que a RN-02 protege |
| **Postgres instalado nativo no Windows** | Dispensa Docker e é mais rápido, mas exige instalação manual por máquina e uma senha de superusuário guardada na configuração de teste — o mesmo defeito que fez descartar a alternativa acima. O contêiner gera credencial própria a cada execução |
| **Manter as 14 migrations e adicionar por cima** | Impossível, não é escolha: foram geradas para o SQLite. A `AddProdutoPesoEDimensoes` chega a rodar `UPDATE Produto` sem aspas, que no Postgres procura uma tabela `produto` que não existe |
| **Duas pastas de migration, uma por provider** | É o caminho documentado do EF para multi-provider, e existiria só para sustentar o híbrido já recusado |
| **Trocar `Promocao` para usar o novo motor de outra forma, ou mexer em esquema de passagem** | Fora de escopo por RN-01. A entrega não tem permissão de mudar comportamento |

## 11. Desvios constitucionais justificados

**Princípio V — "nenhuma tarefa de implementação começa sem o teste
correspondente escrito e falhando".**

Cumprido para o único comportamento novo desta entrega (a mensagem de
pré-requisito ausente, RF-09). **Não cumprível para o resto**, e a razão é
estrutural: não existe teste a escrever antes de trocar o provider — o
comportamento alvo é "exatamente o que os 245 testes existentes já afirmam". A
disciplina equivalente, e que substitui o ciclo aqui, está no §9: suíte verde
antes, suíte verde depois, e cada vermelho no meio tratado como achado a
registrar, nunca como ruído a contornar.

Registrado explicitamente porque o Princípio V não abre exceção sozinho — a
Governança exige que o desvio seja escrito, não presumido.
