# Cloud Development Final Project Report

**Project:** Logistics Package Tracking System

## 1) Project Idea and Objectives

This project addresses a real logistics need: managing and tracking shipments in one system.  
The goal is to provide:

- a user-facing dashboard for package operations,
- a documented REST API for integration/testing,
- persistent storage for package lifecycle data,
- cloud deployment accessible to the public.

The solution supports package creation, tracking by number, status/location updates, and record cleanup (CRUD).

## 2) System Architecture and Services Used

### Application Architecture

The application uses a 3-container architecture:

- **Frontend container (Nginx)**  
  Hosts static web UI and proxies `/api/*` requests to backend.
- **Backend container (ASP.NET Core API)**  
  Exposes REST endpoints and business rules.
- **Database container (Azure SQL Edge)**  
  Stores package data persistently using relational schema managed by EF Core migrations.

### AWS Services Used

- **Amazon EC2** (Ubuntu host for Docker runtime)
- **AWS Security Groups** (network firewall rules)
- **Public IPv4 endpoint** (public browser/API access)

## 3) Deployment Steps (AWS EC2)

1. Provision Ubuntu EC2 instance.
2. Configure Security Group inbound rules:
   - `22` (SSH)
   - `8080` (frontend + proxied API)
   - `5001` (optional direct API/Swagger)
3. Install Docker and Docker Compose plugin.
4. Clone repository on EC2:
   - `git clone https://github.com/Ezzo425/PackageTrackingAPI.git`
5. Start stack:
   - `docker compose up -d --build`
6. Validate health:
   - `docker compose ps`
   - `curl -i http://localhost:8080/api/packages`

## 4) Functional Coverage

Implemented features:

- **Create:** `POST /api/packages`
- **Read:** `GET /api/packages`, `GET /api/packages/{id}`, `GET /api/packages/tracking/{trackingNumber}`
- **Update:** `PUT /api/packages/{id}`
- **Delete:** `DELETE /api/packages/{id}`

User-facing access:

- Web dashboard on port `8080`
- Swagger documentation on port `5001`
- Postman-compatible REST endpoints

## 5) Reliability Strategy (Implemented)

Reliability controls included in the project:

- DB healthcheck in Compose (`sqlcmd SELECT 1`)
- API startup gated on healthy DB (`depends_on.condition: service_healthy`)
- Restart policy for all services (`restart: always`)
- EF Core transient SQL retry policy (`EnableRetryOnFailure`)
- Startup migration retry loop in API (`ApplyMigrations` retries)
- Centralized error middleware for stable error responses
- Serilog logging to console and file

Impact:

- Prevents startup race conditions between API and DB
- Reduces crash loops and recovers from temporary failures/reboots

## 6) Scalability Strategy (Current + Roadmap)

Current scalability foundations:

- Stateless API service design
- Service separation (frontend/api/db)
- Basic container resource protection (`memory: 512M` for API)

Planned scaling path:

1. Move workloads to ECS/Fargate
2. Add Application Load Balancer
3. Scale API replicas horizontally
4. Enable autoscaling policies
5. Move DB to Amazon RDS for managed scale and availability

## 7) Monitoring, CI, and Operational Practices

- **CI pipeline:** GitHub Actions (`.github/workflows/ci.yml`)
  - restores/builds .NET app
  - validates Docker Compose build on push/PR
- **Runtime diagnostics:** Docker logs + Serilog
- **Operational checks:** `docker compose ps`, health status, curl probes

## 8) Challenges and Resolutions

- **Issue:** SQL Server container required more memory than small EC2 instance.
- **Resolution:** Switched to Azure SQL Edge (lighter SQL Server-compatible image), added healthchecks, restart policies, and startup gating.
- **Result:** Stable startup sequence and successful public access through frontend/API.

## 9) Conclusion

The project meets the required cloud development outcomes:

- practical real-world functionality,
- full CRUD backend with persistent database,
- user-facing web interface and testable API,
- Dockerized deployment,
- public AWS hosting,
- documented reliability/scalability strategy with implemented safeguards.
