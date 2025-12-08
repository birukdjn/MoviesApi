# MoviesApi

MoviesStore is a web application with a backend API designed to manage movie data, including user authentication and core CRUD operations.

## Features ✨

Your MoviesApi boasts a comprehensive set of features:

1.  **User Management:** Full user registration, secure login capabilities, and profile management.
2.  **Authentication:** Utilizes **JSON Web Tokens (JWT)** for securing all protected endpoints.
3.  **Role-Based Authorization:** Implements different access levels (e.g., **Admin**, **Basic User**) to control endpoint usage.
4.  **Movie CRUD:** Supports viewing, adding, editing, and deleting movie records.
5.  **Filtering and Searching:** Allows users to query movies by **title**, **genre**, or **release year**.
6.  **Pagination:** Enables efficient loading of large datasets by breaking movie lists into manageable pages.
7.  **Data Validation:** Ensures all incoming data meets defined integrity and business rules.
8.  **Error Handling:** Provides consistent, detailed, and non-sensitive **API response formats** for errors (e.g., using HTTP status codes like 400, 401, 404).
9.  **Logging:** Implements application-wide logging (e.g., using Serilog) to track API requests, errors, and system events.
10. **Asynchronous Operations:** Leverages C#'s `async`/`await` pattern to ensure high performance and non-blocking I/O.
11. **API Testing:** Integrated **Swagger UI** for easy endpoint documentation, visualization, and interactive testing.
12. **Environment Configuration:** Supports flexible configuration management for different environments (e.g., Development, Staging, Production) using `appsettings.json`.

---

## Technologies Used

| Category | Technology | Version / Description |
| :--- | :--- | :--- |
| **Backend** | C# **ASP.NET Core Web API** | Targeting **.NET 10** |
| **Database** | **Entity Framework Core** | ORM for database interaction |
| **Persistence** | **SQL Server** | Primary database system |
| **Security** | **JWT Authentication** | Token-based security standard |
| **Tooling** | **Swagger** | API documentation and testing interface |

---

## Getting Started 🚀

Follow these steps to clone and run the project locally.

### Prerequisites

You'll need the following installed on your system:

* [.NET SDK](https://dotnet.microsoft.com/download)
* **SQL Server** or **LocalDB** installed and running
* **Git**

### Clone and Build the Project

Use the following commands in your terminal to clone the source code and prepare the project.


# Clone the repository
```bash
git clone https://github.com/birukdjn/MoviesApi.git
```
# Navigate into the project directory
```bash
cd MoviesApi/Backend
```

# Restore project dependencies
```bash
dotnet restore
```

# Build the project
```bash
dotnet build
```
# Apply migrations to update/create the database schema
```bash
dotnet ef database update
```
# Start the API server
```bash
dotnet run
```

# Access Swagger UI
Once the application is running, open your web browser and navigate to the Swagger documentation interface to view and test all available API endpoints.

Navigate to :https://localhost:7218/swagger/index.
