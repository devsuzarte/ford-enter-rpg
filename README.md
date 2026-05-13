# FordEnterRPG

RPG de batalha por turnos desenvolvido com ASP.NET Core 10, Entity Framework Core e MySQL.

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- MySQL 8+ rodando localmente na porta **3306**
- Banco de dados `rpg` criado (o EF cria as tabelas automaticamente na primeira execução)

---

## Configuração do banco de dados

Edite `appsettings.json` com suas credenciais MySQL:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=127.0.0.1;Port=3306;Database=rpg;User=root;Password=SUA_SENHA;"
}
```

---

## Instalação dos pacotes

```bash
dotnet restore
```

---

## Executar o servidor

```bash
dotnet run
```

O servidor sobe em:

| Protocolo | URL |
|-----------|-----|
| HTTP | http://localhost:5161 |
| HTTPS | https://localhost:7044 |

Na primeira execução o EF aplica todas as migrations e faz o seed das habilidades e do usuário admin automaticamente.

---

## Explorar a API (Scalar)

Acesse a documentação interativa da API em:

**http://localhost:5161/scalar**

Use o Scalar para testar os endpoints de autenticação (`/api/User/SignUp` e `/api/User/SignIn`) e obter o JWT para chamadas autenticadas.

---

## Usuário admin padrão (seed)

| Campo | Valor |
|-------|-------|
| E-mail | admin@admin.com |
| Senha | admin |

---

## Principais rotas

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/SignIn` | Tela de login |
| GET | `/SignUp` | Tela de cadastro |
| GET | `/Character` | Detalhes do personagem |
| GET | `/Character/Create` | Criar personagem |
| GET | `/Battle/Start` | Iniciar batalha |
| GET | `/Battle/{id}` | Visualizar batalha |
| POST | `/Battle/{id}/Turn` | Executar turno |
| GET | `/Battle/{id}/Reward` | Escolher recompensa |
| POST | `/api/User/SignUp` | Cadastro via API |
| POST | `/api/User/SignIn` | Login via API (retorna JWT) |
