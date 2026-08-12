> Projeto academico

ADOLab

Projeto de estudo sobre acesso a dados utilizando ADO.NET, com uma biblioteca de domínio, uma aplicação Console e uma aplicação Web MVC.

Desafio

Implementar o CRUD da classe AlunoRepository, permitindo:

Inserir alunos;
Listar alunos;
Atualizar alunos;
Excluir alunos;
Buscar alunos por propriedade e valor.
Estrutura
ADOLab/
├── ADOLab/              # Domínio e repository
├── ADOLab.Console/      # Aplicação Console
├── ADOLab.Web/          # Aplicação Web MVC
└── ADOLab.sln           # Solution
Tecnologias
.NET 8
ASP.NET Core MVC (.NET 10)
ADO.NET
SQL Server
Execução

Configure a connection string SqlServerConnection no appsettings.json do projeto que será executado.

Build
dotnet build ADOLab.sln
Console
dotnet run --project ADOLab.Console/ADOLab.Console.csproj
Web
dotnet run --project ADOLab.Web/ADOLab.Web.csproj
