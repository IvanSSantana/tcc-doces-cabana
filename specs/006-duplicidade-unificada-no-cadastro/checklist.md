# Checklist de conclusão — Duplicidade unificada no cadastro

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente
- [x] Todo `CA-xx` foi verificado manualmente na aplicação rodando: CA-01 e
      CA-02 (cadastro de administrador recusa CPF e e-mail duplicados com a
      mensagem certa), CA-03 e CA-04 (mesma checagem no cadastro de cliente,
      sem regressão), CA-05 (verificado **no banco**, não só na tela: o e-mail
      da tentativa recusada por CPF não entrou no sistema, e o mesmo e-mail
      funcionou depois com um CPF livre), CA-06 (cadastro válido segue
      funcionando nas duas portas)
- [x] Nada fora do escopo declarado entrou junto na entrega
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

## Constituição

- [x] **I** — Nenhuma referência nova que inverta a direção de dependência.
      `MensagensCadastro` fica na `Application`, que MVC e Infrastructure já
      referenciam; `ContaJaExiste` fica em `IUsuarioService`, na mesma exceção
      documentada desde a `004`
- [x] **II** — n/a nesta spec; nenhuma entidade de domínio nova
- [x] **III** — Duplicidade não é formato de campo, é estado do banco — por
      isso vive na barreira de aplicação (`ContaJaExiste`), com o índice único
      de CPF como a barreira de baixo que pega a corrida
- [x] **IV** — `ContaJaExiste`, `MensagensCadastro.DadosJaAssociados`, em
      português
- [x] **V** — T003–T006 escritos e vermelhos (erro de compilação por ausência
      de `MensagensCadastro`/`ContaJaExiste`) antes de qualquer T007 em diante
- [x] **VI** — Nenhuma escrita nova; `ContaJaExiste` é consulta via
      `IUsuarioRepository`/`UserManager`. Sem migration: o índice único de CPF
      já existe desde a `004`
- [x] **VII** — Nenhuma ação nova; as alteradas (`Cadastro` POST dos dois
      controllers) mantêm `[ValidateAntiForgeryToken]`, `await` e a guarda de
      `ModelState` que já tinham
- [x] **VIII** — A guarda de duplicidade é `ModelState.AddModelError`, não
      exceção — é o ponto central da feature (ver plano §4). A tradução da
      corrida de CPF continua sendo exceção, tratada no serviço, não em ação
      de controller

## Testes

- [x] `dotnet build` sem warnings novos — 0 avisos
- [x] `dotnet test` verde — 257/257 (baseline da T002: 250)
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] Nenhum teste de integração novo — a feature não muda esquema; o índice
      único de CPF já é provado por
      `Dado_DuasAlteracoesUmaInvalidaParaOBanco_Quando_SalvarAlteracoes_Entao_NenhumaDevePersistir`
      em `DatabaseIntegrationTests`, que continua valendo

## Interface

- [x] `asp-action` de cada formulário aponta para uma ação que existe de fato
      — nenhuma view foi alterada nesta feature
- [x] Erros de validação aparecem no campo (`asp-validation-for`) e no resumo;
      o erro de duplicidade usa o resumo geral (`ModelState[string.Empty]`),
      igual ao padrão já usado por `AutenticacaoController` antes desta feature
- [ ] Testado em largura de tela pequena — não verificado nesta rodada; nenhuma
      view mudou
- [x] n/a — sem valores monetários ou datas novas na tela

## Segurança

- [x] Nenhum segredo commitado
- [x] Entrada do usuário não é interpolada em HTML sem escape — inalterado
- [x] Mensagens de erro não vazam existência de conta nem detalhe interno —
      é exatamente o que esta feature corrige: a mensagem de duplicidade
      deixou de expor "erro interno" (que sugeria falha do sistema) e passou a
      dizer o que de fato aconteceu, sem revelar qual campo colidiu (RN-02)
