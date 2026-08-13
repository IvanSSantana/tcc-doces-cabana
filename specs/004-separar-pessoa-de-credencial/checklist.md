# Checklist de conclusão — Separar pessoa de credencial

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente
- [x] Todo `CA-xx` foi verificado — CA-01, CA-02, CA-03 e CA-07 (suíte) ao vivo
      contra a aplicação rodando; CA-04 verificado por teste unitário
      dedicado (`UsuarioServiceCadastroTests`), não ao vivo — ver nota abaixo;
      CA-05 por teste de integração (`Include`/sem `Include`); CA-06
      parcialmente ao vivo (geração de token via `ContaDeAcesso` confirmada;
      envio de e-mail bloqueado pelo SMTP placeholder do ambiente, mesma
      limitação já documentada na `001`)
- [x] Nada fora do escopo declarado entrou junto na entrega
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

> **Nota sobre CA-04 (conta órfã desfeita):** o `AutenticacaoController.Cadastro`
> já faz uma pré-checagem via `BuscarPorLogin(dto.CPF)` **antes** de chamar
> `CadastrarUsuario` — então, numa requisição sequencial simples, a duplicidade
> de CPF é pega ali, com a mensagem amigável, e o caminho de compensação
> (`DeleteAsync`) da `UsuarioService` nunca chega a ser exercitado. Isso é
> correto e desejável: a UI nunca arrisca a corrida em uso normal. O caminho de
> compensação existe para a janela de corrida entre duas requisições
> simultâneas com o mesmo CPF (TOCTOU) — cenário que só um teste de unidade com
> mocks consegue forçar de forma determinística, e é exatamente o que
> `Dado_CpfJaCadastrado_Quando_CadastrarUsuario_Entao_DeveApagarAContaCriada`
> faz.

## Constituição

- [x] **I** — Nenhuma referência nova entre projetos. A navegação Infra → Domain
      (`ContaDeAcesso.Usuario`) é a mesma direção permitida já usada em toda a
      base; a exceção do Princípio I foi reescrita (emenda 1.2.0), não violada
- [x] **II** — `Usuario` do domínio: `private set`, construtor validante,
      `protected Ctor()`. `ContaDeAcesso` idem, com a validação de e-mail que já
      existia
- [x] **III** — `CadastroDTOValidator` inalterado, continua cobrindo os mesmos
      campos na entrada
- [x] **IV** — `Usuario` fica com o termo do negócio no domínio;
      `ContaDeAcesso` é nome técnico em português
- [x] **V** — Cada bloco (A a D) teve teste vermelho antes da implementação
- [x] **VI** — `IUsuarioRepository` novo; gravação do `Usuario` via
      `IUnitOfWork.SalvarAlteracoes`; uma migration versionada
      (`SepararPessoaDeCredencial`)
- [ ] **VII** — n/a nesta spec (nenhum controller muda de comportamento)
- [x] **VIII** — A compensação da RN-08 é `try/catch` no serviço, não em ação
      de controller

## Testes

- [x] `dotnet build` sem warnings novos — 0 avisos
- [x] `dotnet test` verde — 241/241 (baseline: 233)
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] Feature que toca persistência tem teste de integração —
      `ModeloDeDadosIntegrationTests` prova a navegação `Endereco.Usuario`
      nula sem `Include` e preenchida com `Include`; `DatabaseIntegrationTests`
      prova a atomicidade com CPF único no domínio

## Interface

- [ ] `asp-action` de cada formulário — n/a, nenhuma view alterada
- [ ] Erros de validação no campo — n/a, comportamento inalterado
- [ ] Testado em largura de tela pequena — n/a
- [ ] Valores monetários e datas formatados em `pt-BR` — n/a nesta spec

## Segurança

- [x] Nenhum segredo commitado
- [x] Entrada do usuário não é interpolada em HTML sem escape — inalterado
- [x] Mensagens de erro não vazam existência de conta — inalterado (RN-05/RN-06
      da `002` seguem valendo; nada nesta spec toca `EsqueceuSenha`)

---

## Achado registrado durante a execução

O achado de CA-04 acima (compensação só alcançável via corrida, não via UI
sequencial) não é um defeito — é a arquitetura funcionando como desenhada. A
pré-checagem do controller é a primeira linha de defesa (UX); a compensação do
serviço é a segunda (integridade sob concorrência). Registrado para que quem
revisar não estranhe a ausência de uma prova ao vivo desse caminho específico.
