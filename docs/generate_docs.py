# /// script
# requires-python = ">=3.11"
# dependencies = ["python-docx"]
# ///
"""Script to generate FordEnterRPG documentation in .docx format."""

from docx import Document
from docx.shared import Pt, RGBColor, Cm, Inches
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml.ns import qn
from docx.oxml import OxmlElement
import copy

# ─── Palette ────────────────────────────────────────────────────────────────
DARK_BG     = RGBColor(0x36, 0x36, 0x36)   # section header bg
DARKER_BG   = RGBColor(0x2B, 0x2B, 0x2B)   # page bg (for shading reference)
WHITE       = RGBColor(0xFF, 0xFF, 0xFF)
LIGHT_GRAY  = RGBColor(0xDD, 0xDD, 0xDD)
ACCENT      = RGBColor(0x4A, 0x9E, 0xD4)   # light blue


def set_cell_bg(cell, hex_color: str):
    """Apply background shading to a table cell."""
    tc   = cell._tc
    tcPr = tc.get_or_add_tcPr()
    shd  = OxmlElement("w:shd")
    shd.set(qn("w:val"),   "clear")
    shd.set(qn("w:color"), "auto")
    shd.set(qn("w:fill"),  hex_color)
    tcPr.append(shd)


def add_section_header(doc: Document, number: int, title: str):
    """Render a dark-background section header matching the template."""
    tbl  = doc.add_table(rows=1, cols=1)
    cell = tbl.rows[0].cells[0]
    set_cell_bg(cell, "363636")
    p    = cell.paragraphs[0]
    p.paragraph_format.space_before = Pt(2)
    p.paragraph_format.space_after  = Pt(2)
    run  = p.add_run(f"{number}. {title}")
    run.bold      = True
    run.font.size = Pt(12)
    run.font.color.rgb = WHITE
    doc.add_paragraph()          # breathing room after header


def add_rf_row(doc: Document, code: str, text: str):
    """A single business-rule row: bold code + description."""
    p    = doc.add_paragraph(style="List Bullet")
    p.paragraph_format.left_indent = Cm(0.5)
    r1   = p.add_run(f"{code} – ")
    r1.bold = True
    r1.font.color.rgb = ACCENT
    p.add_run(text)


def set_doc_margins(doc: Document, top=2.0, bottom=2.0, left=2.5, right=2.5):
    for section in doc.sections:
        section.top_margin    = Cm(top)
        section.bottom_margin = Cm(bottom)
        section.left_margin   = Cm(left)
        section.right_margin  = Cm(right)


# ════════════════════════════════════════════════════════════════════════════
# 1. DOCUMENTAÇÃO FUNCIONAL
# ════════════════════════════════════════════════════════════════════════════
def build_functional_doc():
    doc = Document()
    set_doc_margins(doc)

    # ── Title ───────────────────────────────────────────────────────────────
    title = doc.add_heading("FordEnterRPG – Documentação Funcional", level=0)
    title.alignment = WD_ALIGN_PARAGRAPH.CENTER
    for run in title.runs:
        run.font.color.rgb = DARK_BG

    doc.add_paragraph()

    # ── 1. Objetivo do Sistema ──────────────────────────────────────────────
    add_section_header(doc, 1, "Objetivo do Sistema")

    p = doc.add_paragraph()
    p.add_run(
        "O FordEnterRPG é um jogo RPG por turnos baseado em navegador, desenvolvido como "
        "aplicação web com ASP.NET Core. O sistema permite que usuários cadastrados criem "
        "personagens heróis pertencentes a uma das três classes disponíveis (Warrior, Mage ou "
        "Archer) e os levem a batalhas contra inimigos gerados dinamicamente. O objetivo central "
        "é proporcionar uma experiência de progressão de personagem com sistema de habilidades, "
        "vantagens de classe e morte permanente (permadeath), incentivando o reengajamento "
        "contínuo dos jogadores."
    )

    doc.add_paragraph()

    # ── 2. Regras de Negócio ────────────────────────────────────────────────
    add_section_header(doc, 2, "Regras de Negócio")

    rules = [
        ("RF001", "Cadastro de Usuário",
         "O usuário deve fornecer nome, e-mail único e senha com no mínimo 6 caracteres. "
         "Caso o e-mail já esteja cadastrado, o registro é rejeitado com mensagem de erro."),
        ("RF002", "Unicidade de Personagem",
         "Cada conta pode ter apenas um personagem ativo por vez. Personagens mortos exigem reset "
         "explícito antes de criar um novo."),
        ("RF003", "Classes e Atributos Base",
         "Warrior (25 HP, 7 DMG) – tanque corpo-a-corpo. "
         "Mage (18 HP, 10 DMG) – alto dano mágico, baixa vida. "
         "Archer (20 HP, 8 DMG) – equilíbrio entre alcance e resistência."),
        ("RF004", "Habilidades Iniciais",
         "Ao criar o personagem, ele recebe automaticamente 3 habilidades Common e 1 Rare ou Epic "
         "da sua classe, ocupando os 4 slots disponíveis."),
        ("RF005", "Sistema de Vantagem Triangular",
         "Warrior vence Mage (+20% DMG / -20% DMG). "
         "Mage vence Archer (+20% DMG / -20% DMG). "
         "Archer vence Warrior (+20% DMG / -20% DMG). "
         "Confrontos de mesma classe não aplicam bônus."),
        ("RF006", "Chance de Erro por Raridade",
         "Common – 20% de chance de errar. "
         "Rare – 12% de chance de errar. "
         "Epic – 5% de chance de errar. "
         "Habilidades do tipo Heal – 0% de chance de errar."),
        ("RF007", "Recompensas Pós-Vitória",
         "Após vencer uma batalha, o jogador escolhe uma entre três recompensas: +2 de vida "
         "máxima, +1 de dano base ou uma nova habilidade aleatória ponderada por raridade."),
        ("RF008", "Morte Permanente (Permadeath)",
         "Quando o HP do personagem chega a zero durante uma batalha, ele é marcado como morto "
         "(IsDead = true) e não pode mais batalhar. O jogador deve realizar reset para criar "
         "um novo personagem."),
        ("RF009", "Limite de Habilidades",
         "O personagem pode ter no máximo 4 habilidades. Ao adquirir uma nova quando todos os "
         "slots estão ocupados, a habilidade de menor BaseDamage é substituída automaticamente."),
        ("RF010", "Habilidades Cross-Class",
         "O personagem pode adquirir no máximo 2 habilidades da classe que o derrota (counter-class). "
         "Habilidades de classe 'Any' estão disponíveis para qualquer personagem sem restrição."),
        ("RF011", "Seed de Administrador",
         "Na primeira inicialização do sistema, o seeder cria automaticamente o usuário "
         "admin@admin.com com role Admin, garantindo acesso inicial ao sistema."),
    ]

    for code, title_rf, desc in rules:
        p = doc.add_paragraph(style="List Bullet")
        p.paragraph_format.left_indent = Cm(0.5)
        r1 = p.add_run(f"{code} – {title_rf}: ")
        r1.bold = True
        p.add_run(desc)

    doc.add_paragraph()

    # ── 3. Funcionalidades Principais ───────────────────────────────────────
    add_section_header(doc, 3, "Funcionalidades Principais")

    features = [
        ("Autenticação de Usuário",
         "Registro via e-mail/senha, login com geração de Cookie de sessão e token JWT para "
         "acesso à API. Logout encerra a sessão e invalida o cookie."),
        ("Criação de Personagem",
         "Interface para escolha de nome e classe. Validação de classe inválida com retorno de "
         "mensagem de erro. Habilidades iniciais atribuídas automaticamente."),
        ("Sistema de Batalha por Turnos",
         "O jogador seleciona uma habilidade por turno. O inimigo responde com um ataque "
         "aleatório de seu repertório. Efeitos de stun podem impedir a ação no próximo turno."),
        ("Geração Procedural de Inimigos",
         "Inimigos têm classe, nome (prefixo + sufixo aleatório), HP e DMG escalados pelo nível "
         "do personagem. Garantia de variedade a cada batalha."),
        ("Sistema de Efeitos de Habilidade",
         "None – dano puro. Heal – cura o jogador (sem chance de erro). "
         "Stun – 30% de chance de atordoar o inimigo. "
         "Crit – 50% de chance de multiplicar o dano por 1.5x."),
        ("Progressão de Personagem",
         "A cada vitória o personagem ganha +1 de nível e o jogador escolhe uma recompensa "
         "(vida, dano ou habilidade). O nível influencia os atributos dos inimigos futuros."),
        ("Perfil e Histórico",
         "Visualização do personagem com classe, atributos, habilidades equipadas, vitórias, "
         "derrotas e status atual."),
        ("Proteção de Rotas",
         "Personagens mortos são bloqueados de iniciar novas batalhas. "
         "Batalhas ativas redirecionam automaticamente para o combate em curso."),
    ]

    for feat_title, feat_desc in features:
        p = doc.add_paragraph(style="List Bullet")
        p.paragraph_format.left_indent = Cm(0.5)
        r1 = p.add_run(f"{feat_title}: ")
        r1.bold = True
        p.add_run(feat_desc)

    doc.add_paragraph()

    # ── 4. Modelo de Dados ──────────────────────────────────────────────────
    add_section_header(doc, 4, "Modelo de Dados")

    entities = [
        ("User",          "Id, Name, Email, PasswordHash, Role"),
        ("Character",     "Id, UserId (FK), Name, Class, Life, Damage, Level, Wins, Losses, IsDead"),
        ("Skill",         "Id, Name, ClassName, Rarity, BaseDamage, EffectType, EffectValue, Description"),
        ("CharacterSkill","CharacterId (FK), SkillId (FK), Slot"),
        ("Battle",        "Id, CharacterId (FK), EnemyName, EnemyClass, EnemyMaxLife, EnemyCurrentLife, "
                          "EnemyDamage, EnemyLevel, EnemyStunned, PlayerCurrentLife, PlayerStunned, "
                          "Status, RewardClaimed, TurnCount, RunSequence, CreatedAt"),
        ("BattleLog",     "Id, BattleId (FK), Turn, Description, PlayerHpAfter, EnemyHpAfter"),
    ]

    tbl = doc.add_table(rows=1, cols=2)
    tbl.style = "Table Grid"
    hdr = tbl.rows[0].cells
    set_cell_bg(hdr[0], "363636")
    set_cell_bg(hdr[1], "363636")
    for i, txt in enumerate(["Entidade", "Campos Principais"]):
        r = hdr[i].paragraphs[0].add_run(txt)
        r.bold = True
        r.font.color.rgb = WHITE

    for ent, fields in entities:
        row = tbl.add_row().cells
        row[0].text = ent
        row[1].text = fields

    doc.save("DocumentacaoFuncional.docx")
    print("DocumentacaoFuncional.docx gerado com sucesso.")


# ════════════════════════════════════════════════════════════════════════════
# 2. README
# ════════════════════════════════════════════════════════════════════════════
def build_readme_doc():
    doc = Document()
    set_doc_margins(doc)

    # ── Title ───────────────────────────────────────────────────────────────
    title = doc.add_heading("FordEnterRPG – README", level=0)
    title.alignment = WD_ALIGN_PARAGRAPH.CENTER
    for run in title.runs:
        run.font.color.rgb = DARK_BG

    doc.add_paragraph()

    # ── 1. Descrição do Projeto ─────────────────────────────────────────────
    add_section_header(doc, 1, "Descrição do Projeto")

    doc.add_paragraph(
        "FordEnterRPG é um jogo RPG por turnos baseado em navegador, desenvolvido com "
        "ASP.NET Core MVC e Web API. Os jogadores criam personagens heróis pertencentes a uma "
        "das três classes — Warrior, Mage ou Archer — e os levam a batalhas contra inimigos "
        "gerados proceduralmente. O sistema inclui progressão de personagem (níveis, habilidades "
        "e atributos), sistema de vantagem triangular de classes, raridade de habilidades com "
        "efeitos especiais (Crit, Stun, Heal) e morte permanente (permadeath). A interface web "
        "é renderizada via Razor Views, enquanto a API REST expõe endpoints de autenticação para "
        "integrações externas."
    )

    doc.add_paragraph()

    # ── 2. Tecnologias Utilizadas ───────────────────────────────────────────
    add_section_header(doc, 2, "Tecnologias Utilizadas")

    techs = [
        ("Linguagem",           "C# (.NET 10)"),
        ("Framework",           "ASP.NET Core MVC + Web API"),
        ("Banco de Dados",      "MySQL 8.x – via Pomelo.EntityFrameworkCore.MySql 9.0"),
        ("Segurança",           "ASP.NET Cookie Authentication + JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer)"),
        ("Documentação de API", "Swagger (Swashbuckle 6.5) + Scalar 2.14"),
    ]

    for label, value in techs:
        p = doc.add_paragraph(style="List Bullet")
        r1 = p.add_run(f"{label}: ")
        r1.bold = True
        p.add_run(value)

    doc.add_paragraph()

    # ── 3. Instruções de Execução ───────────────────────────────────────────
    add_section_header(doc, 3, "Instruções de Execução")

    doc.add_paragraph("Para rodar o projeto localmente, siga os passos abaixo:")

    steps = [
        "Clone o repositório:\n    git clone <url-do-repositorio>",
        "Certifique-se de ter o .NET SDK 10 instalado (https://dotnet.microsoft.com/download).",
        "Configure a connection string do MySQL no arquivo appsettings.json:\n"
        '    "ConnectionStrings": { "DefaultConnection": "Server=localhost;Database=fordrpg;User=root;Password=<sua_senha>;" }',
        "Configure as variáveis JWT em appsettings.json:\n"
        '    "Jwt": { "Key": "<chave_secreta_min_32_chars>", "Issuer": "FordEnterRPG", "Audience": "FordEnterRPGUsers" }',
        "Execute as migrations para criar o banco de dados:\n    dotnet ef database update",
        "Inicie a aplicação:\n    dotnet run",
        "Acesse o sistema no navegador em https://localhost:<porta> (porta exibida no terminal).\n"
        "Na primeira execução, o usuário admin@admin.com é criado automaticamente.",
    ]

    for i, step in enumerate(steps, 1):
        p = doc.add_paragraph(style="List Number")
        p.add_run(step)

    doc.add_paragraph()

    # ── 4. Endpoints da API ─────────────────────────────────────────────────
    add_section_header(doc, 4, "Endpoints da API")

    doc.add_paragraph("Abaixo estão os principais endpoints REST disponíveis no sistema:")

    doc.add_paragraph()

    endpoints = [
        ("POST", "/api/User/SignUp",   "Cadastra novo usuário (form: name, email, password)",                    "200 OK / 400 Bad Request"),
        ("POST", "/api/User/SignIn",   "Autentica o usuário e retorna token JWT (form: email, password)",         "200 OK + {token} / 401 Unauthorized"),
        ("GET",  "/SignIn",            "Página de login (Razor View)",                                           "200 OK / redirect"),
        ("POST", "/SignIn",            "Processa login via cookie de sessão",                                     "redirect /Profile / 200 com erro"),
        ("GET",  "/SignUp",            "Página de cadastro (Razor View)",                                        "200 OK"),
        ("POST", "/SignUp",            "Processa cadastro via cookie de sessão",                                  "redirect /SignIn / 200 com erro"),
        ("POST", "/SignOut",           "Encerra sessão e remove cookie",                                          "redirect /SignIn"),
        ("GET",  "/Profile",           "Exibe perfil do usuário autenticado",                                     "200 OK / redirect /SignIn"),
        ("GET",  "/Character",         "Exibe detalhes do personagem ativo",                                      "200 OK / redirect /Character/Create"),
        ("GET",  "/Character/Create",  "Página de criação de personagem",                                         "200 OK / redirect /Character"),
        ("POST", "/Character/Create",  "Cria personagem (form: name, class)",                                     "redirect /Character / 200 com erro"),
        ("POST", "/Character/Reset",   "Remove personagem atual e redireciona para criação",                      "redirect /Character/Create"),
        ("GET",  "/Battle/Start",      "Inicia nova batalha ou redireciona para batalha ativa",                   "redirect /Battle/{id}"),
        ("GET",  "/Battle/{id}",       "Exibe estado atual da batalha",                                          "200 OK / redirect"),
        ("POST", "/Battle/{id}/Turn",  "Executa turno com habilidade selecionada (form: skillId)",               "redirect /Battle/{id}"),
        ("GET",  "/Battle/{id}/Reward","Exibe tela de recompensa pós-vitória",                                   "200 OK / redirect"),
        ("POST", "/Battle/{id}/Reward","Processa escolha de recompensa (form: choice = Life|Damage|Skill)",       "redirect /Battle/Start ou /Battle/SkillSwap"),
        ("GET",  "/Battle/SkillSwap",  "Exibe resultado de troca de habilidade",                                 "200 OK / redirect /Character"),
    ]

    tbl = doc.add_table(rows=1, cols=4)
    tbl.style = "Table Grid"
    headers = ["Método", "Rota", "Descrição", "Resposta"]
    for i, h in enumerate(headers):
        c = tbl.rows[0].cells[i]
        set_cell_bg(c, "363636")
        r = c.paragraphs[0].add_run(h)
        r.bold = True
        r.font.color.rgb = WHITE
        r.font.size = Pt(10)

    for method, route, desc, resp in endpoints:
        row = tbl.add_row().cells
        row[0].text = method
        row[1].text = route
        row[2].text = desc
        row[3].text = resp
        for cell in row:
            for para in cell.paragraphs:
                for run in para.runs:
                    run.font.size = Pt(9)

    doc.add_paragraph()

    # ── 5. Estrutura do Projeto ─────────────────────────────────────────────
    add_section_header(doc, 5, "Estrutura do Projeto")

    structure = [
        ("Controllers/Pages/", "Controllers Razor para rotas de interface (Auth, Character, Battle, Profile)"),
        ("Controllers/",       "UserController.cs – endpoints REST de autenticação"),
        ("Services/",          "BattleService, CharacterService, UserService – lógica de negócio"),
        ("Repositories/",      "UserRepository – acesso a dados de usuário"),
        ("Models/",            "Entidades do banco: User, Character, Skill, CharacterSkill, Battle, BattleLog"),
        ("DTOs/",              "Objetos de transferência de dados para requisições de entrada"),
        ("Data/",              "AppDbContext.cs – configuração do EF Core e mapeamentos"),
        ("Utils/",             "AdminSeeder.cs – seed de administrador e habilidades na inicialização"),
        ("Views/",             "Razor Views (.cshtml) das páginas da interface web"),
        ("Migrations/",        "Histórico de migrations do Entity Framework Core"),
    ]

    tbl2 = doc.add_table(rows=1, cols=2)
    tbl2.style = "Table Grid"
    for i, h in enumerate(["Pasta/Arquivo", "Responsabilidade"]):
        c = tbl2.rows[0].cells[i]
        set_cell_bg(c, "363636")
        r = c.paragraphs[0].add_run(h)
        r.bold = True
        r.font.color.rgb = WHITE

    for folder, resp in structure:
        row = tbl2.add_row().cells
        row[0].text = folder
        row[1].text = resp

    doc.save("README.docx")
    print("README.docx gerado com sucesso.")


if __name__ == "__main__":
    build_functional_doc()
    build_readme_doc()
    print("Documentação gerada na pasta docs/")
