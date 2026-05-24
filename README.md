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

[![.NET](https://img.shields.io/badge/.NET_10-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Next.js](https://img.shields.io/badge/Next.js_15-000000?style=flat-square&logo=nextdotjs&logoColor=white)](https://nextjs.org/)
[![React](https://img.shields.io/badge/React_19-61DAFB?style=flat-square&logo=react&logoColor=black)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?style=flat-square&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![MySQL](https://img.shields.io/badge/MySQL_8.4-4479A1?style=flat-square&logo=mysql&logoColor=white)](https://www.mysql.com/)
[![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat-square&logo=docker&logoColor=white)](https://www.docker.com/)
[![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)](LICENSE)

*Monitoramento em tempo real de vulnerabilidades em parques tecnológicos corporativos*

</div>

---

## Visão Geral

O **NetSentinel** é uma plataforma de **Patch Management** e **Governança de Segurança** para redes corporativas. Agentes PowerShell instalados nos endpoints Windows coletam automaticamente softwares instalados, hashes SHA-256 dos binários e identidade do usuário, reportando à API central. A API usa um modelo de linguagem local (**Ollama LLM**) para identificar CVEs nos softwares detectados, expondo tudo em um dashboard SOC em tempo real.

```
┌──────────────────────────────────────────────────────────────────────┐
│  Endpoint Windows                                                    │
│  PowerShell Agent (Task Scheduler — 1 min + eventos MSI)            │
└──────────────────────────┬───────────────────────────────────────────┘
                           │ POST /api/manager/device/register
                           │ X-Api-Key header
                           ▼
┌──────────────────────────────────────────────────────────────────────┐
│  NetSentinel API  (.NET 10 / ASP.NET Core)        porta 5149        │
│                                                                      │
│  ┌────────────────────┐     ┌────────────────────────────────────┐  │
│  │ VulnScannerWorker  │────▶│  Ollama LLM (qwen2.5-coder:3b)    │  │
│  │ a cada 5 minutos   │     │  host.docker.internal:11434        │  │
│  └────────────────────┘     └────────────────────────────────────┘  │
│  ┌────────────────────┐                                              │
│  │ DeviceStatusWorker │  verifica IsActive a cada 50 segundos       │
│  └────────────────────┘                                              │
└──────────────────────────────────┬───────────────────────────────────┘
                                   │
                    ┌──────────────┴──────────────┐
                    │                             │
                    ▼                             ▼
          ┌─────────────────┐          ┌─────────────────────┐
          │  MySQL 8.4      │          │  Next.js 15 Web UI  │
          │  porta 3307     │          │  porta 80           │
          └─────────────────┘          └─────────────────────┘
```

---

## Funcionalidades

| Módulo | Descrição |
|--------|-----------|
| **Dashboard SOC** | KPIs do parque, endpoints críticos, feed de CVEs e resumo por dispositivo |
| **Inventário de Endpoints** | Listagem com status semafórico, SO, IP, MAC, último sync e contagem de apps/vulns |
| **Vulnerabilidades** | Tabela de CVEs detectados com score CVSS, severidade, aplicação afetada e hostname — filtrável e pesquisável |
| **Gestão de Usuários** | Criação e listagem de usuários com roles |
| **Autenticação JWT** | Login com token Bearer de 3 horas, proteção de rotas, parse local do token |
| **Tema Claro / Escuro** | Alternância persistida no `localStorage` |
| **Auto-refresh** | Dados atualizados automaticamente a cada 30 segundos |
| **Agente PowerShell** | Coleta softwares, hashes SHA-256, identidade de usuário (WMI + Registry + Office) |
| **Detecção por IA local** | Ollama LLM analisa cada (nome, versão) de software e retorna CVEs estruturados |
| **Integridade de Binários** | Hash SHA-256 dos executáveis detectados para identificar adulterações |

---

## Estrutura do Projeto

```
NetSentinel/
├── docker-compose.yml               ← Orquestração (MySQL, API, Web)
├── .env                             ← Credenciais e chaves (não commitar)
│
├── NetSentinel.Api/                 ← Backend C# (.NET 10)
│   ├── Controllers/
│   │   ├── AuthController.cs        ← Login, Register User, Create Role
│   │   ├── DeviceController.cs      ← Register (ApiKey) + List (JWT)
│   │   ├── UserController.cs        ← /me, /all
│   │   └── AdminController.cs       ← Reset de dispositivos
│   ├── Models/
│   │   ├── Device.cs                ← Endpoint (hostname, IPs, IsActive, LastSync)
│   │   ├── InstalledApplication.cs  ← Software + HashApplication (SHA-256)
│   │   ├── SoftwareVulnerability.cs ← Ligação app ↔ CVE
│   │   ├── Cve.cs                   ← CVE normalizado (nome, CVSS, severidade)
│   │   ├── User.cs
│   │   └── Role.cs
│   ├── Services/
│   │   └── OllamaVulnerabilityService.cs  ← Chama Ollama LLM por app/versão
│   ├── Workers/
│   │   ├── VulnerabilityScannerWorker.cs  ← Varredura Ollama a cada 5 min
│   │   └── DevicesStatusWorker.cs         ← Atualiza IsActive a cada 50 seg
│   ├── Filters/
│   │   └── ApiKeyAttribute.cs       ← Proteção do endpoint do agente
│   ├── Migrations/                  ← 5 migrations EF Core
│   ├── Data/AppDbContext.cs
│   └── wwwroot/agents/
│       └── Agent.ps1                ← Dropper + agente PowerShell
│
└── NetSentinel.Web/                 ← Frontend Next.js 15 (App Router)
    ├── app/
    │   ├── page.tsx                 ← Dashboard principal
    │   ├── login/page.tsx           ← Autenticação JWT
    │   ├── devices/page.tsx         ← Inventário de endpoints
    │   ├── vulnerabilities/page.tsx ← CVEs detectados
    │   └── users/page.tsx           ← Gestão de usuários
    ├── components/
    │   ├── dashboard/               ← StatStrip, MachineTable, AlertFeed, VulnerabilityFeed
    │   ├── layout/                  ← Sidebar, Topbar
    │   └── ui/                      ← Panel, Tag, ScoreBar, Modal, Spinner, ErrorBanner
    ├── context/
    │   ├── AuthContext.tsx           ← JWT parse + rotas protegidas
    │   ├── ThemeContext.tsx          ← Light/Dark mode
    │   └── ToastContext.tsx          ← Notificações globais
    ├── hooks/
    │   ├── useDevices.ts             ← Auto-refresh 30s
    │   └── useMachineFilter.ts       ← Filtro e busca de endpoints
    ├── lib/
    │   ├── api.ts                    ← HTTP client com Bearer token
    │   └── utils.ts                  ← relativeTime(), cvssColor(), cn()
    └── types/index.ts                ← Device, CVE, User, Application
```

---

## Como Rodar (Docker — recomendado)

### Pré-requisitos

- [Docker](https://docs.docker.com/get-docker/) + Docker Compose
- [Ollama](https://ollama.com/) rodando na máquina host com o modelo `qwen2.5-coder:3b`

```bash
# Instalar o modelo no Ollama
ollama pull qwen2.5-coder:3b

# Configurar Ollama para escutar em todas as interfaces (necessário para o Docker acessar)
sudo mkdir -p /etc/systemd/system/ollama.service.d
sudo tee /etc/systemd/system/ollama.service.d/override.conf << 'EOF'
[Service]
Environment="OLLAMA_HOST=0.0.0.0"
EOF
sudo systemctl daemon-reload && sudo systemctl restart ollama
```

### 1. Configurar variáveis de ambiente

```bash
cp docker-compose.example.yml docker-compose.yml
```

Crie o arquivo `.env` na raiz:

```env
MYSQL_ROOT_PASSWORD=sua_senha_root
MYSQL_PASSWORD=sua_senha_api
NVD_API_KEY=sua_chave_nvd          # opcional
AGENT_API_KEY=chave_secreta_agente
JWT_SECRET=chave_jwt_minimo_32_caracteres
```

### 2. Configurar o IP da máquina host

O `NEXT_PUBLIC_API_URL` precisa ser o IP real da máquina na rede (não `localhost`), pois o browser dos outros dispositivos precisa acessar a API:

```yaml
# docker-compose.yml → netsentinel-web → build → args
NEXT_PUBLIC_API_URL: http://SEU_IP:5149
```

Descubra seu IP:
```bash
ip route get 1.1.1.1 | awk '{print $7; exit}'
```

Atualize também a lista de origins permitidos no CORS em `NetSentinel.Api/Program.cs`:
```csharp
policy.WithOrigins("http://SEU_IP", ...)
```

### 3. Subir os containers

```bash
docker compose up -d --build
```

Na primeira execução, o sistema cria automaticamente:
- Roles: **Admin** e **Operator**
- Usuário padrão: `admin` / senha `admin`

### 4. Acessar

| Serviço | URL |
|---------|-----|
| Web UI | `http://SEU_IP` |
| API | `http://SEU_IP:5149` |
| MySQL | `localhost:3307` |

### Firewall (se necessário)

```bash
sudo ufw allow 80/tcp
sudo ufw allow 5149/tcp
```

---

## Deploy do Agente nos Endpoints Windows

Execute no PowerShell do endpoint com privilégios de administrador:

```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force
IEX (New-Object Net.WebClient).DownloadString('http://SEU_IP:5149/agents/Agent.ps1')
```

O dropper irá:
1. Criar `C:\NetSentinel\agent.ps1` no disco
2. Registrar a task agendada `NetSentinelHybridAgent` (SYSTEM, oculta)
3. Disparar coleta imediata + polling a cada 1 minuto

**O que o agente coleta:**
- Hostname, IP, MAC, sistema operacional
- Usuário logado (WMI `Win32_ComputerSystem`)
- Nome completo do usuário (WMI `Win32_UserAccount`)
- E-mail corporativo (UPN via `whoami /upn` ou registro do Office 365)
- Softwares instalados (registro `HKLM:\...\Uninstall\*` — 32 e 64 bits)
- Hash SHA-256 dos executáveis detectados

---

## Endpoints da API

### Autenticação — `/api/auth`

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| `POST` | `/api/auth/login` | Login — retorna JWT Bearer (3h) | — |
| `POST` | `/api/auth/users` | Cadastro de usuário | — |
| `POST` | `/api/auth/roles` | Criação de role | JWT |

### Dispositivos — `/api/manager/device`

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| `POST` | `/register` | Agente registra/atualiza endpoint + apps | `X-Api-Key` |
| `GET`  | `/list` | Lista endpoints + apps + CVEs (para o frontend) | `JWT` |
| `GET`  | `/listAll` | Lista endpoints no formato raw | `JWT` |

### Usuários — `/api/user`

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| `GET` | `/me` | Dados do usuário autenticado | `JWT` |
| `GET` | `/all` | Todos os usuários com roles | `JWT` |

### Admin — `/api/manager/admin`

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| `DELETE` | `/reset-vms` | Remove todos os dispositivos, apps e vulnerabilidades | — |

---

## Fluxo de Dados

**Registro de dispositivo:**
```
Agent (Windows) → coleta SO + apps + hashes
    → POST /api/manager/device/register [X-Api-Key]
    → API faz upsert de Device e InstalledApplications no MySQL
```

**Detecção de vulnerabilidades:**
```
VulnerabilityScannerWorker (a cada 5 min)
    → SELECT DISTINCT Name, Version FROM tb_installed_applications
    → Para cada app: POST Ollama /api/generate
    → Ollama retorna JSON com array de CVEs
    → API cria registros em tb_cves (se não existir)
    → Cria SoftwareVulnerability linkando app ↔ CVE
```

**Monitoramento de status:**
```
DeviceStatusWorker (a cada 50 seg)
    → SELECT * FROM tb_devices
    → Se LastSync < 2 minutos atrás → IsActive = false
    → Salva alterações e loga ONLINE/OFFLINE
```

**Exibição no frontend:**
```
Browser → GET /api/manager/device/list [Bearer JWT]
    → API retorna Device{InstalledApplications{SoftwareVulnerabilities{Cve}}}
    → Frontend deriva status: offline (>60min) · critical (CVSS≥9) · warning (CVSS≥5) · safe
    → Auto-refresh a cada 30 segundos
```

---

## Classificação de Criticidade

| Score CVSS | Status | Cor | Ação |
|-----------|--------|-----|------|
| ≥ 9.0 | **Crítico** | Vermelho | Patch imediato |
| 5.0 – 8.9 | **Alerta** | Âmbar | Planejar atualização |
| < 5.0 | **Seguro** | Verde | Monitorar |
| Sem sync > 60 min | **Offline** | Cinza | Verificar agente |

---

## Banco de Dados

**MySQL 8.4** — 6 tabelas:

| Tabela | Descrição |
|--------|-----------|
| `tb_roles` | Perfis de acesso (Admin, Operator) |
| `tb_users` | Usuários da plataforma (senha BCrypt) |
| `tb_devices` | Endpoints monitorados (hostname, IPs, IsActive, LastSync) |
| `tb_installed_applications` | Softwares instalados por device (nome, versão, publisher, hash SHA-256) |
| `tb_cves` | CVEs normalizados (nome, descrição, CVSS, severidade, modo de resolução) |
| `tb_software_vulnerabilities` | Ligação N:N entre InstalledApplication e CVE |

---

## Stack Tecnológica

**Backend**
- `ASP.NET Core 10` — API REST
- `Entity Framework Core` + Pomelo — ORM MySQL
- `BCrypt.Net-Next` — Hash de senhas
- `System.IdentityModel.Tokens.Jwt` — JWT Bearer
- `IHostedService` — 2 workers em background

**Frontend**
- `Next.js 15` (App Router + Turbopack, `output: standalone`)
- `React 19` + `TypeScript 5`
- `Tailwind CSS 3`
- `Lucide React` — Ícones

**Infraestrutura**
- `Docker` + `Docker Compose` — Orquestração
- `MySQL 8.4` — Banco de dados
- `Ollama` (qwen2.5-coder:3b) — LLM local para detecção de CVEs

---

## Contribuindo

```bash
git clone https://github.com/jtave111/NetSentinel.git
cd NetSentinel
git checkout -b feat/nome-da-feature
git commit -m "feat: descrição da feature"
git push origin feat/nome-da-feature
```

---

## Licença

Distribuído sob a licença **MIT**.

---

<div align="center">

*Transparência · Governança · Proteção de Dados*

</div>
