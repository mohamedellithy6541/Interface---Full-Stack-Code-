# Interface - Employee Management & Payroll

A full-stack application for managing employees and calculating monthly payslips.

| Part | Folder | Stack | Status |
| --- | --- | --- | --- |
| Backend API | [`back/`](back/) | ASP.NET Core Web API (.NET 9), EF Core 9 + SQL Server | Done |
| Frontend | [`Front/`](Front/) | Angular 21, TypeScript, SCSS, client generated from Swagger | Next |

The assessment brief is in [`docs/Interface_Code_Assessment_2026.md`](docs/Interface_Code_Assessment_2026.md).

---

## Quick start (backend)

Prerequisites: .NET SDK 9, and SQL Server reachable at `Server=.` with Windows authentication.
(Use a different server by setting `ConnectionStrings__Default`; see [`back/.env.example`](back/.env.example).)

```powershell
cd back
dotnet run --project src/Interface.Api --launch-profile http
```

That one command builds the API, applies the EF Core migrations, seeds demo data and opens Swagger at
http://localhost:5019/swagger.

| Demo account | Password | Role |
| --- | --- | --- |
| `hr@interface.local` | `Demo@12345` | HR: everything |
| `sara.ali@interface.local` | `Demo@12345` | Employee: only her own payslip |

These are demo-only credentials, set in `appsettings.Development.json`. Seeding is off in every other environment.

Run the unit tests:

```powershell
cd back
dotnet test
```

### Optional settings (user-secrets or environment variables, never committed)

```powershell
cd back/src/Interface.Api
dotnet user-secrets set "Jwt:SigningKey" "<at least 32 random characters>"
dotnet user-secrets set "Authentication:Google:ClientId" "<your OAuth web client id>.apps.googleusercontent.com"
dotnet user-secrets set "Seed:GoogleHrEmails:0" "you@gmail.com"
```

- Without `Jwt:SigningKey`, Development uses a random key per run, so tokens stop working after a restart.
  Outside Development the API refuses to start without one.
- Google sign-in: create an **OAuth 2.0 Client ID (Web application)** in Google Cloud Console, add
  `http://localhost:4200` as an authorised JavaScript origin, and set the client ID above.
  Only Google accounts whose email already exists as a user can sign in; `Seed:GoogleHrEmails` creates HR users for them.

---

## API overview

| Method | Route | Who | Description |
| --- | --- | --- | --- |
| POST | `/api/auth/login` | anyone | Email + password, returns a JWT |
| POST | `/api/auth/google` | anyone | Google ID token, returns our JWT |
| GET | `/api/auth/me` | signed in | The current user |
| GET | `/api/employees?search=&department=&status=&page=1&pageSize=10` | HR | Paged list. `search` matches name or email |
| GET | `/api/employees/departments` | HR | Distinct departments, for the filter |
| GET | `/api/employees/{id}` | HR | One employee |
| POST | `/api/employees` | HR | Create (201 + Location header) |
| PUT | `/api/employees/{id}` | HR | Update |
| DELETE | `/api/employees/{id}` | HR | Delete (204) |
| GET | `/api/employees/{id}/payslip?month=2026-03` | HR, or the employee themself | Monthly payslip |
| GET | `/health` | anyone | Health check, including the database |

### Errors: RFC 7807 Problem Details

Every error has the same shape, written in one place: [`GlobalExceptionHandlingMiddleware`](back/src/Interface.Api/Middleware/GlobalExceptionHandlingMiddleware.cs).
Handlers and controllers contain no `try/catch`.

| Status | When | Source |
| --- | --- | --- |
| 400 | Validation failed; `errors` maps each camelCase field to its messages | FluentValidation `ValidationException` |
| 401 | Not signed in, bad credentials, bad Google token | JWT middleware / `AuthenticationFailedException` |
| 403 | Signed in but not allowed (e.g. an employee asking for another payslip) | Policy / `ForbiddenAccessException` |
| 404 | Employee not found | `NotFoundException` |
| 409 | Email already used | `ConflictException` (and SQL unique-index violations) |
| 422 | Valid request that breaks a business rule (inactive employee, month before hire) | `BusinessRuleViolationException` |
| 500 | Anything unexpected; details shown only in Development | fallback |

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "See the errors property for details.",
  "traceId": "00-...",
  "errors": { "email": ["Email must be a valid email address."], "allowances[0].amount": ["Amount cannot be negative."] }
}
```

Business errors also carry a machine-readable `code`, e.g. `payroll.employee_inactive` or `employee.email_taken`.

---

## Architecture

Clean Architecture: dependencies point inwards only.

```
back/
├── src/
│   ├── Interface.Domain          Entities, value objects, payroll rules. No framework dependencies.
│   ├── Interface.Application     Use cases (CQRS), interfaces it needs, DTOs, validation, mapping
│   ├── Interface.Infrastructure  EF Core, repositories, JWT/Google/password services, configuration
│   └── Interface.Api             Controllers, global error middleware, auth setup, Swagger
└── tests/
    └── Interface.UnitTests       Domain and application tests; no database, no network
```

```
Api ──► Application ──► Domain
 │            ▲
 └──► Infrastructure (implements Application interfaces)
```

- **Domain** holds the business rules: `Employee`, `PayrollCalculator`, `ProgressiveTaxCalculator`,
  `SocialInsuranceCalculator`, `PayrollPolicy`, `PayrollPeriod`, `Money`. It references nothing, so the payroll
  logic is tested as plain C#.
- **Application** has one folder per feature (`Employees`, `Payroll`, `Auth`). Each use case is a MediatR
  command or query with its own handler and validator (CQRS). It declares the interfaces it needs
  (`IEmployeeRepository`, `IUnitOfWork`, `IPayrollPolicyProvider`, `IJwtTokenGenerator`, `ICurrentUser`, ...).
  Two pipeline behaviours run around every request: logging and validation.
- **Infrastructure** implements those interfaces with EF Core, `Microsoft.AspNetCore.Identity`'s password hasher,
  `System.IdentityModel.Tokens.Jwt` and `Google.Apis.Auth`.
- **Api** is thin: each action maps the HTTP request to a command or query and sends it.
- **Dependency injection**: each layer has one static registration class with one extension method:
  `ApplicationServiceRegistration.AddApplication()`, `InfrastructureServiceRegistration.AddInfrastructure()`,
  `PresentationServiceRegistration.AddPresentation()`. `Program.cs` only chains them.

### Why these choices

| Choice | Reason |
| --- | --- |
| Clean Architecture | Business rules stay independent of EF Core, ASP.NET Core and SQL Server, so they are testable in isolation and the infrastructure can be swapped |
| CQRS with MediatR | One small class per use case; cross-cutting concerns (validation, logging) live in pipeline behaviours instead of every handler. Pinned to MediatR 12.5 (Apache 2.0); later versions need a commercial licence |
| AutoMapper | Entity → DTO and HTTP request → command mapping without hand-written copy code. A test asserts every map is valid. Version 16.3: 14.x has known high-severity advisories and 17.x requires .NET 10. Takes an optional licence key |
| FluentValidation | Validation rules live next to each use case, are unit-testable and shared between create and update |
| Write side through the domain, read side via mapping | Commands go through `Employee.Create`/`UpdateDetails` (private setters protect invariants); queries map entities to DTOs |
| Repository + Unit of Work | Handlers depend on interfaces, so tests use NSubstitute fakes instead of a database |
| Allowances/deductions as JSON columns | They are always read and written with their employee; no extra tables or joins |
| Payroll policy from configuration | Tax brackets, insurance rate and cap are in `appsettings.json` (`Payroll` section), validated at startup and reloaded on change |
| JWT + Google | Our own short-lived JWT for every request. Google sign-in verifies the Google ID token on the server, then issues our JWT, so the API only ever validates one token type |
| Serilog | Structured logs (JSON outside Development) and request logging |

### Extending it

- **New deduction type** (e.g. pension): add an `IPensionCalculator` in the domain, call it from `PayrollCalculator`,
  and add its settings to `PayrollOptions`. Fixed per-employee deductions need no code: they are data.
- **New tax rules**: change the `Payroll:TaxBrackets` configuration, or register another `ITaxCalculator`.
- **New module** (leave, attendance): add a `Features/Leave` folder in Application, an entity in Domain, a
  repository in Infrastructure and a controller. Nothing existing changes.

---

## Payroll rules

```
gross            = base salary + allowances
social insurance = min(base salary x rate, cap)          cap optional
taxable income   = gross - social insurance              set SocialInsuranceReducesTaxableIncome=false to tax gross
income tax       = progressive brackets on taxable income
net              = gross - income tax - social insurance - other deductions
```

Default configuration (illustrative numbers, not any real country's rules):

| Taxable monthly income | Rate |
| --- | --- |
| 0 - 10,000 | 0% |
| 10,000.01 - 30,000 | 10% |
| 30,000.01 - 50,000 | 15% |
| above 50,000 | 20% |

Social insurance: 11% of base salary, capped at 1,500 per month. Currency: EGP.

**Rounding rule.** Money is `decimal`, never `double`: `decimal` is base-10, so amounts like 0.10 are exact.
Every payslip component (base salary, each allowance, social insurance, income tax, each deduction) is rounded to 2 decimals
**half away from zero** (`MidpointRounding.AwayFromZero`, the usual commercial rule, rather than .NET's default banker's rounding).
Totals are then summed from the rounded components, so the lines on a payslip always add up exactly to gross, total deductions and net.
The database stores money as `decimal(18,2)` and the API rejects inputs with more than 2 decimal places.

**Edge cases handled**

| Case | Behaviour |
| --- | --- |
| Inactive employee | 422 `payroll.employee_inactive` |
| Month before the hire month | 422 `payroll.period_before_hire_date` |
| Hired during the month | Paid the full month (no proration) |
| Zero allowances / zero deductions | Gross = base salary |
| Income exactly on a bracket boundary | Taxed at the lower bracket (bounds are inclusive) |
| Insurance above the cap | Cut to the cap |
| Deductions larger than gross | 422 `payroll.deductions_exceed_gross_pay` instead of a negative net |
| Invalid month (`2026-13`, `March`) | 400 validation error |
| Invalid configuration (brackets not ascending, rate > 100%) | The API refuses to start |

---

## Assumptions

- Salaries, allowances and deductions are **monthly fixed amounts** in one currency.
- A payslip is **calculated on demand** and not stored; it always uses the current configuration.
- Social insurance is a percentage of **base salary only** (not allowances), and the cap limits the contribution amount.
- Social insurance is deducted before tax (configurable).
- No proration for joining or leaving mid-month.
- Future months can be calculated (useful for forecasts).
- Employees are deleted permanently, together with their login, if they have one. Deactivating (status Inactive) is the soft alternative.
- Only users who already exist can sign in with Google. There is no self-registration.
- Emails are compared case-insensitively (stored lowercase).

## Known limitations / next steps

- Payslips are not persisted or versioned. Next: store issued payslips so later configuration changes do not alter history.
- One payroll policy for everyone. Next: policies per country or entity, with effective dates.
- No refresh tokens. The access token simply expires (60 minutes).
- HR cannot yet create login accounts for employees from the API; demo accounts come from the seed.
- Integration tests (WebApplicationFactory + Testcontainers SQL Server) and Docker / docker-compose are not done yet.
- No audit log of who changed what (only created/updated timestamps).

## AI usage

See [`ai-usage/`](ai-usage/).
