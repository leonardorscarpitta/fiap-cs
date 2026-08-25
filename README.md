# Domino Ponta de Quina

## Projetos

- `DominoPontaDeQuina.Core`: regras e fluxo do jogo.
- `DominoPontaDeQuina.Domain`: entidades e enums persistentes.
- `DominoPontaDeQuina.Repository`: `DominoDbContext`, mapeamentos Fluent API e repositorios EF Core.
- `DominoPontaDeQuina.Migrations`: aplicacao console usada como startup project para migrations.
- `DominoPontaDeQuina.Tests`: testes automatizados do nucleo do jogo.

## Modelo persistente

`Usuario` representa a conta do aplicativo cliente e pode possuir varios `Jogador`, que sao perfis de jogo.
`Partida` representa uma partida armazenada para consulta de historico. `ParticipacaoPartida` liga um jogador a uma partida e registra sua posicao, pontuacao e resultado.

Esta etapa prepara a persistencia e o futuro fluxo de autenticacao. API, endpoints, autenticacao e JWT estao fora do escopo.

## Pre-requisitos

- .NET 8 SDK
- Ferramenta `dotnet-ef` 8.x (`dotnet tool install --global dotnet-ef --version 8.*`)

## Restaurar e compilar

```bash
dotnet restore
dotnet build
```

## Migrations

Os comandos devem usar `DominoPontaDeQuina.Migrations` como startup project e `DominoPontaDeQuina.Repository` como projeto do contexto:

```bash
dotnet ef migrations add Inicial \
  --project DominoPontaDeQuina.Repository \
  --startup-project DominoPontaDeQuina.Migrations

dotnet ef database update \
  --project DominoPontaDeQuina.Repository \
  --startup-project DominoPontaDeQuina.Migrations
```

O banco SQLite local `domino.db` e ignorado pelo Git.
