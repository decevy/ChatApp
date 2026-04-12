# Agent instructions — ChatApp.Infrastructure

**ChatApp.Infrastructure** implements **persistence** for **ChatApp.Core**: **`ChatDbContext`**, **EF Core configurations**, **repository classes**, **migrations** (under this project’s **`Migrations/`** folder when present), and **database seeding**. It references **ChatApp.Core** only (plus Npgsql/EF packages per **`.csproj`**).

No **controllers**, **hubs**, or **JWT** code belongs here.

---

## Layout (current)

| Path | Role |
|------|------|
| **`Data/ChatDbContext.cs`** | **`DbSet<>`** declarations, **`OnModelCreating`** wiring, migration assembly |
| **`Data/ChatDbSeeder.cs`** | Optional **seed data** invoked when the database is initialized or migrated (follow existing entry points) |
| **`Repositories/UserRepository.cs`** | **`IUserRepository`** |
| **`Repositories/RoomRepository.cs`** | **`IRoomRepository`** (includes **`Query()`** / **`QueryRoomMembers()`** implementations) |
| **`Repositories/MessageRepository.cs`** | **`IMessageRepository`** |

Entity **configuration classes** may live alongside the context or under a `Configurations/` folder depending on how the codebase evolves—**keep fluent API aligned** with **Core** entities.

---

## Repositories

- Implement **Core** interfaces **exactly**; return **entities** or **query builders** as defined by those interfaces.
- **Query builders** in Core should receive **`IQueryable`** from **`DbSet`** and compose **includes/filters**; Infrastructure implementations return **`AsNoTracking()`** where appropriate if read-only queries are standard (match existing repositories).
- **Async** methods should use **`SaveChangesAsync`**, **`ToListAsync`**, **`FirstOrDefaultAsync`**, etc.

When adding a **new repository method**, update **`ChatApp.Core`** interface first, then implement here, then consume from **Services** or **Api**.

---

## Migrations (EF Core)

Always specify **infrastructure** project and **API** startup project:

```bash
dotnet ef migrations add <MigrationName> --project ChatApp.Infrastructure --startup-project ChatApp.Api
dotnet ef database update --project ChatApp.Infrastructure --startup-project ChatApp.Api
```

The **API** also runs **`Migrate()`** on startup, so pending migrations apply automatically in dev—use **`database update`** when you want to apply schema changes **without** launching the full web host.

**Checklist for schema changes:**

1. Update **Core** entities if needed.
2. Adjust **`OnModelCreating`** / configuration classes.
3. Add migration; review generated **Up/Down**.
4. Run **`dotnet build`** and smoke-test **`dotnet run --project ChatApp.Api`**.

---

## Seeding (`ChatDbSeeder`)

- Keep seeding **idempotent** where possible (avoid duplicate key errors on re-run).
- Do not reference **Api** or **Services** types from the seeder.

---

## External dependencies (typical)

- **PostgreSQL** via **Npgsql** EF provider.
- **Redis** / **Azure Blob** may appear in packages or future work—follow existing patterns and configuration keys when wiring new features.

---

## Conventions for agents

- **No business rules** that belong in **Services** (e.g. “user must be admin”)—enforce **data integrity** and **queries** here; **authorization story** stays in **Services** / **Api**.
- Preserve **delete behaviors** (cascade vs restrict) explicitly in configuration to match product expectations.
- After changing **entities**, **always** add a **migration**; do not rely on **`EnsureCreated`** in production-style flows.
