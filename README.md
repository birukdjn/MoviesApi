# MoviesApi

MoviesStore is a web application with a backend API to manage movies. You can perform operations like viewing, adding, editing, and deleting movies. It also includes user authentication.

## Features

- User registration and login
- JWT authentication for protected endpoints
- CRUD operations on movies
- API testing with Swagger

## Technologies Used

- C# .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Swagger for API testing
- JWT Authentication

## Getting Started

Follow these steps to run the project locally.

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed
- SQL Server or LocalDB installed
- Git installed

### Clone the Repository

git clone https://github.com/birukdjn/MoviesApi.git
cd MoviesApi
dotnet restore
dotnet build

### Database Setup 💾

Before running the application, you must apply the Entity Framework Core migrations to create the required SQL Server database schema.

dotnet ef migrations add initialMigrations
dotnet ef database update

### Run the Application

dotnet run
````
