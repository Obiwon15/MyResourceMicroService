# Resourceedge — Organisation Management System

A .NET microservices-based organisation management platform built with ASP.NET Core. The solution covers employee management, performance appraisals, authentication/identity, email notifications, and supporting background workers — each deployed as an independently containerised service.

---

## Architecture Overview

The solution follows a microservices architecture with domain-isolated services communicating over HTTP. Each service owns its data store (MongoDB) and exposes a REST API. Two API gateway implementations are included — a custom gateway and an Ocelot-based gateway — for routing and aggregation.

```
┌─────────────────────────────────────────────────────┐
│                    API Gateways                     │
│         Resourceedge.ApiGateway  |  ApiOcelot       │
└──────────────────────┬──────────────────────────────┘
                       │
        ┌──────────────┼──────────────┐
        ▼              ▼              ▼
  ┌──────────┐  ┌────────────┐  ┌──────────┐
  │ Employee │  │ Appraisal  │  │  Auth    │
  │  Service │  │  Service   │  │ Service  │
  └──────────┘  └────────────┘  └──────────┘
        │              │              │
   MongoDB         MongoDB       SQL Server
 (EdgeEmployee) (EdgeAppraisal)  (Identity)

  ┌──────────┐  ┌─────────────┐
  │  Email   │  │   Workers   │
  │  Service │  │ (Auth/Seed) │
  └──────────┘  └─────────────┘
```

---

## Services

| Service | Project | Port (HTTP/HTTPS) | Database |
|---|---|---|---|
| **Employee API** | `Resourceedge.Employee.API` | 8001 / 8000 | MongoDB — `EdgeEmployee` |
| **Appraisal API** | `Resourceedge.Appraisal.API` | 6001 / 6000 | MongoDB — `EdgeAppraisal` |
| **Authentication API** | `Resourceedge.Authentication.API` | 7001 / 7000 | SQL Server (EF Core Identity) |
| **Email API** | `Resourceedge.Email.Api` | — | — |
| **API Gateway** | `Resourceedge.ApiGateway` | 5001 / 5000 | — |
| **Ocelot Gateway** | `Resourceedge.ApiOcelot` | 4001 / 4000 | — |
| **Operations API** | `Resourceedge.Operations.Api` | 9001 / 9000 | — |

**Background Workers**
- `Resourceedge.Worker.Auth` — handles async auth-related tasks
- `Resourceedge.Worker.Common` — shared worker utilities
- `DBInitializers` — seeds and initialises database state on startup

**Shared Library**
- `Resourceedge.Common` — shared models, helpers, and contracts used across services

---

## Tech Stack

- **Runtime:** ASP.NET Core (.NET — Visual Studio 2019 solution)
- **Language:** C# (~85%), HTML (~13%)
- **Databases:** MongoDB (Employee, Appraisal), SQL Server via Entity Framework Core (Identity/Auth)
- **Auth:** ASP.NET Core Identity + IdentityServer (PersistedGrantDbContext, ConfigurationDbContext)
- **Email:** SendGrid
- **API Gateway:** Custom gateway + Ocelot
- **Containerisation:** Docker + Docker Compose
- **Orchestration:** Docker bridge network (`resourceedge`)

---

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (compatible with the solution's target framework)
- [Docker & Docker Compose](https://docs.docker.com/get-docker/)
- MongoDB instance (local or Atlas)
- SQL Server instance (for the Authentication service)
- SendGrid API key (for email functionality)

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/Obiwon15/MyResourceMicroService.git
cd MyResourceMicroService
```

### 2. Configure environment variables

Each service expects the following environment variables. For local development, these can be set in `docker-compose.override.yml` or via user secrets.

**Appraisal Service**
```
DefaultConnection__ConnectionString=mongodb://127.0.0.1:27017/EdgeAppraisal
DefaultConnection__DataBaseName=EdgeAppraisal
SendGrid__SENDGRID_API_KEY=<your_key>
```

**Employee Service**
```
DefaultConnection__ConnectionString=mongodb://127.0.0.1:27017/EdgeEmployee
DefaultConnection__DataBaseName=EdgeEmployee
SendGrid__SENDGRID_API_KEY=<your_key>
```

**Authentication Service**
```
SendGrid__SENDGRID_API_KEY=<your_key>
```
> Connection string for the Identity database is managed via `appsettings.json` or user secrets.

### 3. Run database migrations (Authentication service)

```bash
# IdentityServer tables
dotnet ef migrations add InitialIdentityServerPersistedGrantDbMigration \
  -c PersistedGrantDbContext \
  -o Data/Migrations/IdentityServer/PersistedGrantDb

dotnet ef migrations add InitialIdentityServerConfigurationDbMigration \
  -c ConfigurationDbContext \
  -o Data/Migrations/IdentityServer/ConfigurationDb

# Application identity tables
dotnet ef migrations add -c EdgeDbContext -o Data/Migrations/IdentityDbContext

# Apply migrations
dotnet ef database update -c EdgeDbContext
dotnet ef database update -c PersistedGrantDbContext
dotnet ef database update -c ConfigurationDbContext
```

### 4. Run with Docker Compose

```bash
docker-compose up --build
```

This starts the Appraisal, Authentication, and Employee services on a shared `resourceedge` bridge network.

To run individual services without Docker, open `Resourceedge.sln` in Visual Studio and start the desired projects.

---

## Solution Structure

```
MyResourceMicroService/
├── ApiGateway/                  # Custom API gateway
├── ApiGatewayOcelot/            # Ocelot-based API gateway
├── Appraisal/
│   └── src/
│       ├── Resourceedge.Appraisal.API/
│       └── Resourceedge.Appraisal.Domain/
├── Authentication/
│   └── src/
│       ├── Resourceedge.Authentication.API/
│       └── Resourceedge.Authentication.Domain/
├── Common/
│   └── Resourceedge.Common/     # Shared library
├── Email/
│   └── src/Resourceedge.Email.Api/
├── Employee/
│   └── src/
│       ├── Resourceedge.Employee.API/
│       ├── Resourceedge.Employee.Application/
│       ├── Resourceedge.Employee.Domain/
│       └── Resourceedge.Employee.Infrastructure/
├── Operations/
│   └── src/Resourceedge.Operations.Api/
├── Workers/
│   └── src/
│       ├── Resourceedge.Worker.Auth/
│       ├── Resourceedge.Worker.Common/
│       └── DBInitializers/
├── docker-compose.yml
├── docker-compose.override.yml
└── Resourceedge.sln
```

---

## Notes

- The Operations API and both gateway implementations are scaffolded in the solution but commented out in `docker-compose.yml`. They can be enabled by uncommenting the relevant service blocks.
- MongoDB and RabbitMQ service definitions are also present in the compose file but commented out — intended to be run externally or added back as needed.
- User secrets (`${APPDATA}/Microsoft/UserSecrets`) are mounted into containers in the override file for local development.

---

## Contributing

Pull requests are welcome. For significant changes, please open an issue first to discuss the proposed change.
