# PracticeReload — GastroFest Catalog API & Web Application

A full-stack web application designed for managing and browsing food festival events, built with modern .NET core principles and a clean multi-layered architecture.

## 🏗️ Architecture Overview

The project is strictly separated into distinct logical layers to enforce the Separation of Concerns (SoC) principle:
*   **Domain**: Contains core enterprise business entities and models.
*   **DAL (Data Access Layer)**: Manages database context, entities persistence, and infrastructure configuration using Entity Framework Core.
*   **Services**: Implements business logic layer (BLL), processing data validation, and core application services.
*   **Web**: The presentation layer built on ASP.NET Core MVC, handling user routing, controllers, and views.

## 🛠️ Tech Stack

*   **Backend:** C# Development, .NET Core, ASP.NET Core MVC
*   **Database & ORM:** PostgreSQL, Entity Framework Core (EF Core), LINQ queries
*   **Tools:** Git for version control, DBeaver/pgAdmin for database management

## 🚀 Getting Started

### Prerequisites
*   .NET SDK (8.0 or higher recommended)
*   PostgreSQL database instance

### Configuration
Update the database connection string in `Web/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=your_host;Database=GastroFestDb;Username=your_user;Password=your_password"
}
