# Docker Compose - UniHub Development Environment

## 🐳 Services

| Service    | Image              | Port  | Credentials Source |
|------------|--------------------|-------|--------------------|
| PostgreSQL | `postgres:16-alpine` | 5432  | `.env.docker`      |
| MongoDB    | `mongo:7-jammy`    | 27017 | `.env.docker`      |
| Redis      | `redis:7-alpine`   | 6379  | `.env.docker`      |

## 🚀 Quick Start

### Start all services

```bash
cp .env.docker.example .env.docker
docker compose --env-file .env.docker up -d
```

### Stop all services

```bash
docker compose --env-file .env.docker down
```

### Stop and remove volumes (clean slate)

```bash
docker compose --env-file .env.docker down -v
```

### View logs

```bash
# All services
docker compose --env-file .env.docker logs -f --tail=200

# Specific service
docker compose --env-file .env.docker logs -f --tail=200 postgres
docker compose --env-file .env.docker logs -f --tail=200 mongodb
docker compose --env-file .env.docker logs -f --tail=200 redis
```

### Check service health

```bash
docker compose --env-file .env.docker ps
```

### Fast debug workflow

```bash
# Start only one service if needed
docker compose --env-file .env.docker up -d redis

# Watch health + status quickly
docker compose --env-file .env.docker ps

# Restart one service while debugging
docker compose --env-file .env.docker restart postgres
```

## 🔌 Connection Strings

### PostgreSQL

```
Host: localhost
Port: 5432
Database: unihub
Username: <POSTGRES_USER>
Password: <POSTGRES_PASSWORD>

Connection String (example):
Host=localhost;Port=5432;Database=unihub;Username=<POSTGRES_USER>;Password=<POSTGRES_PASSWORD>
```

### MongoDB

```
Host: localhost
Port: 27017
Database: <MONGO_DB>
Username: <MONGO_ROOT_USER>
Password: <MONGO_ROOT_PASSWORD>

Connection String (example):
mongodb://<MONGO_ROOT_USER>:<MONGO_ROOT_PASSWORD>@localhost:27017/<MONGO_DB>?authSource=admin
```

### Redis

```
Host: localhost
Port: 6379
Password: <REDIS_PASSWORD>

Connection String (example):
localhost:6379,password=<REDIS_PASSWORD>
```

## 📦 Data Persistence

Data is persisted in Docker volumes:
- `postgres_data` - PostgreSQL database
- `mongo_data` - MongoDB database
- `mongo_config` - MongoDB configuration
- `redis_data` - Redis data

## 🔧 Environment Isolation

- DB ports are bound to `127.0.0.1` by default (`DB_BIND_HOST=127.0.0.1`)
- All credentials should be managed via `.env.docker` (ignored by git)
- Use `.env.docker.example` as the baseline template

## 🧪 Test Connection

### PostgreSQL

```bash
docker exec -it unihub-postgres psql -U <POSTGRES_USER> -d <POSTGRES_DB>
```

### MongoDB

```bash
docker exec -it unihub-mongodb mongosh -u <MONGO_ROOT_USER> -p <MONGO_ROOT_PASSWORD> --authenticationDatabase admin
```

### Redis

```bash
docker exec -it unihub-redis redis-cli -a <REDIS_PASSWORD>
```

## 🛑 Troubleshooting

### Port already in use

If ports are already in use, change host ports in `.env.docker`:

```env
POSTGRES_PORT=5433
MONGODB_PORT=27018
REDIS_PORT=6380
```

### Reset everything

```bash
docker compose --env-file .env.docker down -v
docker compose --env-file .env.docker up -d
```

### View resource usage

```bash
docker stats
```

## 📝 Notes

- All containers have health checks configured
- Containers will restart automatically unless stopped manually
- Network `unihub-network` is created for inter-container communication
- For production, use separate docker-compose files with proper secrets management
