# Plano Técnico — Envio de imagem do produto

**Spec:** [`spec.md`](./spec.md) · **Status:** Rascunho
**Criado em:** 2026-09-05

---

## 1. Resumo da abordagem

**Um adaptador novo, no mesmo formato do que já existe.** `IFreteService` /
`FreteServiceMelhorEnvio` (spec `020`) estabeleceu a forma: contrato na
`Application`, implementação na `Infrastructure` com `HttpClient` tipado,
configuração por `IOptions`, credencial fora do versionamento, e **falha de
transporte que não lança**. `IArmazenamentoDeImagem` /
`ArmazenamentoSupabase` é o mesmo desenho aplicado a outro serviço.

**O contrato fala `Stream`, não `IFormFile`.** `IFormFile` é do ASP.NET, e a
`Application` só referencia o `Domain` (Princípio I). Quem abre o arquivo é a
MVC; o que atravessa a fronteira é `Stream`, que é BCL.

**O endereço continua sendo o mesmo campo de sempre.** `Produto.ImagemUrl`
segue `string` de 255, `IsRequired` — **sem migration, sem coluna nova**. O que
muda é a origem do texto: antes vinha do formulário, agora vem do resultado do
envio. Medido: o endereço público do Supabase com nome em `Guid` ocupa 92
caracteres.

**O envio acontece antes de gravar, e falha impede a gravação.** Não há estado
intermediário: ou a imagem subiu e o produto nasce com endereço válido, ou nada
acontece e a tela volta com o motivo.

## 2. Verificação constitucional

| Princípio | Situação |
|---|---|
| **I — Direção de dependência** | ✅ `IArmazenamentoDeImagem` na `Application`, implementado na `Infrastructure`. O contrato usa `Stream` (BCL) justamente para não arrastar `Microsoft.AspNetCore.Http` para dentro da `Application` |
| **II — Domínio se defende** | ✅ Nenhuma entidade muda. `Produto.ImagemUrl` já era obrigatória e validada no construtor |
| **III — Duas barreiras** | ✅ Formato e tamanho do arquivo em `ImagemParaEnvioDTOValidator` (barreira de entrada); `Produto` segue recusando `ImagemUrl` vazia no construtor (invariante) |
| **IV — Português** | ✅ `IArmazenamentoDeImagem.Enviar`, `ResultadoDoEnvioDeImagemDTO`, `ImagemParaEnvioDTO`, `SupabaseSettings` |
| **V — Teste antes** | ✅ Ciclo vermelho-verde. O adaptador é testado com `HttpMessageHandler` falso, como `FreteServiceMelhorEnvioTests` |
| **VI — Persistência escondida** | ✅ Nenhuma mudança de esquema, nenhuma migration. A gravação segue por `ProdutoService.Cadastrar` e o `IUnitOfWork` que ele já usa |
| **VII — Seguro na borda** | ✅ A ação continua `[HttpPost]` com `[ValidateAntiForgeryToken]`, aguardada, com guarda de `ModelState` e redirecionamento no sucesso. `[Authorize(Roles = Administrador)]` já está na classe. A chave de serviço nunca chega ao navegador: o envio é servidor a servidor |
| **VIII — Dono do erro** | ✅ Arquivo inválido e falha de envio são erro esperado do usuário — viram `ModelState`, não exceção. `Enviar` nunca lança por falha de transporte |

## 3. Direção visual

O campo de texto do endereço vira um seletor de arquivo, na mesma posição e com
a mesma moldura dos demais campos do formulário (`.campo-entrada`,
`.campo-texto`, `.mensagem-erro`). Nada de área de arrastar-e-soltar, nada de
pré-visualização: o formulário desta tela é uma coluna de campos, e um bloco
grande de upload quebraria o ritmo dela por conveniência que ninguém pediu.

A mensagem de recusa aparece abaixo do campo, como as demais.

## 4. Impacto por camada

### `DocesCabana.Domain`

Nenhuma mudança.

### `DocesCabana.Application`

| Arquivo | Mudança |
|---|---|
| `Contracts/Services/IArmazenamentoDeImagem.cs` | **novo** |
| `DTOs/ResultadoDoEnvioDeImagemDTO.cs` | **novo** |
| `DTOs/ImagemParaEnvioDTO.cs` | **novo** — só metadados; o conteúdo não passa pelo validador |
| `Validators/ImagemParaEnvioDTOValidator.cs` | **novo** |
| `DTOs/ProdutoDTO.cs` | Ganha `ComImagem(string)` — cópia com o endereço preenchido |

```csharp
public interface IArmazenamentoDeImagem
{
    /// <summary>
    /// Nunca lança por falha de transporte: credencial ausente, serviço fora
    /// do ar ou arquivo recusado voltam no resultado (RN-03, Princípio VIII).
    /// O nome que chega é o do computador de quem enviou e serve só para
    /// derivar a extensão — quem nomeia o arquivo guardado é o adaptador
    /// (RN-02).
    /// </summary>
    Task<ResultadoDoEnvioDeImagemDTO> Enviar(Stream conteudo, string nomeDoArquivoOriginal, string contentType);
}

public record ResultadoDoEnvioDeImagemDTO(bool Sucesso, string? Url, string? Mensagem)
{
    public static ResultadoDoEnvioDeImagemDTO ParaSucesso(string url) => new(true, url, null);
    public static ResultadoDoEnvioDeImagemDTO ParaFalha(string mensagem) => new(false, null, mensagem);
}

public record ImagemParaEnvioDTO(string NomeDoArquivo, string ContentType, long TamanhoEmBytes);
```

**`ProdutoDTO.ComImagem` existe porque as propriedades do DTO são `init`.**
Atribuir `dto.ImagemUrl` depois do binding não compila — e não deveria mesmo: o
próprio `ProdutoDTO` carrega, desde que foi escrito, o comentário de que "o uso
do init se dá pois o DTO é imutável e não deve ser alterado após a criação".
A cópia não é contorno da linguagem, é respeito a uma decisão registrada.
`CarrinhoDTO.ComCotacao` (spec `020`) resolveu o mesmo problema do mesmo jeito.

**Formato e tamanho são constantes do validador, não configuração.** São regra
de negócio ("o que a loja aceita publicar"), e como constante ficam testáveis
sem subir configuração nenhuma. Aceitos: `.jpg`, `.jpeg`, `.png`, `.webp`,
com o `Content-Type` correspondente; teto de **5 MB**.

### `DocesCabana.Infrastructure`

| Arquivo | Mudança |
|---|---|
| `Services/SupabaseSettings.cs` | **novo** — `UrlBase`, `Bucket`, `Pasta`, `ChaveDeServico`, `TimeoutEmSegundos` |
| `Services/ArmazenamentoSupabase.cs` | **novo** |
| `DependencyInjections/ApplicationDependencyInjection.cs` | `Configure<SupabaseSettings>` e `AddArmazenamentoDeImagem()`, no formato de `AddFreteService()` |

A API do Storage é REST simples, e o projeto já tem `HttpClient` tipado
funcionando contra outro serviço — **não entra SDK novo por um endpoint**:

```
POST {UrlBase}/storage/v1/object/{Bucket}/{Pasta}/{guid}{extensão}
     Authorization: Bearer {ChaveDeServico}
     Content-Type: {contentType}
     corpo: os bytes

público: {UrlBase}/storage/v1/object/public/{Bucket}/{Pasta}/{guid}{extensão}
```

**Chave vazia é recusada sem tocar a rede.** Se `ChaveDeServico` estiver em
branco, `Enviar` devolve falha na hora, com mensagem própria. Isso torna o
CA-09 determinístico e faz a suíte de ponta a ponta rodar offline — a lição que
a spec `020` aprendeu tarde, quando um `UserAgent` em branco derrubou a página
em vez de recusar.

`UrlBase`, `Bucket` e `Pasta` vão versionados (não são segredo, como
`FreteSettings.CepDeOrigem`). `ChaveDeServico` fica em *user secrets* no
desenvolvimento e em variável de ambiente no resto (RN-04), com o campo vazio
em `appsettings.Example.json`.

**A chave é a `service_role`, e ela nunca sai do servidor.** O arquivo vai do
navegador para a aplicação, e da aplicação para o Storage — o navegador nunca
vê a credencial. É o que permite enviar sem política de acesso por usuário.

### `DocesCabana.MVC`

| Arquivo | Mudança |
|---|---|
| `Areas/Admin/Controllers/ProdutoController.cs` | `Cadastro` (POST) recebe também o arquivo, valida, envia e só então cadastra |
| `Areas/Admin/Views/Produto/Cadastro.cshtml` | Campo de arquivo no lugar do campo de endereço; **`enctype="multipart/form-data"` no formulário** |
| `Helpers/DbInitializer.cs` | `ImagensDeExemplo` passa a apontar para o Storage |

## 5. O caminho do arquivo, ponta a ponta

```
Cadastro(ProdutoDTO dto, IFormFile? imagem)
 1. arquivo ausente ou vazio          → erro em ModelState["imagem"]
 2. formato ou tamanho recusados      → erro em ModelState["imagem"]
    (ImagemParaEnvioDTOValidator, sobre metadados — o conteúdo não é lido)
 3. ModelState.Remove("ImagemUrl")    → o campo saiu do formulário; quem o
                                        preenche é o servidor
 4. ModelState inválido               → recarrega subcategorias, volta a view
                                        com o que já foi digitado (RF-05)
 5. Enviar(stream, nome, contentType)
    falhou                            → ModelState geral com a mensagem do
                                        resultado, volta a view (RF-08)
 6. dto.ComImagem(url)
 7. ProdutoService.Cadastrar          → o caminho que já existe
 8. TempData + RedirectToAction       → POST-Redirect-Get, como já era
```

**O passo 3 tem precedente literal.** `ContaController.AlterarDados` repõe o
CPF no DTO quando o resto falhou, porque ele "nunca veio do que a pessoa
digitou" (CA-07 da spec `018`). `ImagemUrl` está na mesma situação a partir
desta entrega: é campo de saída do servidor, não de entrada do usuário, e o
erro que o binding levanta sobre ele não descreve nada de real.

**O erro do arquivo mora na chave `imagem`**, lida na view por
`ViewData.ModelState["imagem"]` — mesmo caminho que `_ItensDoCarrinho.cshtml`
usa para o erro de CEP (spec `020`), já que também ali o campo não é
propriedade do modelo da view.

## 6. A massa de demonstração

`DbInitializer.ImagensDeExemplo` troca os seis links de pré-visualização do
Drive pelos seis endereços públicos do bucket. **A semeadura não envia nada** —
grava texto, como sempre gravou. Consequência boa: `dotnet test`, o E2E e quem
clonar o projeto seguem semeando sem credencial nenhuma (RF-11).

Os endereços são públicos e ficam versionados, exatamente como os do Drive
ficam hoje.

## 7. Estratégia de teste

| Camada | Casos |
|---|---|
| `Units/Services/ArmazenamentoSupabaseTests.cs` | `HttpMessageHandler` falso, como `FreteServiceMelhorEnvioTests`: envio bem-sucedido devolve o endereço público montado corretamente; **o caminho enviado usa `Guid` e a extensão do original, nunca o nome recebido** (RN-02, CA-07); `Authorization: Bearer` e `Content-Type` vão na requisição; `401`/`4xx`/`5xx` devolvem falha com mensagem **sem lançar**; **chave em branco recusa sem fazer requisição nenhuma** (CA-09) |
| `Units/Services/ArmazenamentoSupabaseCaminhosDeFalhaTests.cs` | `HttpClient` real contra endereço que recusa conexão (`http://localhost:9`) e contra endereço não roteável com timeout curto — não lança, devolve falha. Mesmo par que a `020` usa |
| `Units/Validators/ImagemParaEnvioDTOValidatorTests.cs` | Extensão fora da lista recusada; `Content-Type` fora da lista recusado; acima de 5 MB recusado; caso válido aceito (CA-03, CA-04) |
| `Units/Controllers/Admin/ProdutoControllerTests.cs` *(já existe)* | Sem arquivo → `ModelState` inválido e o armazenamento **nunca é chamado** (CA-02); arquivo inválido → idem (CA-03/CA-04); envio falhou → volta a view e `IProdutoService.Cadastrar` **nunca roda** (CA-08); sucesso → o endereço devolvido chega ao DTO e redireciona (CA-06) |
| `Integration/` | Nenhum teste novo: não há mudança de esquema, e o caminho de gravação é o que `ProdutoRepository` já cobre |
| `E2E/Fluxos/CadastroDeProdutoTests.cs` | O caminho feliz passa a `[Trait("Categoria", "Externo")]` — só roda com credencial no ambiente. Entra **um teste novo na suíte padrão**: sem credencial, o cadastro recusa com a mensagem de "não configurado" e nenhum produto é criado (CA-09). Os testes de tela (título, contenção, largura estreita) e o de preço inválido seguem na suíte padrão sem mudança |
| `E2E/Paginas/PaginaCadastroProduto.cs` | `Preencher` deixa de escrever endereço e passa a anexar arquivo por `SetInputFilesAsync` com um PNG mínimo em memória (`FilePayload`), sem depender de arquivo no disco |
| `E2E/Infraestrutura/AplicacaoEmExecucao.cs` | Repassa `SupabaseSettings__ChaveDeServico` do ambiente de quem executa, quando presente — mesmo mecanismo que `FreteSettings__Token` já usa |

## 8. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| **Adaptador local para desenvolvimento e teste** (como `EmailServiceArquivo`) | Recomendado ao especificar e **recusado pelo responsável**. Manteria a suíte inteira rodando sem credencial; o custo aceito no lugar é um teste de ponta a ponta fora da suíte padrão e a área administrativa inutilizável sem configuração |
| **Manter o campo de endereço como alternativa ao arquivo** | Duas origens de imagem convivendo para sempre, e não resolve a fragilidade — continuaria possível colar link de pré-visualização. Além disso é o controle "ou/ou" que a regra herdada da `017`/`021` existe para evitar |
| **Subir pelo painel do Supabase e colar o endereço** | Trocaria um serviço de terceiro por outro sem construir nada: sem envio, sem verificação de formato, e ainda dependente de alguém colar o endereço certo |
| **Bucket privado com endereço assinado** | Medido nos endereços fornecidos: 384 caracteres contra teto de 255, e validade de 150 dias. Exigiria alargar a coluna e assinar de novo a cada exibição — uma chamada por produto por listagem, por um sigilo que foto de vitrine não pede |
| **Guardar a imagem no banco** | Transformaria o banco em servidor de arquivo, e a coluna que existe é de endereço, não de conteúdo |
| **SDK oficial do Supabase para .NET** | Uma dependência nova para um `POST`. O projeto já tem `HttpClient` tipado funcionando contra serviço externo (`FreteServiceMelhorEnvio`), e o formato da requisição cabe em dez linhas |
| **Inspecionar os bytes do arquivo** (*magic numbers*) | A tela exige papel de administrador; a verificação por extensão e `Content-Type` cobre o engano honesto, que é o caso real. Registrado como fora de escopo, não como esquecimento |

## 9. Riscos

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| **Esquecer `enctype="multipart/form-data"`** | Média | Alto | O arquivo chega `null` e o sintoma vira "imagem obrigatória", que não descreve a causa. O teste da suíte padrão afirma a mensagem **específica** de "não configurado" — com o `enctype` faltando, ele falha com outra mensagem e aponta para cá |
| **A chave de serviço vazar para o navegador** | Baixa | Alto | Ela só é lida na `Infrastructure`, dentro do adaptador. Nada em view, nada em JavaScript. O arquivo trafega navegador → aplicação → Storage |
| **Endereço público estourar os 255 da coluna** | Baixa | Médio | Medido: 92 caracteres com nome em `Guid`. A folga só some se `Bucket` ou `Pasta` receberem nomes absurdos — ambos versionados e sob controle |
| **A massa de demonstração perder as imagens** | Média | Baixo | Risco declarado na spec §10. Mesmo risco do Drive hoje, num lugar que a loja controla |
| **Arquivo grande travando a requisição** | Baixa | Médio | Teto de 5 MB verificado **antes** de abrir o stream, pelos metadados. Bem abaixo do limite padrão do Kestrel |
| **Cadastro impossível sem credencial** | Alta | Médio | Consequência aceita da decisão registrada na spec §10. Mitigado pela recusa explícita e imediata, com mensagem que diz o que está faltando — não um erro genérico |

## 10. Desvios constitucionais justificados

*Nenhum.*

Sem entidade nova, sem mudança de esquema, sem migration. A ação de escrita
mantém antiforgery, autorização por papel, `await`, guarda de `ModelState` e
redirecionamento no sucesso. A falha de serviço externo é erro esperado do
usuário, tratada por `ModelState` como o Princípio VIII determina — e o
contrato do adaptador diz isso por escrito, como o de frete já diz.
