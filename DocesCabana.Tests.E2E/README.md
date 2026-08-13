# DocesCabana.Tests.E2E

Testes de ponta a ponta em navegador real, via Playwright. Percorrem os
fluxos que as specs `001` a `006` entregaram — cadastro, login, recuperação
de senha, cadastro de produto e gestão de administradores — clicando e
preenchendo como uma pessoa faria, contra a aplicação de verdade subindo num
processo filho, com um SQLite descartável e um adaptador de e-mail que grava
em arquivo (nunca contra a base do dia a dia — ver `specs/007-testes-e2e-com-playwright/plan.md`).

## Instalação (uma vez por máquina)

O pacote `Microsoft.Playwright` já vem pela restauração normal do projeto;
falta só o navegador, que não é um pacote NuGet.

**Não use `playwright.ps1`** — ele exige PowerShell 7 (`pwsh`), que não é o
PowerShell 5.1 que vem por padrão no Windows, e o erro resultante ("comando
`pwsh` não encontrado") não deixa claro que o problema é o navegador ausente.

Instale pela via programática, com um projeto de console descartável:

```powershell
mkdir $env:TEMP\instalador-playwright
cd $env:TEMP\instalador-playwright
dotnet new console --framework net10.0
dotnet add package Microsoft.Playwright --version 1.62.0
'Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });' | Out-File -Encoding utf8 Program.cs
dotnet run
cd ..
Remove-Item -Recurse -Force instalador-playwright
```

O navegador fica em `%LOCALAPPDATA%\ms-playwright`, fora do repositório —
uma instalação serve para qualquer clone e qualquer branch.

## Rodando

```powershell
# Ciclo rápido de desenvolvimento — só as suítes de unidade e integração,
# sem subir navegador nem aplicação
dotnet test --filter "Categoria!=E2E"

# Suíte E2E inteira — sobe a aplicação uma vez, roda os ~17 testes, derruba
dotnet test DocesCabana.Tests.E2E
```

`dotnet test` puro, na raiz da solução, roda as duas suítes juntas — mais
lento, mas garante que ninguém que abra a solução na IDE deixe de ver o
projeto. Prefira o primeiro comando no dia a dia.

## Se um teste falhar

A falha vem com `stdout`/`stderr` da aplicação quando o problema é ela não
ter subido, e com um rastro do Playwright (`bin/<Config>/net10.0/rastros-e2e/*.zip`)
quando o problema é no fluxo em si. Abra o rastro com:

```powershell
pwsh # ou powershell mesmo
npx playwright show-trace caminho\para\o\rastro.zip
```

(o `show-trace` do próprio pacote também funciona sem `npx`, chamando
`Microsoft.Playwright.Program.Main(new[] { "show-trace", caminho })` do
mesmo jeito que a instalação acima.)

## O que não é coberto

O caminho de envio real de e-mail por SMTP continua sem teste automatizado —
o adaptador de arquivo (`EmailServiceArquivo`) prova que o token é gerado, o
link é montado e a senha troca, não que o `.NET` fala SMTP corretamente.
Decisão registrada em `specs/007-testes-e2e-com-playwright/plan.md` §4.
