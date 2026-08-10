<#
.SYNOPSIS
    Cria a pasta de uma nova feature em specs/ a partir dos templates de SDD.

.DESCRIPTION
    Descobre o próximo número livre em specs/, gera o slug a partir da descrição,
    cria a pasta specs/NNN-slug com spec.md, plan.md e tasks.md preenchidos com
    o nome da feature, e opcionalmente cria a branch de mesmo nome.

.PARAMETER Descricao
    Descrição curta da feature, em português. Ex.: "listagem de produtos admin".

.PARAMETER CriarBranch
    Cria e faz checkout da branch NNN-slug a partir da branch atual.

.EXAMPLE
    .\.specify\scripts\nova-feature.ps1 "listagem de produtos admin"

.EXAMPLE
    .\.specify\scripts\nova-feature.ps1 "controle de estoque" -CriarBranch
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Descricao,

    [switch]$CriarBranch
)

$ErrorActionPreference = 'Stop'

$raiz = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$pastaSpecs = Join-Path $raiz 'specs'
$pastaTemplates = Join-Path $raiz '.specify\templates'

if (-not (Test-Path $pastaTemplates)) {
    throw "Templates não encontrados em $pastaTemplates. Você está na raiz do repositório?"
}

if (-not (Test-Path $pastaSpecs)) {
    New-Item -ItemType Directory -Path $pastaSpecs | Out-Null
}

# --- Próximo número livre -----------------------------------------------------
$maior = 0
Get-ChildItem -Path $pastaSpecs -Directory -ErrorAction SilentlyContinue |
    ForEach-Object {
        if ($_.Name -match '^(\d{3})-') {
            $n = [int]$Matches[1]
            if ($n -gt $maior) { $maior = $n }
        }
    }
$numero = '{0:D3}' -f ($maior + 1)

# --- Slug ---------------------------------------------------------------------
# Remove acentos via normalização Unicode (decompõe e descarta as marcas),
# baixa a caixa e troca não-alfanumérico por hífen.
$decomposto = $Descricao.Normalize([Text.NormalizationForm]::FormD)
$semAcento = -join ($decomposto.ToCharArray() | Where-Object {
    [Globalization.CharUnicodeInfo]::GetUnicodeCategory($_) -ne
        [Globalization.UnicodeCategory]::NonSpacingMark
})
$slug = $semAcento.ToLowerInvariant()
$slug = [Regex]::Replace($slug, '[^a-z0-9]+', '-')
$slug = $slug.Trim('-')

if ([string]::IsNullOrWhiteSpace($slug)) {
    throw "Não foi possível gerar um slug a partir de '$Descricao'."
}

# Limita a 5 palavras para o nome de branch não ficar quilométrico.
$partes = $slug -split '-' | Where-Object { $_ } | Select-Object -First 5
$slug = $partes -join '-'

$id = "$numero-$slug"
$pastaFeature = Join-Path $pastaSpecs $id

if (Test-Path $pastaFeature) {
    throw "A pasta $pastaFeature já existe."
}

New-Item -ItemType Directory -Path $pastaFeature | Out-Null

# --- Cópia dos templates ------------------------------------------------------
$hoje = Get-Date -Format 'yyyy-MM-dd'

# Só a primeira letra em maiúscula: title case capitalizaria preposições
# ("Teste De Validação"), o que não é a convenção do português.
$titulo = $Descricao.Trim()
$titulo = $titulo.Substring(0, 1).ToUpperInvariant() + $titulo.Substring(1)

# Escreve sem BOM: os .md são lidos por renderizadores, não pelo PowerShell.
$utf8SemBom = New-Object Text.UTF8Encoding $false

$mapa = @{
    'spec-template.md'      = 'spec.md'
    'plan-template.md'      = 'plan.md'
    'tasks-template.md'     = 'tasks.md'
    'checklist-template.md' = 'checklist.md'
}

foreach ($origem in $mapa.Keys) {
    $caminhoOrigem = Join-Path $pastaTemplates $origem
    $caminhoDestino = Join-Path $pastaFeature $mapa[$origem]

    $conteudo = Get-Content -Path $caminhoOrigem -Raw -Encoding UTF8
    $conteudo = $conteudo -replace '\[NOME DA FEATURE\]', $titulo
    $conteudo = $conteudo -replace '\[NNN-slug-da-feature\]', $id
    $conteudo = $conteudo -replace '\[NNN-slug\]', $id
    $conteudo = $conteudo -replace '\[AAAA-MM-DD\]', $hoje

    [IO.File]::WriteAllText($caminhoDestino, $conteudo, $utf8SemBom)
}

Write-Host "Feature criada: specs/$id" -ForegroundColor Green
Write-Host "  spec.md, plan.md, tasks.md, checklist.md"

# --- Branch -------------------------------------------------------------------
if ($CriarBranch) {
    git -C $raiz checkout -b $id
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Não foi possível criar a branch $id."
    }
    else {
        Write-Host "Branch criada: $id" -ForegroundColor Green
    }
}
else {
    Write-Host "Para criar a branch:  git checkout -b $id" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "Próximo passo: preencher specs/$id/spec.md (ou rodar /especificar)." -ForegroundColor Cyan
Write-Host "Não adicione a linha no índice à mão — atualize specs/README.md ao final." -ForegroundColor DarkGray
