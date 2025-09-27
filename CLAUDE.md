# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Build and Run
- `dotnet build` - Build the entire solution
- `dotnet run --project ChatApp.Api` - Run the API server (will apply database migrations on startup)
- `dotnet restore` - Restore NuGet packages

### Database
- `dotnet ef migrations add <MigrationName> --project ChatApp.Infrastructure --startup-project ChatApp.Api` - Add new migration
- `dotnet ef database update --project ChatApp.Infrastructure --startup-project ChatApp.Api` - Apply migrations

### Testing
- No test projects are currently configured in this solution

## Architecture

This is a .NET 9 chat application using Clean Architecture with three main projects:

### ChatApp.Core
Domain layer containing:
- **Entities**: User, Room, Message, RoomMember, MessageReaction with proper EF Core relationships
- **DTOs**: Data transfer objects for API contracts (AuthDTOs, UserDTOs, RoomDTOs, MessageDTOs, SignalRDTOs)
- **Interfaces**: Repository and service contracts (IUserRepository, IRoomRepository, IMessageRepository, IAuthService, IUserService, IRoomService, IMessageService, IPresenceService)

### ChatApp.Infrastructure
Data access layer containing:
- **ChatDbContext**: EF Core context with PostgreSQL provider, includes comprehensive entity configurations and seeding
- **Repositories**: UserRepository, RoomRepository, MessageRepository implementing Core interfaces
- **Dependencies**: PostgreSQL (Npgsql), Redis (StackExchange.Redis), Azure Blob Storage

### ChatApp.Api
Web API layer containing:
- **ASP.NET Core Web API** with Swagger/OpenAPI
- **JWT Authentication** configured for both HTTP and SignalR
- **SignalR Hub** support (currently commented out but configured)
- **CORS** configured for React frontend (localhost:3000)
- **Dependencies**: JWT Bearer tokens, SignalR, Entity Framework tools

## Key Configurations

- **Database**: PostgreSQL with automatic migrations on startup
- **Authentication**: JWT with configurable issuer/audience/secret from appsettings
- **SignalR**: Configured for real-time chat with JWT token support via query parameters
- **Redis**: Ready for SignalR backplane (currently commented out)
- **CORS**: Allows React development server at localhost:3000

## Entity Relationships

- Users can belong to multiple Rooms via RoomMember junction table
- Rooms contain Messages from Users
- Messages can have Reactions from Users
- All entities use standard integer primary keys with proper cascade/restrict delete behaviors