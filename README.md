# Loan Management API

A full-stack loan management application built for the bachelor's degree project at Caucasus University. The system handles bank loan applications with two roles, User and Accountant, and consists of an ASP.NET Core 8 Web API backend, a React frontend, and a SQL Server database.

Author: Luka Vashakidze
Supervisor: Artur Zakharyan

## Features

- JWT authentication with HMAC-SHA512 signing and role claims
- Two roles: User (manages own loans) and Accountant (oversees the system)
- Time-based user blocking that expires automatically
- Loan status rules: update and delete are only allowed while a loan is `InProcess`
- FluentValidation on every request DTO
- Global exception middleware that maps custom exceptions to 400 / 403 / 404 responses
- Serilog logging to console and rolling daily files
- xUnit test suite covering service-layer rules
- React + TypeScript + Tailwind frontend with separate user and accountant flows

## Tech Stack

| Concern | Library |
| :--- | :--- |
| Web framework | ASP.NET Core 8 |
| ORM | Entity Framework Core 8 |
| Database | SQL Server (LocalDB) |
| Authentication | Microsoft.AspNetCore.Authentication.JwtBearer |
| Validation | FluentValidation.AspNetCore |
| Logging | Serilog.AspNetCore, Serilog.Sinks.File |
| API docs | Swashbuckle.AspNetCore |
| Testing | xUnit, Microsoft.EntityFrameworkCore.InMemory |
| Frontend | React 19, TypeScript, Vite, Tailwind, React Router, Axios |

## Architecture

The backend uses a layered architecture.

| Layer | Responsibility |
| :--- | :--- |
| Controllers | HTTP request handling, claim extraction, delegation to services |
| Services | Business logic (blocking, status rules, ownership checks) |
| Validators | FluentValidation rules for inbound DTOs |
| Middleware | Global exception to HTTP status mapping |
| Data | EF Core `DbContext`, migrations, seeding |
| Models | `User`, `Loan` entities |

Services are accessed through interfaces (`ILoanService`, `IAuthService`, `IAccountantService`) and injected by the DI container. Custom exceptions (`NotFoundException`, `BadRequestException`, `ForbiddenException`) are thrown from services and translated to HTTP status codes by `ExceptionMiddleware`. The API does not expose stack traces.

## Local Setup

### Prerequisites

- .NET 8.0 SDK
- SQL Server LocalDB (ships with Visual Studio)
- Node.js 20+

### Backend

```powershell
dotnet build
dotnet run --project project --launch-profile http
```

The API listens on `http://localhost:5141`. The database is created and migrated on first startup, and a seed accountant is inserted.

Seeded accountant credentials:
- Username: `accountant`
- Password: `Admin123!`

Swagger UI (dev only): `http://localhost:5141/swagger`

### Frontend

```powershell
cd client
npm install
npm run dev
```

The dev server runs on `http://localhost:5173` and reads `client/.env.development` for `VITE_API_URL`.

### Tests

```powershell
dotnet test
```

## Configuration

All configuration lives in `appsettings.Development.json` and is read by the standard ASP.NET Core configuration system. The base `appsettings.json` contains no secrets.

| Setting | Purpose |
| :--- | :--- |
| `ConnectionStrings:DefaultConnection` | LocalDB connection string |
| `JwtSettings:SecretKey` | Signing key for JWT tokens |
| `JwtSettings:Issuer` / `Audience` / `ExpiryMinutes` | Token metadata |
| `Cors:AllowedOrigins` | Frontend origins allowed to call the API |

The frontend reads `VITE_API_URL` from `client/.env.development` when started with `npm run dev`. The code paths support environment-variable overrides (`ConnectionStrings__DefaultConnection`, `JwtSettings__SecretKey`, `Cors__AllowedOrigins__0`, `VITE_API_URL` in `.env.production`) so the project can be deployed to any hosting provider without code changes.

## API Endpoints

All endpoints are under `/api`.

### Authentication (`/api/Auth`)

| Method | Endpoint | Description | Authorization |
| :--- | :--- | :--- | :--- |
| POST | `/register` | Create a new user with the `User` role. | Public |
| POST | `/login` | Return a JWT. Blocked users get 403. | Public |
| GET | `/user/{id}` | Return a user profile with their loans. | User (self only) or Accountant (any) |

### Loan Management (`/api/Loan`)

| Method | Endpoint | Description | Rules |
| :--- | :--- | :--- | :--- |
| POST | `/` | Submit a new loan application. | User must not be blocked. |
| GET | `/my-loans` | List the caller's loans. Accountants see all. | Authenticated |
| GET | `/{id}` | Get a single loan. | Owner or Accountant |
| PUT | `/{id}` | Update amount, type, currency, period. | Owner, `InProcess` only |
| DELETE | `/{id}` | Delete a loan. | Owner, `InProcess` only |

### Accountant Controls (`/api/Accountant`)

Requires the `Accountant` role.

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| GET | `/all-loans` | List every loan in the system. |
| PATCH | `/loan/{loanId}/status` | Change a loan's status (`InProcess`, `Approved`, `Rejected`). |
| PATCH | `/block-user/{userId}` | Block a user for N days: `{ "days": 7 }`. |
| PATCH | `/unblock-user/{userId}` | Lift a user block immediately. |
| PUT | `/loan/{loanId}` | Override-update any loan, regardless of status. |
| DELETE | `/loan/{loanId}` | Override-delete any loan, regardless of status. |

## Authorization

| Resource | User | Accountant |
| :--- | :--- | :--- |
| Own loans | Create (if not blocked), read, update and delete while `InProcess`. | All user actions plus override update and delete regardless of status. |
| Other users' loans | None | Read, change status, update, delete. |
| Own profile | Read. | Read any user. |
| Block / unblock | None | Block a user for N days, unblock. |

### Time-Based Blocking

When `block-user/{userId}` is called with `{ "days": N }`, `IsBlocked` is set to `true` and `BlockedUntil` is set to `DateTime.UtcNow.AddDays(N)`. On the next login or loan creation, if `BlockedUntil` is in the past, both fields are cleared automatically.

## Testing

12 tests run against an in-memory EF Core database:

- `LoanServiceTests` covers creation rules (blocked user, expired block, missing user), status rule enforcement on update and delete, and ownership checks.
- `AccountantServiceTests` covers block, unblock, and the rule that Accountants cannot block other Accountants.

## Frontend Routes

| Route | Audience | Purpose |
| :--- | :--- | :--- |
| `/login` | Public | Sign in. |
| `/register` | Public | Create a User account. |
| `/loans` | User | List own loans, edit or delete `InProcess` ones. |
| `/loans/new` | User | Submit a new loan application. |
| `/loans/:id/edit` | User | Edit an `InProcess` loan. |
| `/admin` | Accountant | List every loan, change status. |
| `/admin/users/:id` | Accountant | View any user, block or unblock, see their loans. |

JWT is stored in `localStorage` and attached to every request via an Axios interceptor. A 401 response logs the user out.

## Project Structure

```
project/                  ASP.NET Core Web API
  Constants/              Role string constants
  Controllers/            AuthController, LoanController, AccountantController
  Data/                   LoanDbContext, DataSeeder
  Dtos/                   Request and response DTOs
  Enums/                  LoanType, Currency, LoanStatus
  Exceptions/             NotFoundException, BadRequestException, ForbiddenException
  Middleware/             ExceptionMiddleware
  Migrations/             EF Core migrations
  Models/                 User, Loan
  Services/               AuthService, LoanService, AccountantService and interfaces
  Validators/             FluentValidation validators
  Program.cs              Composition root

Project.Tests/            xUnit test project
  LoanServiceTests.cs
  AccountantSerivceTests.cs
  TestHelpers.cs

client/                   React + TypeScript + Vite + Tailwind frontend
  src/
    api/                  Axios client
    components/           Layout, LoanForm, StatusDot
    context/              AuthContext
    lib/                  Formatting helpers
    pages/                Login, Register, MyLoans, CreateLoan, EditLoan, AdminLoans, AdminUser
    types/                Shared API types
```
