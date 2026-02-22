# Marketplace Application

A web-based marketplace application that allows users to create accounts, log in, and interact with marketplace posts. Authenticated users can create, view, edit, and delete posts, as well as leave comments on posts created by other users.

## Tech Stack

- **Frontend:** React with Vite
- **Backend:** ASP.NET Core Web API (.NET 9.0)
- **Database:** PostgreSQL 16

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running

## How to Run our Application

### 1. Clone the Repo

```bash
git clone 
cd Project-VI-Group-1
```

### 2. Start the application
```bash
docker compose up --build
```

This builds and starts all three services:

| Service    | Container Name        | URL                        |
|------------|-----------------------|----------------------------|
| Frontend   | marketplace-frontend  | http://localhost:3000       |
| Backend    | marketplace-backend   | http://localhost:5289       |
| Database   | marketplace-db        | localhost:5560              |

EF Core migrations are applied automatically on startup.

### 3. Access the application

Open http://localhost:3000 in your browser.

## Common Commands

```bash
# Start all services
docker compose up --build

# Start in the background
docker compose up --build -d

# View logs
docker compose logs -f

# View logs for a specific service
docker compose logs -f backend

# Stop all services
docker compose down

# Stop and remove database volume (full reset)
docker compose down -v
```
