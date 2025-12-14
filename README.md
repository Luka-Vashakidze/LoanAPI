# 🏦 Loan Management API (LoanAPI)

> **A Secure, Role-Based API for Managing User Loan Applications.**

[](https://dotnet.microsoft.com/download/dotnet/8.0)
[](https://jwt.io/)
[](https://www.google.com/search?q=https://en.wikipedia.org/wiki/Clean_code)

-----

## 📚 Project Overview

LoanAPI is a RESTful Web API designed to handle the full lifecycle of loan applications. The system employs **Role-Based Authorization** (RBAC) to enforce strict segregation between regular users and administrative accountants.

### Key Objectives Achieved

  * **Role-Based Security:** Strict separation of privileges via JWT tokens (`User` and `Accountant` roles).
  * **Data Integrity:** Robust validation on all incoming requests using **FluentValidation**.
  * **Maintainable Code:** Adherence to **SOLID principles** and layered architecture.
  * **Full Observability:** Comprehensive error handling and **Serilog** logging.

-----

## 🏗 Architecture and Design Principles

The application is structured to maximize maintainability, testability, and adherence to **Clean Architecture** principles.

### 1\. Layered Structure

| Layer | Responsibility | Key Technologies |
| :--- | :--- | :--- |
| **Controllers** | Handles HTTP requests, delegates tasks, returns standardized responses. | ASP.NET Core, DTOs |
| **Services** | Contains all **Business Logic** (e.g., "Cannot create loan if blocked," "Only update if InProcess"). | C\# Interfaces, SOLID |
| **Data** | Manages database interactions (Context and Migrations). | EF Core, SQL Server |
| **Models** | Defines database entities (`User`, `Loan`). | C\# POCOs |

### 2\. SOLID Principles

  * **Single Responsibility Principle (SRP):** Business logic is isolated in the **Service Layer** (e.g., `LoanService`, `AccountantService`), separate from the Controllers.
  * **Open/Closed Principle (OCP):** Interfaces (`ILoanService`, `IAccountantService`) are used, allowing us to extend functionality (add new services) without modifying existing service implementations.
  * **Dependency Inversion Principle (DIP):** Controllers depend on abstractions (Interfaces) rather than concrete implementations, achieved via **Dependency Injection**.

### 3\. Error & Exception Handling

All exceptions are caught by a global exception handling middleware. The API **never exposes internal server errors** and instead returns a normalized JSON error message with the appropriate HTTP Status Code (e.g., **400 Bad Request**, **403 Forbidden**).

-----

## 🚀 Getting Started

### Prerequisites

  * .NET 8.0 SDK
  * SQL Server (or LocalDB)
  * Visual Studio 2022

### Installation and Setup

1.  **Clone the Repository:**

    ```bash
    git clone [Your GitHub Repo URL]
    ```

2.  **Configure Database:** Update the connection string in `appsettings.json` to point to your local SQL Server instance.

3.  **Run Migrations:** Open the Package Manager Console (PMC) and execute:

    ```powershell
    Update-Database
    ```

4.  **Run the API:** Start the project using Visual Studio (F5) or via CLI:

    ```bash
    dotnet run
    ```

5.  **Access Documentation:** Navigate to the **Swagger UI** to interact with the API: `https://localhost:7173/swagger`

-----

## 💻 API Endpoints Documentation

All endpoints are normalized (e.g., `/api/user/{id}`).

### 1\. Authentication & Users (`/api/Auth`)

| Method | Endpoint | Description | Security |
| :--- | :--- | :--- | :--- |
| `POST` | `/register` | Creates a new user with default `User` role. | Public |
| `POST` | `/login` | Authenticates user and returns **JWT Token**. | Public |
| `GET` | `/user/{id}` | Retrieves user profile details. | Authorized (`User` only sees self; `Accountant` sees all) |

### 2\. Loan Management (User Access) (`/api/Loan`)

| Method | Endpoint | Description | Security & Rules |
| :--- | :--- | :--- | :--- |
| `POST` | `/` | Submits a new loan application. | **Rule:** User must **not** be `IsBlocked`. |
| `GET` | `/my-loans` | Views all applications submitted by the logged-in user. | Authorized (`User` only) |
| `PUT` | `/{id}` | Updates an existing application. | **Rule:** Only allowed if status is **`InProcess`**. |
| `DELETE` | `/{id}` | Deletes an application. | **Rule:** Only allowed if status is **`InProcess`**. |

### 3\. Accountant / Admin Controls (`/api/Accountant`)

Requires **`Accountant`** role. These endpoints bypass standard user restrictions.

| Method | Endpoint | Description | Security & Rules |
| :--- | :--- | :--- | :--- |
| `GET` | `/all-loans` | Retrieves all loans in the database (system-wide view). | Authorized (`Accountant` only) |
| `PATCH` | `/loan/{id}/status` | Force change a loan's status (e.g., to `Approved` or `Rejected`). | Authorized (`Accountant` only) |
| `PATCH` | `/block-user/{id}` | Sets a user's `IsBlocked` field to **`true`**, restricting loan applications. | Authorized (`Accountant` only) |
| `PUT` | `/loan/{id}` | Force update the amount/period of any loan, regardless of status. | Authorized (`Accountant` only) |
| `DELETE` | `/loan/{id}` | Force delete any loan, regardless of status. | Authorized (`Accountant` only) |

-----

## 🔑 Project Dependencies

| Package | Purpose |
| :--- | :--- |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Handles JWT token validation. |
| `Microsoft.EntityFrameworkCore.SqlServer` | Database provider for SQL Server. |
| `AutoMapper.Extensions.Microsoft.DependencyInjection` | Simplifies mapping between Models and DTOs. |
| `Serilog.AspNetCore` | Centralized structured logging to files. |
| `FluentValidation.AspNetCore` | Enforces request data integrity and validation. |
| `Microsoft.AspNetCore.OpenApi` | Generates the Swagger/OpenAPI documentation. |
