# Queuelytics – Web Analytics Data Aggregator

[![.NET](https://img.shields.io/badge/.NET-8-blue)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019-blue)](https://www.microsoft.com/en-us/sql-server)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.12-orange)](https://www.rabbitmq.com/)
[![Redis](https://img.shields.io/badge/Redis-7-red)](https://redis.io/)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

**A compact but challenging backend system built with .NET 8, RabbitMQ, Redis, and SQL Server, designed to read, aggregate, and report analytics data from multiple sources.**

---

## 🚀 Overview

Queuelytics was a small project full of real-world backend challenges. It gave me the chance to work with tools I hadn’t used in years — Docker, Redis, and RabbitMQ — and integrate them into a real backend system.  

**Key goals of the project:**

- Reading analytics data from multiple sources (mocked JSON files).  
- Publishing data to a **real message broker** (RabbitMQ).  
- Aggregating the data with a background worker and storing it in SQL Server.  
- Exposing **JWT-secured reporting APIs** for overview and per-page analytics.  
- Following clean architecture principles and separation of concerns.  

> ⚡ Even though the task was small, it involved reliability, retries, Docker orchestration, and full backend integration.

---

## ⚙️ Tech Stack

- **Backend:** .NET 8 + ASP.NET Core Web API  
- **Database:** SQL Server (EF Core)  
- **Broker:** RabbitMQ  
- **Cache:** Redis  
- **Authentication:** JWT Bearer  
- **Documentation:** Swagger / OpenAPI  
- **Runtime:** Docker Compose  

---

## 🏗️ Architecture
Queuelytics.sln
┣ 📁 Api/             → Web API Layer (Controllers + Swagger)
┣ 📁 ProducerService/ → Worker Service: Reads JSON → Publishes to RabbitMQ
┣ 📁 ConsumerService/ → Worker Service: Consumes from RabbitMQ → Aggregates → Saves to DB
┣ 📁 Shared/          → Common Models / DTOs
┣ 📁 Application/     → Business Logic & Handlers
┣ 📁 Domain/          → Core Entities
┗ 📁 Infrastructure/  → EF Core Repositories + RabbitMQ + Redis + DB Migrations

---

## 🛠️ Getting Started

### 1. Build and start Docker containers
```bash
docker compose up --build

