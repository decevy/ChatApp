# ChatApp - Local Development Setup

## Prerequisites
- Docker Desktop installed and running
- .NET 8 SDK installed
- Your preferred IDE (Visual Studio, Rider, or VS Code with C# extensions)

## Quick Start

### 1. Start Docker Services
```bash
# From the project root directory
docker-compose up -d

# Verify all services are running
docker-compose ps
```

You should see three services running:
- `chatapp-postgres` on port 5432
- `chatapp-redis` on port 6379
- `chatapp-pgadmin` on port 5050

### 2. Update appsettings.Development.json
Place the provided `appsettings.Development.json` in your `src/ChatApp.API/` directory.

### 3. Apply Database Migration
```bash
# Navigate to the API project
cd src/ChatApp.API

# Apply the existing migration
dotnet ef database update

# Verify migration succeeded
dotnet ef migrations list
```

### 4. Run the API
```bash
# From src/ChatApp.API
dotnet run

# Or use your IDE's run/debug functionality
```

The API should start on `https://localhost:7001` and `http://localhost:5001`

## Accessing Services

### PostgreSQL Database
- **Host**: localhost:5432
- **Database**: chatappdb_dev
- **Username**: chatapp_user
- **Password**: dev_password_123

### pgAdmin (Web UI for PostgreSQL)
- **URL**: http://localhost:5050
- **Email**: admin@chatapp.dev
- **Password**: admin123

To connect to your database in pgAdmin:
1. Right-click "Servers" → "Register" → "Server"
2. General Tab: Name = "ChatApp Dev"
3. Connection Tab:
   - Host: postgres (use container name, not localhost)
   - Port: 5432
   - Database: chatappdb_dev
   - Username: chatapp_user
   - Password: dev_password_123

### Redis
- **Host**: localhost:6379
- No authentication required for local dev

## Useful Commands

### Docker
```bash
# Stop all services
docker-compose down

# Stop and remove all data (fresh start)
docker-compose down -v

# View logs
docker-compose logs -f [service_name]

# Restart a specific service
docker-compose restart postgres
```

### Entity Framework
```bash
# Create a new migration
dotnet ef migrations add MigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove

# Update database to specific migration
dotnet ef database update MigrationName

# Generate SQL script for migration
dotnet ef migrations script
```

## Troubleshooting

### Database connection fails
- Ensure PostgreSQL container is running: `docker-compose ps`
- Check container logs: `docker-compose logs postgres`
- Verify connection string in appsettings.Development.json

### Port conflicts
If ports 5432, 6379, or 5050 are already in use, update docker-compose.yml:
```yaml
ports:
  - "5433:5432"  # Change first number only
```
Then update your connection string accordingly.

### Migration fails
- Ensure previous migration was created: Check `src/ChatApp.Infrastructure/Migrations/`
- Try removing and recreating: `dotnet ef migrations remove` then `dotnet ef migrations add Initial`

## Next Steps
Once your environment is running:
1. Test database connection by running the API
2. Use Swagger UI at `https://localhost:7001/swagger` to explore API endpoints
3. Start implementing the service layer