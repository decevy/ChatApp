# Agent instructions — ChatApp.Services

**ChatApp.Services** holds **application service implementations** for interfaces defined in **ChatApp.Core**. It depends on:

- **ChatApp.Core** — entities, DTOs, interfaces, exceptions, extensions, query builders.
- **ChatApp.Infrastructure** — concrete **repositories** (you inject **`IUserRepository`**, **`IRoomRepository`**, **`IMessageRepository`** via their **Core** interfaces).

It does **not** reference **ChatApp.Api**. HTTP and SignalR stay in the API layer.

**Packages (see `.csproj`):** e.g. **BCrypt.Net-Next**, **Microsoft.Extensions.Configuration**, **System.IdentityModel.Tokens.Jwt** for **`AuthService`**.

---

## Implemented services (current)

| Class | Interface | Responsibility |
|-------|-----------|----------------|
| **`AuthService`** | **`IAuthService`** | Register/login, JWT access tokens, refresh token generation and validation, password hashing (**BCrypt**), user persistence via **`IUserRepository`**, **`IConfiguration`** for JWT settings. |
| **`UserService`** | **`IUserService`** | Get/update user, list/search users, **`UpdateUserPresenceAsync`** (online flag + **`LastSeen`**). |
| **`RoomService`** | **`IRoomService`** | User’s room list with last message, get/create/update/delete room, membership and admin checks, uses **`IRoomRepository`**, **`IMessageRepository`**, **`IUserRepository`** and **`RoomQueryBuilder`** / related query patterns. |

**DI registration** (scoped): all three are registered in **`ChatApp.Api/Program.cs`**:

- `IAuthService` → `AuthService`
- `IUserService` → `UserService`
- `IRoomService` → `RoomService`

---

## Not implemented here (by design, today)

- **`IMessageService`** — no `MessageService.cs`; messaging flows through **`ChatHub`** + repositories. If you add an implementation, register it in **`Program.cs`** and refactor **hub/controllers** to avoid duplication.
- **`IPresenceService`** — no dedicated class; presence updates appear in **hub connection lifecycle** and **`UserService.UpdateUserPresenceAsync`**. A future **`PresenceService`** might centralize typing/presence and optional **Redis**.

---

## Coding patterns used in this project

1. **Primary constructors** (C# 12) for concise DI:  
   `public class UserService(IUserRepository userRepository) : IUserService`
2. **Throw Core exceptions** for business-rule failures: **`NotFoundException`**, **`BadRequestException`**, **`ForbiddenException`**, etc. The API middleware turns them into JSON **error responses**.
3. **Repository query builders** — e.g. **`roomRepository.Query().WithCreator().WithMembers()...`** before **`ToListAsync`** / **`FindByIdAsync`** (extension methods from Core).
4. **DTO mapping** — prefer **`SomeDto.FromEntity(entity)`** (and **`Transform`** where already used) for consistency with **`RoomService`** / **`UserService`** / **`AuthService`**.

---

## Adding a new service

1. Add **`IYourService`** in **`ChatApp.Core/Interfaces/`** (methods take **DTOs** and primitive IDs, not **`HttpContext`**).
2. Implement **`YourService`** in this project; inject only **repositories** and **framework abstractions** (`IConfiguration`, `ILogger<T>`, etc.) as needed.
3. Register **`AddScoped<IYourService, YourService>()`** in **`ChatApp.Api/Program.cs`**.
4. Call it from **controllers** or **hubs**—keep **transport** concerns out of this assembly.

---

## Conventions for agents

- Keep methods **`async`** end-to-end when calling repositories.
- Use **`DateTime.UtcNow`** for timestamps (match existing entities/services).
- Do **not** add **EF `DbContext`** or **ASP.NET** types here.
- After behavioral changes, ensure **Api** layer (controllers/hub) still compiles and **exceptions** remain mapped correctly.
