# tcc-doces-cabana
Aplicação com .NET 10 que visa criar um sistema de e-commerce para a loja de doces de Barra Bonita chamada "Doces Cabana".

## Configuração local

`DocesCabana.MVC/appsettings.json` não é versionado (contém, ou pode vir a
conter, credenciais de SMTP). Ao clonar o repositório:

```powershell
Copy-Item DocesCabana.MVC/appsettings.Example.json DocesCabana.MVC/appsettings.json
```

O exemplo já sobe a aplicação em desenvolvimento (SQLite local, sem SMTP real).
Para credenciais de verdade, use os *user secrets* do .NET:

```powershell
cd DocesCabana.MVC
dotnet user-secrets set "EmailSettings:SmtpUsername" "seu-usuario"
dotnet user-secrets set "EmailSettings:SmtpPassword" "sua-senha"
```
