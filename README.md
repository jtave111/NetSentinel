<div align="center">

```
███╗   ██╗███████╗████████╗███████╗███████╗███╗   ██╗████████╗██╗███╗   ██╗███████╗██╗
████╗  ██║██╔════╝╚══██╔══╝██╔════╝██╔════╝████╗  ██║╚══██╔══╝██║████╗  ██║██╔════╝██║
██╔██╗ ██║█████╗     ██║   ███████╗█████╗  ██╔██╗ ██║   ██║   ██║██╔██╗ ██║█████╗  ██║
██║╚██╗██║██╔══╝     ██║   ╚════██║██╔══╝  ██║╚██╗██║   ██║   ██║██║╚██╗██║██╔══╝  ██║
██║ ╚████║███████╗   ██║   ███████║███████╗██║ ╚████║   ██║   ██║██║ ╚████║███████╗███████╗
╚═╝  ╚═══╝╚══════╝   ╚═╝   ╚══════╝╚══════╝╚═╝  ╚═══╝   ╚═╝   ╚═╝╚═╝  ╚═══╝╚══════╝╚══════╝
```

**Security Operations Center — Patch Management & Vulnerability Intelligence**

[![.NET](https://img.shields.io/badge/.NET_9-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Next.js](https://img.shields.io/badge/Next.js_15-000000?style=flat-square&logo=nextdotjs&logoColor=white)](https://nextjs.org/)
[![React](https://img.shields.io/badge/React_19-61DAFB?style=flat-square&logo=react&logoColor=black)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?style=flat-square&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Tailwind](https://img.shields.io/badge/Tailwind_CSS-06B6D4?style=flat-square&logo=tailwindcss&logoColor=white)](https://tailwindcss.com/)
[![NVD](https://img.shields.io/badge/NVD%2FNIST-CC0000?style=flat-square&logo=gov.br&logoColor=white)](https://nvd.nist.gov/)
[![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)](LICENSE)

*Monitoramento em tempo real de vulnerabilidades em parques tecnológicos corporativos*

---

</div>

## 📌 Visão Geral

O **NetSentinel** é uma plataforma de **Patch Management** e **Governança de Segurança** para redes corporativas. Agentes instalados nos endpoints reportam automaticamente os softwares instalados para a API central, que cruza essas informações com o banco de dados público de vulnerabilidades **NVD/NIST** em tempo real, gerando alertas visuais categorizados por criticidade.

O projeto está alinhado ao **ODS 16 da ONU** — Paz, Justiça e Instituições Eficazes — com foco em transparência institucional, governança corporativa e proteção contra vazamento de dados sensíveis.

```
┌─────────────────────────────────────────────────────────────────────┐
│                                                                     │
│   Endpoint (Windows/Linux/macOS)                                    │
│        │                                                            │
│        │  POST /api/manager/device/register  (Agent → API)         │
│        ▼                                                            │
│   ┌──────────┐     ┌─────────────────┐     ┌──────────────────┐   │
│   │  C# API  │────▶│ VulnScanner     │────▶│  NVD / NIST      │   │
│   │  .NET 9  │     │ BackgroundWorker│     │  CVE Database    │   │
│   └──────────┘     └─────────────────┘     └──────────────────┘   │
│        │                                                            │
│        │  GET /api/manager/device/list  (Frontend → API)           │
│        ▼                                                            │
│   ┌──────────────────────────────────┐                             │
│   │  Next.js 15  Dashboard           │                             │
│   │  Dashboard · Endpoints · CVEs    │                             │
│   └──────────────────────────────────┘                             │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## ✨ Funcionalidades

| Módulo | Descrição |
|--------|-----------|
| 🖥️ **Dashboard SOC** | Visão consolidada do parque: KPIs, endpoints críticos, CVEs ativos e log de eventos ao vivo |
| 🔍 **Inventário de Endpoints** | Listagem completa de dispositivos com SO, usuário, status semafórico e histórico de sync |
| 🛡️ **Vulnerabilidades** | Tabela de CVEs detectados com score CVSS, severidade, aplicação afetada e hostname |
| 👤 **Gestão de Usuários** | Criação e listagem de usuários via `POST /api/auth/users` + `GET /api/user/all` |
| 🔐 **Autenticação JWT** | Login com token Bearer, refresh automático, proteção de rotas e expiração de sessão |
| 🌗 **Tema Claro / Escuro** | Alternância persistida no `localStorage`, modo escuro com preto neutro sem azul |
| ♻️ **Auto-refresh** | Dados de endpoints atualizados automaticamente a cada 30 segundos |
| 📡 **Worker NVD** | Background service que varre a NVD a cada 5 minutos e replica CVEs para todos os hosts afetados |

---

## 🏗️ Estrutura do Projeto

```
NetSentinel/
├── NetSentinel.Api/                  ← Backend C# (.NET 9)
│   ├── Controllers/
│   │   ├── AuthController.cs         ← Login, Register, Roles
│   │   ├── DeviceController.cs       ← Register endpoint, List devices
│   │   └── UserController.cs         ← GetMe, GetAllUsers
│   ├── Models/
│   │   ├── Device.cs
│   │   ├── InstalledApplication.cs
│   │   ├── SoftwareVulnerability.cs
│   │   └── User.cs
│   ├── Services/
│   │   └── NvdIntegrationService.cs  ← Consulta NVD/NIST por CVE
│   ├── Workers/
│   │   └── VulnerabilityScannerWorker.cs  ← BackgroundService (5 min)
│   ├── Filters/
│   │   └── ApiKeyAttribute.cs        ← Proteção do endpoint de agente
│   └── Data/
│       └── AppDbContext.cs
│
└── NetSentinel.Web/                  ← Frontend Next.js 15
    ├── app/
    │   ├── page.tsx                  ← Dashboard principal
    │   ├── login/page.tsx            ← Autenticação JWT
    │   ├── devices/page.tsx          ← Inventário de endpoints
    │   ├── vulnerabilities/page.tsx  ← CVEs detectados
    │   └── users/page.tsx            ← Gestão de usuários
    ├── components/
    │   ├── dashboard/                ← StatStrip, MachineTable, AlertFeed, VulnFeed
    │   ├── layout/                   ← Sidebar, Topbar
    │   └── ui/                       ← Panel, Tag, ScoreBar, Modal, Spinner
    ├── context/
    │   ├── AuthContext.tsx            ← JWT parse + proteção de rotas
    │   ├── ThemeContext.tsx           ← Light / Dark mode
    │   └── ToastContext.tsx           ← Notificações globais
    ├── hooks/
    │   ├── useDevices.ts              ← Auto-refresh 30s
    │   └── useMachineFilter.ts        ← Filtro + busca de endpoints
    ├── lib/
    │   ├── api.ts                     ← Chamadas REST para o backend
    │   └── utils.ts                   ← cn(), cvssColor(), relativeTime()
    └── types/
        └── index.ts                   ← Tipos TypeScript (Device, CVE, User…)
```

---

## 🚀 Como Rodar

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [PostgreSQL](https://www.postgresql.org/) (ou SQL Server — configurável no `appsettings.json`)

---

### 1 · Backend (API C#)

```bash
cd NetSentinel.Api

# Restaurar dependências
dotnet restore

# Configurar variáveis de ambiente (copie e edite)
cp appsettings.json appsettings.Development.json
```

Edite o `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=netsentinel;Username=postgres;Password=suasenha"
  },
  "Jwt": {
    "Key":      "sua-chave-secreta-minimo-32-caracteres",
    "Issuer":   "NetSentinel",
    "Audience": "NetSentinel"
  },
  "NvdApi": {
    "BaseUrl": "https://services.nvd.nist.gov/rest/json/cves/2.0",
    "ApiKey":  ""
  },
  "ApiKey": "sua-api-key-para-o-agente"
}
```

```bash
# Aplicar migrations
dotnet ef database update

# Rodar a API
dotnet run --urls "http://0.0.0.0:5149"
# API disponível em: http://localhost:5000
```

---

### 2 · Frontend (Next.js)

```bash
cd NetSentinel.Web

# Instalar dependências
npm install

# Configurar a URL da API
echo "NEXT_PUBLIC_API_URL=http://localhost:5000" > .env.local

# Rodar em desenvolvimento
npm run dev
# Acesse: http://localhost:3000
```

> **Primeira vez?** Crie um usuário Admin pela API antes de logar:
> ```bash
> curl -X POST http://localhost:5000/api/auth/users \
>   -H "Content-Type: application/json" \
>   -d '{"name":"Admin","username":"admin","email":"admin@corp.com","password":"Admin@123","department":"TI","roleId":1}'
> ```

---

## 🔌 Endpoints da API

### Autenticação

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| `POST` | `/api/auth/login` | Login — retorna JWT Bearer | — |
| `POST` | `/api/auth/users` | Cadastro de usuário | — |
| `POST` | `/api/auth/roles` | Criação de perfil | — |

### Dispositivos

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| `POST` | `/api/manager/device/register` | Agente registra/atualiza endpoint | `ApiKey` |
| `GET`  | `/api/manager/device/list` | Lista todos os endpoints + apps + CVEs | `JWT` |

### Usuários

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| `GET` | `/api/user/me` | Dados do usuário logado | `JWT` |
| `GET` | `/api/user/all` | Lista todos os usuários | `JWT` |

---

## 🛡️ Fluxo de Segurança

```
1. Agente coleta softwares instalados (Windows: registro; Linux: dpkg/rpm)
        │
        ▼
2. POST /api/manager/device/register  [ApiKey]
   → Upsert do device + InstalledApplications no banco
        │
        ▼
3. VulnerabilityScannerWorker (a cada 5 min)
   → Para cada app único no banco:
       → GET NVD API: ?keywordSearch={nome}+{versão}
       → Extrai CVEs, score CVSS v3.1 / v2
       → Replica para todos os hosts com aquela app/versão
        │
        ▼
4. Frontend consulta GET /api/manager/device/list  [JWT]
   → Retorna Device + InstalledApplications + Vulnerabilities
   → UI deriva status: critical (≥9.0) · warning (≥5.0) · safe
```

---

## 📊 Classificação de Criticidade

| Score CVSS | Status | Cor | Ação |
|-----------|--------|-----|------|
| ≥ 9.0 | **Crítico** | 🔴 Vermelho | Patch imediato |
| 5.0 – 8.9 | **Alerta** | 🟡 Âmbar | Planejar atualização |
| < 5.0 | **Seguro** | 🟢 Verde | Monitorar |
| Sem sync > 60min | **Offline** | ⚫ Cinza | Verificar agente |

---

## 🧰 Stack Tecnológica

**Backend**
- `ASP.NET Core 9` — API REST
- `Entity Framework Core` — ORM + Migrations
- `BCrypt.Net` — Hash de senhas
- `System.IdentityModel.Tokens.Jwt` — Geração e validação de JWT
- `IHostedService` — Worker de varredura em background

**Frontend**
- `Next.js 15` (App Router + Turbopack)
- `React 19`
- `TypeScript 5`
- `Tailwind CSS 3`
- `Lucide React` — Ícones
- `IBM Plex Sans / IBM Plex Mono` — Tipografia

**Integração**
- **NVD/NIST API v2** — Base pública de CVEs
- **JWT Bearer** — Autenticação stateless
- **ApiKey Filter** — Proteção do endpoint de agente

---

## 🤝 Contribuindo

```bash
# Fork o repositório
git clone https://github.com/jtave111/NetSentinel.git
cd NetSentinel

# Crie uma branch para sua feature
git checkout -b feat/nome-da-feature

# Commit seguindo Conventional Commits
git commit -m "feat: adiciona exportação de relatório PDF"

# Push e abra um Pull Request
git push origin feat/nome-da-feature
```

---

## 📄 Licença

Distribuído sob a licença **MIT**. Veja [`LICENSE`](LICENSE) para mais informações.

---

<div align="center">

Desenvolvido com propósito de segurança corporativa e alinhado ao **ODS 16 da ONU**

*Transparência · Governança · Proteção de Dados*

</div>