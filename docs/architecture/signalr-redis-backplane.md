# SignalR Redis Backplane Configuration

## Overview

The Chat module uses Redis as a **backplane** for SignalR to enable **horizontal scaling** across multiple server instances. This allows multiple UniHub API servers to share SignalR connections and messages seamlessly.

## Architecture

```
┌─────────────┐       ┌─────────────┐       ┌─────────────┐
│   Client A  │       │   Client B  │       │   Client C  │
└──────┬──────┘       └──────┬──────┘       └──────┬──────┘
       │                     │                     │
       │ WebSocket           │ WebSocket           │ WebSocket
       │                     │                     │
┌──────▼──────────────────────▼─────────────────────▼──────┐
│                    Load Balancer                          │
└──────┬──────────────────────┬─────────────────────┬──────┘
       │                      │                     │
┌──────▼──────┐       ┌───────▼─────┐       ┌──────▼──────┐
│  Server 1   │       │  Server 2   │       │  Server 3   │
│  (ChatHub)  │───────│  (ChatHub)  │───────│  (ChatHub)  │
└─────────────┘       └─────────────┘       └─────────────┘
       │                      │                     │
       └──────────────────────┼─────────────────────┘
                              │
                    ┌─────────▼─────────┐
                    │   Redis Cluster   │
                    │    (Backplane)    │
                    └───────────────────┘
```

## Configuration

### appsettings.json

```json
{
  "RedisBackplane": {
    "ConnectionString": "localhost:6379",
    "Enabled": true,
    "KeyPrefix": "signalr:",
    "ConnectTimeoutMs": 5000,
    "SyncTimeoutMs": 5000,
    "AbortOnConnectFail": false
  }
}
```

### Configuration Options

| Property             | Type   | Default    | Description                                                           |
| -------------------- | ------ | ---------- | --------------------------------------------------------------------- |
| `ConnectionString`   | string | null       | Redis connection string. Format: `host:port[,password=pwd,ssl=true]`  |
| `Enabled`            | bool   | true       | Enable/disable Redis backplane. Set to `false` for single-server mode |
| `KeyPrefix`          | string | "signalr:" | Redis key prefix for SignalR messages                                 |
| `ConnectTimeoutMs`   | int    | 5000       | Connection timeout in milliseconds                                    |
| `SyncTimeoutMs`      | int    | 5000       | Synchronous operation timeout in milliseconds                         |
| `AbortOnConnectFail` | bool   | false      | Whether to abort on connection failure                                |

## Redis Connection String Formats

### Basic (Local Development)

```
localhost:6379
```

### With Password

```
localhost:6379,password=yourpassword
```

### With SSL (Production)

```
yourserver.redis.cache.windows.net:6380,password=yourkey,ssl=true
```

### Azure Redis Cache

```
yourname.redis.cache.windows.net:6380,password=yourkey,ssl=true,abortConnect=false
```

## Deployment Scenarios

### Single Server (Development)

Disable Redis backplane for local development:

```json
{
  "RedisBackplane": {
    "Enabled": false
  }
}
```

Or remove the configuration entirely. SignalR will run in in-memory mode.

### Multiple Servers (Production)

Enable Redis backplane for load-balanced production:

```json
{
  "RedisBackplane": {
    "ConnectionString": "production-redis:6379,password=secure-password,ssl=true",
    "Enabled": true,
    "KeyPrefix": "unihub:signalr:",
    "AbortOnConnectFail": true
  }
}
```

## Testing Redis Backplane

### 1. Start Redis (Docker)

```bash
docker run -d --name redis -p 6379:6379 redis:7-alpine
```

### 2. Run Multiple API Instances

Terminal 1:

```bash
dotnet run --project src/UniHub.API --urls "https://localhost:5001"
```

Terminal 2:

```bash
dotnet run --project src/UniHub.API --urls "https://localhost:5002"
```

### 3. Connect Clients to Different Servers

- Client A connects to `https://localhost:5001/hubs/chat`
- Client B connects to `https://localhost:5002/hubs/chat`
- Both clients should receive messages from each other (via Redis)

## Redis Pub/Sub Channels

SignalR uses these Redis channels:

- `signalr:all` - Broadcast to all connections
- `signalr:group:{groupName}` - Broadcast to specific SignalR group
- `signalr:connection:{connectionId}` - Send to specific connection
- `signalr:user:{userId}` - Send to specific user (all connections)

## Performance Considerations

### Redis Configuration

For production, configure Redis for optimal performance:

```bash
# /etc/redis/redis.conf
maxmemory 256mb
maxmemory-policy allkeys-lru
tcp-backlog 511
timeout 0
tcp-keepalive 300
```

### Connection Pooling

SignalR Redis backplane uses connection pooling automatically. Typical memory usage:

- ~5MB per 1,000 connections (in-memory)
- ~50KB per message through Redis

### Monitoring

Monitor Redis performance:

```bash
# Connect to Redis CLI
redis-cli

# Monitor commands in real-time
MONITOR

# Check memory usage
INFO memory

# Check pub/sub channels
PUBSUB CHANNELS signalr:*

# Check number of subscribers
PUBSUB NUMSUB signalr:all
```

## Troubleshooting

### Redis Connection Failed

**Error**: `SocketException: No connection could be made`

**Solution**: Ensure Redis is running and accessible

```bash
docker ps | grep redis
redis-cli ping  # Should return "PONG"
```

### Messages Not Distributed

**Symptom**: Messages only received on same server

**Causes**:

1. Redis backplane disabled - check `Enabled: true` in config
2. Different KeyPrefix on different servers
3. Redis pub/sub not working - check `PUBSUB CHANNELS`

### High Memory Usage

**Symptom**: Redis memory growing continuously

**Solution**: Configure `maxmemory-policy` in Redis:

```
maxmemory 256mb
maxmemory-policy allkeys-lru
```

## Migration Path

### From In-Memory to Redis

1. Add Redis configuration to appsettings.json
2. Restart API servers (zero-downtime with rolling deployment)
3. Verify backplane: check logs for "SignalR Redis backplane enabled"

### From Redis to In-Memory

1. Set `Enabled: false` in configuration
2. Restart API servers
3. Scale down to single server instance

## Security Considerations

### Connection String Security

- **Never** commit Redis passwords to source control
- Use environment variables or Azure Key Vault:

```bash
# Environment variable
export RedisBackplane__ConnectionString="redis:6379,password=secret"
```

```json
// appsettings.json (without password)
{
  "RedisBackplane": {
    "ConnectionString": "${RedisBackplane__ConnectionString}",
    "Enabled": true
  }
}
```

### SSL/TLS

Always use SSL in production:

```json
{
  "RedisBackplane": {
    "ConnectionString": "redis.prod:6380,password=secret,ssl=true,sslHost=redis.prod"
  }
}
```

## References

- [SignalR Scale-out with Redis](https://learn.microsoft.com/en-us/aspnet/core/signalr/redis-backplane)
- [StackExchange.Redis Documentation](https://stackexchange.github.io/StackExchange.Redis/)
- [Redis Pub/Sub](https://redis.io/docs/manual/pubsub/)
