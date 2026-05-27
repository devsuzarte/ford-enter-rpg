# FordEnterRPG

RPG de batalha por turnos baseado em navegador, desenvolvido com **ASP.NET Core 10 MVC + Web API**. Os jogadores criam personagens heróis pertencentes a uma das três classes — Warrior, Mage ou Archer — e os levam a batalhas contra inimigos gerados proceduralmente. O sistema inclui progressão de personagem (níveis, habilidades e atributos), sistema de vantagem triangular de classes, raridade de habilidades com efeitos especiais (Crit, Stun, Heal) e morte permanente (permadeath).

---

## Tecnologias Utilizadas

| Camada | Tecnologia |
|---|---|
| Linguagem | C# (.NET 10) |
| Framework | ASP.NET Core MVC + Web API |
| Banco de Dados | MySQL 8.x — via Pomelo.EntityFrameworkCore.MySql 9.0 |
| ORM | Entity Framework Core 9.0 |
| Autenticação | ASP.NET Cookie Authentication + JWT Bearer |
| Documentação de API | Swagger (Swashbuckle 6.5) + Scalar 2.14 |

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- MySQL 8+ rodando localmente na porta **3306**
- Banco de dados `rpg` criado (o EF aplica as tabelas automaticamente via migrations)

---

## Instruções de Execução

1. Clone o repositório:
   ```bash
   git clone <url-do-repositorio>
   cd ford-enter-rpg
   ```

2. Configure a connection string do MySQL em `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=127.0.0.1;Port=3306;Database=rpg;User=root;Password=SUA_SENHA;"
   }
   ```

3. Configure as variáveis JWT em `appsettings.json`:
   ```json
   "Jwt": {
     "Key": "<chave_secreta_minimo_32_caracteres>",
     "Issuer": "FordEnterRPG",
     "Audience": "FordEnterRPGUsers"
   }
   ```

4. Restaure os pacotes:
   ```bash
   dotnet restore
   ```

5. Inicie a aplicação:
   ```bash
   dotnet run
   ```
   Na primeira execução o EF aplica todas as migrations, faz o seed das habilidades e cria o usuário admin automaticamente.

6. Acesse no navegador:

   | Protocolo | URL |
   |---|---|
   | HTTP | http://localhost:5161 |
   | HTTPS | https://localhost:7044 |

---

## Usuário Admin Padrão (Seed)

| Campo | Valor |
|---|---|
| E-mail | admin@admin.com |
| Senha | admin |

---

## Mecânicas do Jogo

### Classes de Personagem

| Classe | HP Base | Dano Base | Perfil |
|---|---|---|---|
| Warrior | 25 | 7 | Tanque corpo-a-corpo |
| Mage | 18 | 10 | Alto dano mágico, baixa vida |
| Archer | 20 | 8 | Equilíbrio entre alcance e resistência |

### Sistema de Vantagem Triangular

```
Warrior → vence → Mage
Mage    → vence → Archer
Archer  → vence → Warrior
```

Vantagem aplica **+20% de dano**; desvantagem aplica **−20% de dano**.

### Habilidades — Raridades e Efeitos

| Raridade | Chance de Erro | Efeitos Disponíveis |
|---|---|---|
| Common | 20% | None, Heal, Stun |
| Rare | 12% | None, Heal, Stun, Crit |
| Epic | 5% | None, Stun, Crit |
| Heal (qualquer) | 0% | Restaura HP do jogador |

**Efeitos especiais:**
- **Crit** — 50% de chance de multiplicar o dano por 1.5×
- **Stun** — 30% de chance de atordoar o inimigo (perde um turno)
- **Heal** — restaura HP; nunca erra

### Recompensas Pós-Vitória

Após cada batalha vencida o jogador escolhe **uma** recompensa:
- **+2 HP Máximo**
- **+1 Dano Base**
- **Nova Habilidade** (ponderada por raridade; substitui a mais fraca se os 4 slots estiverem cheios)

### Permadeath

Quando o HP do personagem chega a zero ele é marcado como morto (`IsDead = true`) e não pode mais batalhar. O jogador deve realizar um reset para criar um novo personagem.

---

## Endpoints da API

### REST (JSON)

| Método | Rota | Descrição | Resposta |
|---|---|---|---|
| POST | `/api/User/SignUp` | Cadastra novo usuário (`form: name, email, password`) | 200 OK / 400 Bad Request |
| POST | `/api/User/SignIn` | Autentica e retorna token JWT (`form: email, password`) | 200 `{token}` / 401 Unauthorized |

> Explore e teste os endpoints interativamente em **http://localhost:5161/scalar**

### Rotas MVC (Interface Web)

| Método | Rota | Descrição |
|---|---|---|
| GET | `/` | Redireciona para `/Profile` ou `/SignIn` |
| GET / POST | `/SignIn` | Tela e processamento de login |
| GET / POST | `/SignUp` | Tela e processamento de cadastro |
| POST | `/SignOut` | Encerra sessão |
| GET | `/Profile` | Perfil do usuário autenticado |
| GET | `/Character` | Detalhes do personagem ativo |
| GET / POST | `/Character/Create` | Tela e criação de personagem |
| POST | `/Character/Reset` | Remove personagem atual |
| GET | `/Battle/Start` | Inicia nova batalha (ou redireciona para a ativa) |
| GET | `/Battle/{id}` | Estado atual da batalha |
| POST | `/Battle/{id}/Turn` | Executa turno com habilidade selecionada |
| GET / POST | `/Battle/{id}/Reward` | Tela e processamento de recompensa pós-vitória |
| GET | `/Battle/SkillSwap` | Exibe resultado de troca de habilidade |

---

## Estrutura do Projeto

```
ford-enter-rpg/
├── Controllers/
│   ├── Pages/          # Controllers Razor (Auth, Character, Battle, Profile)
│   └── UserController  # Endpoints REST de autenticação
├── Services/           # BattleService, CharacterService, UserService
├── Repositories/       # UserRepository — acesso a dados
├── Models/             # Entidades: User, Character, Skill, CharacterSkill, Battle, BattleLog
├── DTOs/               # Objetos de entrada das requisições
├── Data/               # AppDbContext — configuração do EF Core
├── Utils/              # AdminSeeder — seed de admin e habilidades
├── Views/              # Razor Views (.cshtml)
├── Migrations/         # Histórico de migrations do EF Core
└── docs/               # Documentação funcional e README em .docx
```

---

## Modelo de Dados

```
User ──< Character ──< CharacterSkill >── Skill
                  └──< Battle ──< BattleLog
```

| Entidade | Campos Principais |
|---|---|
| User | Id, Name, Email, PasswordHash, Role |
| Character | Id, UserId (FK), Name, Class, Life, Damage, Level, Wins, Losses, IsDead |
| Skill | Id, Name, ClassName, Rarity, BaseDamage, EffectType, EffectValue, Description |
| CharacterSkill | CharacterId (FK), SkillId (FK), Slot |
| Battle | Id, CharacterId (FK), EnemyName, EnemyClass, EnemyMaxLife, EnemyCurrentLife, EnemyDamage, EnemyLevel, PlayerCurrentLife, Status, RewardClaimed, TurnCount, RunSequence, CreatedAt |
| BattleLog | Id, BattleId (FK), Turn, Description, PlayerHpAfter, EnemyHpAfter |
