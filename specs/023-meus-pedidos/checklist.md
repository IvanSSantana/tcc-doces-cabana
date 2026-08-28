# Checklist de conclusão — Meus pedidos

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente — RF-01 (`_MenuDaConta.
      cshtml`), RF-02 a RF-05 (`Meus.cshtml`, `PedidoService.
      ListarDoUsuario`), RF-06 a RF-10 (`Detalhe.cshtml`, `PedidoService.
      BuscarDetalhe`), RF-11/RF-12 (`IPedidoRepository.Buscar`)
- [x] Todo `CA-xx` foi verificado — **CA-01 a CA-09 por teste automatizado**
      (unidade, integração e E2E, em navegador real, sobre os pedidos
      semeados). **A única coisa que não pôde ser percorrida manualmente**
      é comprar de verdade e ver o pedido novo aparecer — esta entrega é só
      leitura e depende do fechamento (`022`), que depende da credencial do
      MelhorEnvio, ainda não obtida (`020` §10). Não é uma lacuna desta
      entrega em si: tudo o que ela precisa mostrar, ela mostra
      corretamente sobre os pedidos que já existem.
- [x] Nada fora do escopo declarado entrou junto na entrega — cancelar/
      alterar pedido, repetir compra, avançar situação, rastrear entrega,
      segunda via, paginação/filtro e visão administrativa seguem de fora
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

## Constituição

- [x] **I** — Nenhuma referência nova entre projetos que inverta a direção
      de dependência
- [x] **II** — Nenhuma entidade nova, nem mudança de esquema — entrega de
      leitura pura
- [x] **III** — Não se aplica: nenhum dado é recebido do usuário além do
      identificador na rota
- [x] **IV** — Nomes, mensagens e comentários em português
      (`PedidoService.ListarDoUsuario`/`BuscarDetalhe`, `ResumoDePedidoDTO`,
      `DetalheDePedidoDTO`)
- [x] **V** — Os testes foram escritos antes e vistos falhar antes de
      passar (compilação: `Meus`/`Detalhe`/`Buscar`/`ListarDoUsuario`/
      `BuscarDetalhe` inexistentes)
- [x] **VI** — Nenhuma escrita nesta entrega; sem `IUnitOfWork`, sem
      migration
- [x] **VII** — `[Authorize]` na classe de `PedidoController` (já estava lá
      desde a `022`) — nenhuma ação nova nasce desprotegida
- [x] **VIII** — Sem `try/catch` em ação de controller. Pedido alheio ou
      inexistente lança `KeyNotFoundException`, que o `FilterException`
      global traduz para 404

## Testes

- [x] `dotnet build` sem warnings novos (só o aviso pré-existente do pacote
      SQLite, alheio a esta entrega)
- [x] `dotnet test` verde — `DocesCabana.Tests`: 657/657;
      `DocesCabana.Tests.E2E`: 185/185 (suíte completa, do zero)
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] Feature que toca persistência tem teste de integração —
      `PedidoRepositoryIntegrationTests` (o detalhe vem numa consulta só,
      com itens, produto de cada item e endereço; `Buscar` de pedido
      alheio devolve nulo)

## Interface

- [x] `asp-action` de cada formulário aponta para uma ação que existe de
      fato — não há formulário nesta entrega (só leitura); os links
      (`Meus`, `Detalhe`, o atalho do menu) apontam para ações reais
- [x] Não se aplica: nenhum campo de entrada do usuário nesta entrega
- [x] Testado em largura de tela pequena — a lista e o detalhe reaproveitam
      o mesmo `.pagina-conta`/`.corpo-conta` que a `018` já prova em
      375px; não introduziu layout de página novo
- [x] Valores monetários e datas formatados em `pt-BR` (`N2`/`dd/MM/yyyy`)
      em toda a lista e o detalhe

## Segurança

- [x] Nenhum segredo commitado — nenhuma credencial nova nesta entrega
- [x] Entrada do usuário não é interpolada em HTML sem escape — Razor
      padrão em toda view nova, sem `Html.Raw`
- [x] Mensagens de erro não vazam existência de conta nem detalhe interno —
      pedido alheio e inexistente devolvem o mesmo 404, sem distinguir os
      dois casos (RN-01)
