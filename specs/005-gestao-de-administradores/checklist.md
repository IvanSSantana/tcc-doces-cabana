# Checklist de conclusão — Gestão de administradores

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente
- [x] Todo `CA-xx` foi verificado ao vivo contra a aplicação rodando:
      CA-01 (lista traz o administrador semeado), CA-02 (cadastro novo aparece
      com confirmação), CA-03 (administrador recém-criado acessa `/Admin` e
      `/Administrador` sem negação), CA-04 (e-mail duplicado — mensagem exata
      "Os dados informados já estão associados a uma conta existente.", nada
      gravado), CA-05 (CPF duplicado — `UsuarioService` apaga a `ContaDeAcesso`
      já criada; e-mail da tentativa não fica no sistema), CA-06 (senha
      "senha123" recusada pela validação de entrada, sem chegar ao serviço),
      CA-07 (visitante redirecionado para `/Autenticacao/Login`), CA-08
      (cliente comum redirecionado para `/Home/AcessoNegado`), CA-09 (link
      "Administradores" ausente do cabeçalho para cliente comum)
- [x] Nada fora do escopo declarado entrou junto na entrega
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

## Constituição

- [x] **I** — Nenhuma referência nova entre camadas fora do sentido permitido.
      `AdministradorController` (MVC) depende só de `IAdministradorService`
      (Infra); `AdministradorService` depende de `UserManager`,
      `IUsuarioRepository` e `IUsuarioService` — mesma exceção já registrada
      pela `004`
- [x] **II** — Nenhuma entidade de domínio nova nesta spec; `Papeis` é uma
      constante, não uma entidade
- [x] **III** — Reaproveita `CadastroDTOValidator` sem alteração — mesma
      barreira de entrada do cadastro de cliente (RF-04)
- [x] **IV** — Nomes em português; `Papeis.Administrador` mantém o valor
      literal existente
- [x] **V** — T005–T007 escritos e vermelhos antes de T009–T018
      (confirmado em T008)
- [x] **VI** — Gravação passa por `IUnitOfWork.SalvarAlteracoes`
      (`UsuarioService`, inalterado); nenhuma migration nova — `Papeis` e o
      parâmetro `papel` não tocam o schema
- [x] **VII** — `[Authorize(Roles = Papeis.Administrador)]` na classe do
      controller; `[ValidateAntiForgeryToken]` no POST
- [x] **VIII** — A compensação de RN-05 é o mesmo `try/catch` da `004` em
      `UsuarioService`, agora também cobrindo a falha de `AddToRoleAsync`

## Testes

- [x] `dotnet build` sem warnings novos — 0 avisos
- [x] `dotnet test` verde — 250/250, acima da linha de base registrada na T002
      (herdada da `004`, já integrada em `main` antes desta branch nascer)
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] `AdministradorServiceTests` prova RF-01, RF-03 e RN-04 com mocks;
      `UsuarioServiceCadastroTests` prova RN-05/CA-05 (papel + compensação);
      `AdministradorControllerTests` prova RF-06, RF-07 e CA-02 no nível do
      controller

## Interface

- [x] `asp-action` de cada formulário aponta para a ação correta
      (`Cadastro`, `Index`)
- [x] Erros de validação aparecem junto ao campo (`asp-validation-for`) e um
      resumo geral para erros sem campo (duplicidade de e-mail/CPF)
- [ ] Testado em largura de tela pequena — não verificado nesta rodada
- [x] Sem valores monetários; data de nascimento segue o mesmo componente e
      máscara do cadastro de cliente

## Segurança

- [x] Nenhum segredo commitado — senha do administrador semeado continua em
      `dotnet user-secrets`
- [x] Entrada do usuário não é interpolada em HTML sem escape — Razor faz o
      escape por padrão, nenhuma view usa `Html.Raw`
- [x] Mensagens de erro não vazam existência de conta além do que a `002` já
      decidiu (mensagem genérica de duplicidade, sem dizer se foi o e-mail ou
      o CPF)
- [x] Área e link protegidos por papel, não por checagem de UI isolada —
      `[Authorize(Roles = ...)]` no controller é a barreira real; o `@if` no
      header é só cosmético (CA-08 continua valendo mesmo digitando a URL
      direto)
