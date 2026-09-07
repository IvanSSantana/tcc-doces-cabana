# tcc-doces-cabana
Aplicação com .NET 10 que visa criar um sistema de e-commerce para a loja de doces de Barra Bonita chamada "Doces Cabana".

## Configuração local

`DocesCabana.MVC/appsettings.json` não é versionado (contém, ou pode vir a
conter, credenciais de SMTP). Ao clonar o repositório:

```powershell
Copy-Item DocesCabana.MVC/appsettings.Example.json DocesCabana.MVC/appsettings.json
```

O exemplo já sobe a aplicação em desenvolvimento (sem SMTP real). O banco é
Postgres, hospedado no Supabase (spec 028) — não há mais SQLite local nem
fallback sem configuração: a connection string real, com senha, precisa ir
para os *user secrets* do .NET:

```powershell
cd DocesCabana.MVC
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Port=5432;Database=postgres;Username=postgres.<ref>;Password=...;SSL Mode=Require"
dotnet user-secrets set "SupabaseSettings:ChaveDeServico" "sua-chave-de-servico"
dotnet user-secrets set "EmailSettings:SmtpUsername" "seu-usuario"
dotnet user-secrets set "EmailSettings:SmtpPassword" "sua-senha"
```
