# MoviesApi

MoviesStore is a web application with a backend API designed to manage movie data, including user authentication and core CRUD operations.

## Features

* **User Management:** Full user registration and secure login capabilities.
* **Authentication:** Utilizes **JSON Web Tokens (JWT)** for securing protected endpoints.
* **Movie CRUD:** Supports viewing, adding, editing, and deleting movie records.
* **API Testing:** Integrated **Swagger UI** for easy endpoint documentation and testing.

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

```bash
# Clone the repository
git clone https://github.com/birukdjn/MoviesApi.git

# Navigate into the project directory
cd MoviesApi

# Restore project dependencies
dotnet restore

# Build the project
dotnet build


