# Package Tracking API

RESTful package tracking system built with ASP.NET Core. It solves a real logistics problem: creating packages, tracking them by ID/tracking number, and updating delivery lifecycle and location.

## Features

- Create, read, update, and delete package records (CRUD)
- Track package by tracking number
- Web frontend + backend API
- Swagger/OpenAPI for testing
- Dockerized multi-service deployment

## Tech Stack

- ASP.NET Core Web API (.NET 9)
- Entity Framework Core (Code First)
- Azure SQL Edge (low-memory SQL Server-compatible engine)
- Nginx static frontend + API reverse proxy
- Docker / Docker Compose
- Serilog logging

## Project Structure

- `PackageTracking.API/` - backend API
- `Frontend/` - static web UI served by nginx
- `docker-compose.yml` - local and single-host orchestration
- `PackageTracking.API/Dockerfile` - backend container
- `Frontend/Dockerfile` - frontend container

## Run Locally (Docker)

1. Clone the repository:

```bash
git clone https://github.com/Ezzo425/PackageTrackingAPI.git
cd PackageTrackingAPI
```

2. Build and run:

```bash
docker compose up --build -d
```

3. Access services:

- Frontend: `http://localhost:8080`
- API via frontend proxy: `http://localhost:8080/api/packages`
- API Swagger: `http://localhost:5001/swagger`

4. Stop services:

```bash
docker compose down
```

## API Endpoints

- `POST /api/packages` - create package
- `GET /api/packages` - list packages
- `GET /api/packages/{id}` - get package by ID
- `GET /api/packages/tracking/{trackingNumber}` - track package
- `PUT /api/packages/{id}` - update package
- `DELETE /api/packages/{id}` - delete package

## AWS Deployment (EC2)

This project is designed to run on AWS EC2 using Docker Compose.

1. Launch an EC2 instance (Ubuntu recommended).
2. Install Docker and Docker Compose plugin.
3. Open inbound ports in Security Group:
   - `8080` for frontend/API public access
   - `22` for SSH
4. Clone project on EC2 and run:

```bash
docker compose up --build -d
```

5. Access publicly:

- `http://<EC2_PUBLIC_IP>:8080`
- `http://<EC2_PUBLIC_IP>:8080/api/packages`

## Scalability Strategy (Basic)

Current state: containerized services with clean service separation (`frontend`, `api`, `sqlserver`) to enable horizontal/vertical growth.

Planned/ready scaling path on AWS:

- Move from single EC2 host to ECS/Fargate for API/frontend
- Add Application Load Balancer in front of frontend/API
- Run multiple API task replicas behind ALB
- Enable ECS Auto Scaling (CPU/Memory target tracking)
- Move database from container to Amazon RDS SQL Server/Aurora for managed scale and durability

This demonstrates basic scalability understanding and a practical migration path.

## Reliability Strategy

Reliability improvements already applied:

- Switched DB container to Azure SQL Edge for low-memory environments
- API has SQL retry policy (`EnableRetryOnFailure`)
- API startup retries migrations before failing
- `restart: unless-stopped` on API service
- Centralized error middleware for consistent failures
- Serilog console/file logging for diagnosis

Recommended production reliability hardening:

- Use EC2 instance with enough memory (>=2 GB for full SQL Server, lower with SQL Edge)
- Add health checks for API/DB and monitor restarts
- Use managed DB (RDS) to avoid DB container resource crashes
- Configure CloudWatch alarms for container failures and high memory usage

## Error Handling and Logging

- Global exception middleware: `PackageTracking.API/Middleware/ErrorHandlingMiddleware.cs`
- Structured logging with Serilog in `Program.cs` (console + rolling file logs)

## Testing the API

- Swagger UI: `http://localhost:5001/swagger`
- Frontend-driven API calls: `http://localhost:8080/api/packages`
- Postman collections can be used with the same endpoints.

## 🌐 Live Cloud Access (AWS)
The application is currently deployed and publicly accessible via AWS EC2:
- **Web Dashboard (Customer & Admin):** [http://13.60.190.157:8080](http://13.60.190.157:8080)
- **API Swagger Documentation:** [http://13.60.190.157:5001/swagger](http://13.60.190.157:5001/swagger)

## 🛠️ Troubleshooting & Architecture Notes
- **502 Bad Gateway / Database Connection Issues:** Standard Microsoft SQL Server containers require a minimum of 2GB of RAM, which causes silent crashes on standard free-tier/micro cloud instances. To ensure maximum reliability and lower infrastructure costs, this project utilizes **Azure SQL Edge** (`mcr.microsoft.com/azure-sql-edge`), an official, lightweight SQL engine optimized for containers with a ~500MB memory footprint.
- **Startup Order:** The `docker-compose.yml` implements dependency gating. The API will wait to boot until the SQL container passes its internal healthcheck ping.

## ⚙️ CI/CD Pipeline
This project includes automated Continuous Integration via **GitHub Actions**. Upon every push to the `main` branch, the pipeline automatically restores .NET dependencies, compiles the C# codebase in Release mode, and validates the Docker Compose build process.

