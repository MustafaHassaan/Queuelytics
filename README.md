# Queuelytics – Web Analytics Data Aggregator

**A small but challenging backend system built with .NET 8, RabbitMQ, Redis, and SQL Server, designed to read, aggregate, and report analytics data from multiple sources.**

---

## 🚀 Overview

Queuelytics was a compact task, but full of interesting challenges. I got the chance to work with tools I hadn’t touched in years — Docker, Redis, and RabbitMQ — and integrate them into a real-world backend system. The project focuses on:

- Reading analytics data from multiple sources (mocked JSON files).
- Publishing data to a **real message broker** (RabbitMQ).
- Aggregating the data with a background worker and storing it in SQL Server.
- Exposing **JWT-secured reporting APIs** for overview and per-page analytics.
- Using clean architecture principles and separation of concerns.

> ⚡ Even though the task was small, it pushed me to think about reliability, retries, Docker orchestration, and full-stack backend integration.

---

## 🏗️ Architecture

Queuelytics.sln
┣ 📁 Api/ → Web API Layer (Controllers + Swagger)
┣ 📁 ProducerService/ → Worker Service: Reads JSON → Publishes to RabbitMQ
┣ 📁 ConsumerService/ → Worker Service: Consumes from RabbitMQ → Aggregates → Saves to DB
┣ 📁 Shared/ → Common Models / DTOs
┣ 📁 Application/ → Business Logic & Handlers
┣ 📁 Domain/ → Core Entities
┗ 📁 Infrastructure/ → EF Core Repositories + RabbitMQ + Redis + DB Migrations


---

## ⚙️ Tech Stack

- **Backend:** .NET 8 + ASP.NET Core Web API
- **Database:** SQL Server (EF Core)
- **Broker:** RabbitMQ
- **Cache:** Redis
- **Auth:** JWT Bearer
- **Documentation:** Swagger / OpenAPI
- **Runtime:** Docker Compose

---

## 🛠️ Getting Started

1. **Build and start containers**
```bash
docker compose up --build
Apply database migrations

2. **Open Package Manager Console**
Select Infrastructure project
Run: Update-Database
3.Open Swagger UI
http://localhost:8080/swagger/index.html

📂 Mock Data

GA mock: { "date": "2025-10-20", "page": "/home", "users": 120, "sessions": 150, "views": 310 }

PSI mock: { "date": "2025-10-20", "page": "/home", "performanceScore": 0.9, "LCP_ms": 2100 }

Combined into a standard record and processed through the system.

🔑 Features

Real message queue for ingestion (RabbitMQ)

Background aggregation with retries and reliability

JWT-secured APIs

Full Docker Compose orchestration (API + DB + Broker + Redis)

Swagger documentation

Clean, modular code with async programming

🎯 Challenges & Learnings

Re-familiarized with Docker, Redis, and RabbitMQ.

Built a full end-to-end system including producer → broker → consumer → database → API.

Managed retries, reliability, and background processing.

Practiced JWT authentication and API documentation with Swagger.

Learned to structure a small project with Clean Architecture while keeping everything simple and maintainable.

✅ Conclusion

Queuelytics may have been a “small” task, but it was packed with real-world backend challenges. It gave me hands-on experience with orchestration, messaging, caching, and secure API design — a perfect mini-project to demonstrate solid backend skills.
