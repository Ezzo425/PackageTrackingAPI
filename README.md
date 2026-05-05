# 📦 Package Tracking API

A backend-only RESTful API built with ASP.NET Core Web API that simulates a real-world logistics system for tracking package deliveries.

The system allows creating, updating, and tracking packages using a unique tracking number.

---

## 🚀 Features

- Create new packages
- Track packages using tracking number
- Update package status and location
- Delete packages
- Get all packages
- Get package by ID
- RESTful API design
- Docker support
- SQL Server database (Dockerized)

---

## 🧱 Tech Stack

- ASP.NET Core Web API (.NET 9)
- Entity Framework Core (Code First)
- SQL Server (Docker container)
- Docker & Docker Compose
- Swagger for API testing

---

## 📦 Project Structure
PackageTrackingSystem/
│
├── PackageTracking.API/
│ ├── Controllers/
│ ├── Models/
│ ├── DTOs/
│ ├── Services/
│ ├── Repositories/
│ ├── Data/
│ ├── Mappings/
│ ├── Middleware/
│
├── docker-compose.yml
├── Dockerfile
└── PackageTrackingSystem.sln

---

## 🐳 How to Run the Project (Docker)

### 1️⃣ Clone the repository

```bash
git clone https://github.com/Ezzo425/PackageTrackingAPI.git
cd PackageTrackingAPI

---
2️⃣ Run with Docker Compose

Make sure Docker Desktop is running, then:

docker compose up --build
3️⃣ Open the API

Once running, open:

http://localhost:5001/swagger

🧪 How to Test the API

You can use:

Swagger UI 
Postman

---
📌 Main Endpoints
📦 Packages
Method	Endpoint	                            Description
POST	/api/packages	                          Create a new package
GET	/api/packages                             Get all packages
GET	/api/packages/{id}	                      Get package by ID
GET	/api/packages/tracking/{trackingNumber}	  Track package
PUT	/api/packages/{id}	                      Update package
DELETE	/api/packages/{id}	                  Delete package

