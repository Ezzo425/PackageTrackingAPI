# Package Tracking API

A full-stack package tracking system for logistics workflows. The project solves a real-world problem: managing package records and tracking delivery lifecycle/status through a public web interface and REST API.

## Project Objectives

- Build a clear logistics use case with CRUD operations.
- Provide both user-facing UI and testable API endpoints.
- Use persistent database storage.
- Deploy publicly on AWS.
- Containerize the whole stack for repeatable environments.
- Demonstrate reliability/scalability understanding.

## Core Functionality

- Create package records with sender/receiver/location.
- Read all packages, by ID, or by tracking number.
- Update package status/location/estimated delivery.
- Delete package records.
- Track package progress from the frontend dashboard.

## Architecture Overview

Current runtime is a 3-service container stack:

- `frontend` (Nginx): serves static UI and proxies `/api/*` to backend.
- `api` (ASP.NET Core): business logic + REST API.
- `sqlserver` (Azure SQL Edge): relational persistence layer.

Request flow:

1. Browser calls `http://<host>:8080`.
2. Nginx serves UI assets and forwards `/api/*` traffic to `api:8080`.
3. API reads/writes package data in SQL Edge via EF Core.

## Tech Stack

- ASP.NET Core Web API (.NET 9)
- Entity Framework Core (Code First + migrations)
- Azure SQL Edge (SQL Server-compatible, low memory footprint)
- Nginx (reverse proxy + static host)
- Docker + Docker Compose
- Serilog (console + rolling file logs)
- GitHub Actions (CI build validation)

## Project Structure

- `PackageTracking.API/` - backend source code
  - `Controllers/` - HTTP endpoints
  - `Services/` - business logic
  - `Repositories/` - data access
  - `Models/`, `DTOs/`, `Mappings/` - domain and transfer models
  - `Middleware/` - centralized exception handling
  - `Migrations/` - EF migration history
- `Frontend/` - web UI (HTML/JS) and Nginx config
- `docker-compose.yml` - orchestration for frontend/api/db
- `PackageTracking.API/Dockerfile` - backend container image
- `Frontend/Dockerfile` - frontend container image
- `.github/workflows/ci.yml` - CI pipeline
- `Project_Report.md` - final project report

## API Endpoints (CRUD + tracking)

- `POST /api/packages` - create package
- `GET /api/packages` - list all packages
- `GET /api/packages/{id}` - get by ID
- `GET /api/packages/tracking/{trackingNumber}` - track by tracking number
- `PUT /api/packages/{id}` - update package
- `DELETE /api/packages/{id}` - delete package

Use Swagger or Postman to test these endpoints.

## Run Locally

### Prerequisites

- Docker Engine / Docker Desktop
- Docker Compose plugin

### Start

```bash
git clone https://github.com/Ezzo425/PackageTrackingAPI.git
cd PackageTrackingAPI
docker compose up --build -d
```

### Access

- Frontend: `http://localhost:8080`
- API through frontend proxy: `http://localhost:8080/api/packages`
- Swagger: `http://localhost:5001/swagger`

### Stop

```bash
docker compose down
```

## AWS Deployment (EC2)

### Services Used

- Amazon EC2 (compute host)
- AWS Security Groups (network access control)
- Public IPv4 (internet access to app)

### Deployment Steps

1. Launch Ubuntu EC2 instance.
2. Install Docker + Compose plugin.
3. Open inbound ports:
   - `22` (SSH)
   - `8080` (frontend + proxied API)
   - `5001` (optional direct Swagger/API)
4. Clone repo on EC2 and start stack:

```bash
git clone https://github.com/Ezzo425/PackageTrackingAPI.git
cd PackageTrackingAPI
docker compose up --build -d
```

5. Validate:

```bash
docker compose ps
curl -i http://localhost:8080/api/packages
```

## Live Cloud Access (AWS)

- Web app: [http://13.53.67.121:8080](http://13.53.67.121:8080)
- API (proxy path): [http://13.53.67.121:8080/api/packages](http://13.53.67.121:8080/api/packages)
- Swagger (direct API): [http://13.53.67.121:5001/swagger](http://13.53.67.121:5001/swagger)

## Reliability Measures Implemented

- Database service healthcheck (`sqlcmd SELECT 1`).
- Startup dependency gating (`api` waits for healthy DB).
- Automatic restart policy (`restart: always`) for all services.
- API startup migration retry loop in `Program.cs`.
- EF Core SQL transient retry (`EnableRetryOnFailure`).
- Centralized exception middleware for consistent API failures.
- Structured logging via Serilog.

## Scalability Measures and Path

Current:

- Clear service separation (`frontend`, `api`, `sqlserver`).
- Stateless API architecture suitable for horizontal scaling.
- Basic container memory limit for API (`512M`) to protect host.

Next production steps:

- Move API/frontend to ECS Fargate tasks.
- Put Application Load Balancer in front of API/frontend.
- Configure autoscaling based on CPU/memory.
- Migrate DB from container to Amazon RDS for durability and managed scaling.

## Error Handling and Logging

- Global exception middleware:
  - `PackageTracking.API/Middleware/ErrorHandlingMiddleware.cs`
- Structured startup/request logs:
  - `PackageTracking.API/Program.cs`
- Useful controller-level informational and warning logs:
  - `PackageTracking.API/Controllers/PackagesController.cs`

## CI/CD (Optional Bonus)

GitHub Actions workflow at `.github/workflows/ci.yml`:

- triggers on push/PR to `main`
- restores/builds .NET API
- validates Docker Compose build

## Troubleshooting

- **502 Bad Gateway on `:8080`**
  - Check `docker compose ps` and ensure `api` is running.
  - Check frontend logs for upstream connection errors.
- **API connection refused on `:5001`**
  - API container is restarting/not ready; check `docker compose logs api`.
- **DB startup failures on small EC2**
  - SQL Server 2022 image requires ~2GB RAM.
  - This project uses Azure SQL Edge for lower-memory compatibility.
- **Security Group blocks access**
  - Ensure inbound rule exists for `8080` (and `5001` if needed).

## Requirement Coverage (Checklist)

- Real-world functionality: package logistics tracking.
- User-facing interface: web frontend + REST API.
- Backend service: ASP.NET Core REST API.
- Persistent DB: Azure SQL Edge.
- CRUD: fully implemented.
- AWS deployment: EC2 + Security Groups.
- Public access: browser/API endpoints.
- Dockerization: Dockerfiles + Compose.
- Local run command: `docker compose up --build`.
- Reliability/scalability: implemented controls + documented roadmap.

## Known Limitations

- Compose `deploy.resources` limits are strongest in Swarm; non-Swarm Compose behavior can vary by environment.
- Database is still containerized in current environment; RDS is recommended for stronger production reliability.
- No authentication layer yet (optional enhancement).

