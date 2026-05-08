# Cloud Development Final Project Report
**Project:** Logistics Package Tracking System

## 1. Project Idea and Objectives
The objective of this project is to provide a robust, cloud-native backend and visual dashboard for a logistics company. The system solves the real-world problem of package tracking by allowing administrators to create, update, and route packages, while enabling customers to track their deliveries in real-time using a unique tracking number.

## 2. Architecture & Cloud Services Used
The application utilizes a microservices-inspired architecture:
- **Amazon EC2 (t3.small):** Acts as the primary compute instance hosting the Docker engine.
- **AWS Security Groups:** Configured as a virtual firewall to allow public access on ports 8080 (Frontend) and 5001 (API), while restricting SSH (22) for administration.
- **Containers:** The stack is fully Dockerized, decoupling the Nginx Web Server, the ASP.NET Core API, and the Azure SQL Edge database.

## 3. Deployment Steps
1. Provisioned an Ubuntu EC2 instance on AWS and attached an auto-assigned Public IP.
2. Configured AWS Security Groups to expose TCP ports 8080 and 5001 to `0.0.0.0/0`.
3. Connected to the instance via SSH and installed Docker and Docker Compose plugin.
4. Cloned the repository directly to the EC2 instance environment.
5. Executed `docker compose up -d --build` to pull images, apply Entity Framework Code-First migrations, and detach the processes to run in the background.

## 4. Scalability & Reliability Strategy
- **Reliability:** The system utilizes Docker container healthchecks. The API container includes a `depends_on: condition: service_healthy` block, ensuring the API does not boot until the SQL database is fully initialized, completely eliminating startup crash loops. All containers utilize `restart: always` policies to automatically recover from unexpected host reboots.
- **Scalability:** The API is completely stateless, meaning it is designed to be horizontally scaled. While currently running as a single replica, the system can be scaled by updating the compose file to include multiple API replicas behind a load balancer. Furthermore, memory limits (`512M`) are explicitly defined in the docker-compose file to prevent resource starvation on the host node.
