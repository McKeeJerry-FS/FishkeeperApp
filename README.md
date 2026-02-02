# AquaHub.MVC

A minimal ASP.NET Core MVC application with Entity Framework Core, ASP.NET Core Identity, and dual database support (SQLite for development, PostgreSQL for production).

## Features

- **ASP.NET Core 8.0 MVC** - Modern web framework with Model-View-Controller pattern
- **Entity Framework Core 8.0** - ORM for database operations
- **ASP.NET Core Identity** - Built-in authentication and user management
- **Dual Database Support**:
  - SQLite for local development
  - PostgreSQL (via Npgsql) for production
- **Railway-Ready** - Configured for easy deployment to Railway
- **Docker Support** - Includes Dockerfile for containerization

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio 2022](https://visualstudio.microsoft.com/)
- (Optional) [Docker](https://www.docker.com/) for containerization

## Getting Started

### 1. Clone or Download the Project

```bash
cd /path/to/AquaHub.MVC
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Create Initial Database Migration

The project uses Entity Framework Core migrations. Create the initial migration:

```bash
dotnet ef migrations add InitialCreate
```

### 4. Run the Application

```bash
dotnet run
```

The application will start on `http://localhost:5000` (or `https://localhost:5001` for HTTPS).

## Development

### Database Configuration

#### Local Development (SQLite)

The application uses SQLite by default in development mode. The database file `aquahub.db` will be created automatically in the project root when you run the application.

Connection string is configured in `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=aquahub.db"
  }
}
```

#### Production (PostgreSQL)

In production (Railway), the application reads the `DATABASE_URL` environment variable and automatically configures PostgreSQL. Railway provides this variable when you add a PostgreSQL database to your service.

### Entity Framework Core Commands

- **Add a new migration**: `dotnet ef migrations add <MigrationName>`
- **Update database**: `dotnet ef database update`
- **Remove last migration**: `dotnet ef migrations remove`
- **List all migrations**: `dotnet ef migrations list`

### Project Structure

```
AquaHub.MVC/
├── Controllers/         # MVC Controllers
├── Data/               # Database context and migrations
├── Models/             # Data models and view models
├── Views/              # Razor views
├── wwwroot/            # Static files (CSS, JS, images)
├── Program.cs          # Application entry point
├── appsettings.json    # Configuration
└── Dockerfile          # Docker configuration
```

## Deployment to Railway

### Prerequisites

- [Railway CLI](https://docs.railway.app/develop/cli) (optional)
- Railway account

### Deployment Steps

1. **Initialize Railway Project** (if not already done):

   ```bash
   railway login
   railway init
   ```

2. **Add PostgreSQL Database**:
   - In Railway dashboard, click "New" → "Database" → "PostgreSQL"
   - Railway automatically sets the `DATABASE_URL` environment variable

3. **Deploy**:

   ```bash
   railway up
   ```

   Or connect your GitHub repository in the Railway dashboard for automatic deployments.

4. **Set Environment Variables** (if needed):
   Railway automatically provides:
   - `PORT` - The port your application should listen on
   - `DATABASE_URL` - PostgreSQL connection string (when database is added)

### Railway Configuration

The application is configured to:

- Listen on the port specified by the `PORT` environment variable
- Automatically parse and use the `DATABASE_URL` for PostgreSQL connections
- Run database migrations on startup
- Use HTTPS redirection and HSTS in production

## Docker

### Build Docker Image

```bash
docker build -t aquahub-mvc .
```

### Run Docker Container Locally

With SQLite (development):

```bash
docker run -p 5000:5000 -e ASPNETCORE_ENVIRONMENT=Development aquahub-mvc
```

With PostgreSQL:

```bash
docker run -p 5000:5000 \
  -e DATABASE_URL=postgres://user:password@host:5432/database \
  -e PORT=5000 \
  aquahub-mvc
```

## Authentication

The application includes ASP.NET Core Identity for authentication. To add login/register pages:

```bash
dotnet tool install -g dotnet-aspnet-codegenerator
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
dotnet aspnet-codegenerator identity -dc AquaHub.MVC.Data.ApplicationDbContext
```

This will scaffold Identity UI pages (Login, Register, etc.) into your project.

## Configuration

### appsettings.json

The main configuration file contains:

- Connection strings
- Logging configuration
- Application-specific settings

### Environment Variables

- `ASPNETCORE_ENVIRONMENT` - Set to "Development" or "Production"
- `DATABASE_URL` - PostgreSQL connection string (automatically set by Railway)
- `PORT` - Port number for the application (automatically set by Railway)

## Troubleshooting

### Database Issues

**Migration not applied:**

```bash
dotnet ef database update
```

**Reset database:**

```bash
dotnet ef database drop
dotnet ef database update
```

### Railway Deployment Issues

**Check logs:**

```bash
railway logs
```

**Restart service:**

```bash
railway up --detach
```

## Technologies Used

- [ASP.NET Core 8.0](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core 8.0](https://docs.microsoft.com/ef/core)
- [ASP.NET Core Identity](https://docs.microsoft.com/aspnet/core/security/authentication/identity)
- [SQLite](https://www.sqlite.org/)
- [PostgreSQL](https://www.postgresql.org/) with [Npgsql](https://www.npgsql.org/)
- [Railway](https://railway.app/)

## License

This project is provided as-is for educational and development purposes.

## Support

For issues and questions:

- Check the [ASP.NET Core documentation](https://docs.microsoft.com/aspnet/core)
- Visit the [Railway documentation](https://docs.railway.app/)
- Review the [Entity Framework Core documentation](https://docs.microsoft.com/ef/core)
