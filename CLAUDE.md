# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

Build and test from the repository root (`RefeicaoIscal.slnx` is picked up automatically):

```bash
dotnet build                                    # build the whole solution
dotnet test                                     # run all tests (xUnit)
dotnet test --filter "FullyQualifiedName~UsuarioTests"   # run one test class
dotnet test --filter "DisplayName~Usuario_PrimeiroAcesso" # run one test
dotnet run --project Refeicao.Api                # run the API
```

Individual projects can also be built directly, e.g. `dotnet build Refeicao.Core/Refeicao.Core.csproj`.

## Architecture

Three-project solution, target framework `net10.0`, nullable + implicit usings enabled everywhere:

- **Refeicao.Core** — class library with all domain/data-access code and abstractions: EF Core entities (`Database/Entities/`), the `DbContext` (`Database/Context/RefeicaoContext.cs`), repository implementations (`Database/Repositories/`), service implementations (`Services/` — e.g. `TokenService`, `JwtOptions`), and every interface under `Abstractions/` (`Abstractions/Repositories/`, `Abstractions/Authentication/`). **All interfaces belong here, not in Refeicao.Api** — `Refeicao.Api` only holds concrete implementations and wiring.
- **Refeicao.Api** — ASP.NET Core Web API host, references `Refeicao.Core`. `Program.cs` itself is minimal: it just calls `builder.ConfigureServices()` and `app.ConfigurePipeline()`, both extension methods in `Config/` (`BuilderConfiguration.cs` wires EF Core DI, password hashers, `ITokenService`, JWT bearer authentication, Swashbuckle, and the global exception handler; `AppConfiguration.cs` sets up the middleware pipeline — `UseExceptionHandler()` first, before Swagger/auth/endpoints — Swagger UI (dev-only), and maps endpoints). Auth-specific code that stays API-side lives under `Authentication/` (role constants, e.g. `Papeis.cs`), request/response DTOs live under `Contracts/` (`AuthContracts.cs`, `CadastroContracts.cs`, namespace `Refeicao.Api.Contracts`), and endpoint-mapping extension methods live under `Endpoints/`. `Middleware/GlobalExceptionHandler.cs` implements `IExceptionHandler`: any unhandled exception is logged server-side and turned into a generic `500` `ProblemDetails` response — no exception message or stack trace ever reaches the client, in any environment. `Program.cs` ends with `public partial class Program;` so `Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>` can target it from `Refeicao.Tests`.
- **Refeicao.Tests** — xUnit test project referencing both `Refeicao.Core` and `Refeicao.Api`. Tests mostly mirror the Core project's folder structure (`Entities/`, `Database/Context/`, `Services/`); `Endpoints/` holds `Refeicao.Api` integration tests via `WebApplicationFactory<Program>`, and `TestDoubles/` holds the in-memory fake repositories (`FakeUsuarioRepository`, `FakeFuncionarioRepository`) used to swap out the real EF Core repositories in those tests via `ConfigureServices`/`RemoveAll` — this keeps auth tests off the real MySQL database. Because `Jwt:Key` is no longer in `appsettings.json` (see below), `AuthEndpointsTests` sets the `JWT_SECRET_KEY` env var in a static constructor before the `WebApplicationFactory` boots the app — `IWebHostBuilder.ConfigureAppConfiguration` callbacks run too late for `BuilderConfiguration.ConfigureServices`, which reads `builder.Configuration` synchronously during `WebApplication.CreateBuilder`.

### Data layer

- Database is **MySQL**, configured via `MySql.EntityFrameworkCore`. `RefeicaoContext.OnConfiguring` reads the connection string only from the `REFEICAO_CONNECTION_STRING` environment variable (it is not in `appsettings.json` and there is no hardcoded fallback — the constructor throws immediately if it's unset) — used both by `Refeicao.Api`'s `AddDbContext<RefeicaoContext>()` (registered with no `UseMySQL` call, so `OnConfiguring` applies) and by tests that construct `new RefeicaoContext()` directly, so both paths require the env var to be set — `RefeicaoContextTests` sets a dummy value in a static constructor before constructing the context (the tests only build the EF model, never actually connect). See `.env.example` at the repo root; never commit a real value.
- All entity-to-table mapping (table names, column names, keys, delete behaviors) is configured explicitly in `RefeicaoContext.OnModelCreating`, split into one private `Configure<Entity>` method per entity. Table and column names are uppercase snake_case (e.g. `USUARIO`, `ID_USUARIO`) while C# properties use PascalCase — there are no data annotations on the entities themselves, so the Fluent API in `RefeicaoContext` is the single source of truth for schema mapping.
- Two entities use **composite primary keys**: `FuncionarioCardapio` (`IdFuncionario` + `IdCardapio`) and `AcompanhamentoRefeicao` (`IdAcompanhamento` + `IdRefeicao`). Their repositories key on a tuple `(int, int)` rather than a single `TKey`.
- `Usuario.IdUsuario` is a `string` primary key (not auto-increment int like the other entities); `Funcionario`/`Cardapio`/`Refeicao`/`Acompanhamento` use auto-increment `int` PKs.
- `Cardapio.TpCardapio` is a `TipoCardapio` enum (`J`/`A` for Janta/Almoço) stored as `char(1)` via `HasConversion<string>()`.
- Delete behavior is deliberate per relationship: `Usuario -> Refeicao`/`Cardapio` and `Refeicao/Cardapio -> Cardapio` are `Restrict`; `Funcionario -> FuncionarioCardapio`, `Cardapio -> FuncionarioCardapio`, `Refeicao -> AcompanhamentoRefeicao`, and `Acompanhamento -> AcompanhamentoRefeicao` are `Cascade`. Preserve this split when touching the model — it's tested explicitly in `RefeicaoContextTests`.

### Repository pattern

- No generic repository abstraction: there is no `IRepository<TEntity, TKey>` and no shared `RepositoryBase<TEntity>`. Each entity has its own interface under `Abstractions/Repositories/` (e.g. `IUsuarioRepository`, `ICardapioRepository`) declaring exactly the methods its consumers actually need — not a fixed CRUD set. `ICardapioRepository`, for instance, only has `GetByPeriodoAsync`; it has no `GetByIdAsync`/`AddAsync` because nothing calls those yet. Add a method to an interface only when something needs it.
- Each concrete repository (`Database/Repositories/`) takes `RefeicaoContext context` via a primary constructor and implements its interface by querying the context's `DbSet` directly (e.g. `context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id)`) — no `DbSet.FindAsync(id).AsTask()`, always `FirstOrDefaultAsync`/`ToListAsync`/etc. from `Microsoft.EntityFrameworkCore`. `AcompanhamentoRefeicaoRepository` and `FuncionarioCardapioRepository` key on a `(int, int)` tuple since their entities have composite primary keys.
- `AddAsync`/`UpdateAsync` only stage changes on the `DbSet` — nothing is persisted until `SaveChangesAsync` is called explicitly.
- Method names are the English CRUD verbs (`GetByIdAsync`, `AddAsync`, `UpdateAsync`, `SaveChangesAsync`) by convention, matching the interface exactly — there is no compiler-enforced generic contract keeping them in sync anymore, so a typo'd or renamed method silently just doesn't implement the interface member it was meant to (a normal C# "class doesn't implement interface" compile error, not a silent runtime bug).

### Authentication (Refeicao.Api)

- Two separate login endpoints, no shared account model: `POST /api/auth/usuario/login` and `POST /api/auth/funcionario/login`, both taking `{ "id": ..., "senha": ... }` (`Contracts/AuthContracts.cs`) and returning `{ token, expiraEm, papel }` on success or `401` otherwise. `Usuario` logs in with `IdUsuario` (its string PK doubles as the login id) via `IUsuarioRepository.GetByIdAsync`; `Funcionario` logs in with `NmUsuario` (its PK, `IdFuncionario`, is a surrogate int, not something a person types in) via `IFuncionarioRepository.GetByUsernameAsync` — an entity-specific method added to that repository per the extension pattern described above. An inactive account (`DtInativacao` set) is rejected the same as a bad password.
- Passwords are verified with ASP.NET Core Identity's `IPasswordHasher<TUser>` (`PasswordHasher<Usuario>` / `PasswordHasher<Funcionario>`, registered as singletons in `Config/BuilderConfiguration.cs`) against the `HashSenha` column.
- `TokenService` (`Refeicao.Core/Services/TokenService.cs`, implementing `ITokenService` from `Refeicao.Core/Abstractions/Authentication/`) issues the JWT: claims are `sub`/`NameIdentifier` (the login id), `Name` (display/username), and `Role` (`Papeis.Usuario` or `Papeis.Funcionario`, from `Refeicao.Api/Authentication/Papeis.cs`) — use `[Authorize(Roles = Papeis.Funcionario)]` etc. to restrict an endpoint to one account type. Signing is HMAC-SHA256 using `Jwt:Key` from configuration.
- `Jwt:Issuer`/`Audience`/`ExpiresInMinutes` live in `appsettings.json` bound to `JwtOptions` (`Refeicao.Core/Services/JwtOptions.cs`); `Config/BuilderConfiguration.cs` throws on startup if the `Jwt` section is missing. `Jwt:Key` is deliberately **not** in `appsettings.json` — it's sourced only from the `JWT_SECRET_KEY` environment variable, and startup throws a clear error if it's unset. See `.env.example` at the repo root for the required variable; never commit a real key.

### Cadastro (Refeicao.Api)

- `POST /api/cadastro/usuario` and `POST /api/cadastro/funcionario` (`Endpoints/CadastroEndpoints.cs`) create accounts. Both request bodies (`Contracts/CadastroContracts.cs`) carry a `chave` field that must match a shared secret, otherwise the endpoint returns `401` before touching the repository. `Usuario` cadastro also requires `idUsuario` (its PK, chosen by the caller); `Funcionario`'s PK is a DB-generated int, so only `nmUsuario` is supplied. Both return `409 Conflict` if the id/username is already taken and `400 BadRequest` if a required field is blank; on success both return `201 Created` (no auto-login/token — call the corresponding `/api/auth/*/login` endpoint after).
- The shared secret is `CadastroOptions.Chave` (`Refeicao.Core/Services/CadastroOptions.cs`, section `Cadastro`), sourced the same way as `Jwt:Key`: **not** in `appsettings.json`, only via the `ADMIN_KEY` environment variable — `Config/BuilderConfiguration.cs` throws on startup if it's unset. The comparison in `CadastroEndpoints` uses `CryptographicOperations.FixedTimeEquals` to avoid timing attacks. See `.env.example` for the required variable.

### CORS (Refeicao.Api)

- One named policy (`BuilderConfiguration.CorsPolicyName`), registered differently per environment in `BuilderConfiguration.ConfigureServices` based on `builder.Environment.IsDevelopment()`: in **Development** it's wide open (`AllowAnyOrigin/Header/Method`) for convenience; in every other environment (Production included) it's locked to `Cors:AllowedOrigins` from configuration — an empty array by default in `appsettings.json`, meaning **no cross-origin requests are allowed until origins are explicitly configured** (deny-by-default, not fail-open). `app.UseCors(BuilderConfiguration.CorsPolicyName)` in `AppConfiguration.ConfigurePipeline` runs before `UseAuthentication`/`UseAuthorization`, matching the order ASP.NET Core requires. Set real production origins via `Cors:AllowedOrigins` in an environment-specific appsettings file or `Cors__AllowedOrigins__0`/`__1`/... environment variables (not secret, but not committed with real values either since they're deployment-specific).

### Naming conventions

The domain is Brazilian Portuguese throughout (entity/property names, table/column names, test names), while repository interfaces and their members use English CRUD verbs (`GetByIdAsync`, `AddAsync`, etc.). Keep this split consistent: new entities/tests follow the Portuguese naming already in use; repository method names follow the existing English convention.

## Git commits

Every commit message in this repository must follow **Conventional Commits** and include a **gitmoji**, in the format:

```
<gitmoji> <type>(<optional scope>): <description>
```

- `<type>` is a Conventional Commits type: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `build`, `ci`, `perf`, `style`.
- `<gitmoji>` matches the type/intent, e.g. `✨` (feat), `🐛` (fix), `♻️` (refactor), `✅` (test), `📝` (docs), `🔧` (chore/config), `👷` (ci/build), `⚡️` (perf), `🎨` (style), `🔒️` (security).
- Example: `✨ feat(auth): add funcionario login endpoint`.

Apply this to every commit created in this repository, including single-line and multi-paragraph commit messages.
