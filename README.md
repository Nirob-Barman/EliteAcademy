# EliteAcademy

An ASP.NET Core 8 MVC online academy platform supporting three roles — **Admin**, **Instructor**, and **Student** — with class management, enrollment, payments, Q&A, reviews, announcements, and notifications.

## Tech Stack

- **ASP.NET Core 8 MVC** — Razor views, Bootstrap 5, SweetAlert2, Font Awesome 6
- **EF Core 8** — SQL Server (SQLEXPRESS)
- **ASP.NET Identity** — cookie-based auth
- **Clean Architecture** — Domain → Application → Infrastructure → Web

## Prerequisites

- .NET 8 SDK
- SQL Server (SQLEXPRESS)
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

## Getting Started

1. **Clone** the repository
2. **Configure** `appsettings.json` (see `appsettings.sample.json`):
   ```json
   {
     "ConnectionStrings": { "DefaultConnection": "Server=.\\SQLEXPRESS;Database=EliteAcademyDb;Trusted_Connection=True;" },
     "EmailSettings": { "SmtpServer": "smtp.gmail.com", "Port": 587, "SenderEmail": "", "Password": "" }
   }
   ```
3. **Apply migrations** and seed the database:
   ```bash
   dotnet ef database update --project EliteAcademy.Infrastructure --startup-project EliteAcademy.Web
   ```
4. **Run**:
   ```bash
   dotnet run --project EliteAcademy.Web
   ```

## Seeded Accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@eliteacademy.com | Admin@123 |
| Instructor | james@eliteacademy.com | Instructor@123 |
| Student | alice@eliteacademy.com | Student@123 |

## Features

| Area | Capabilities |
|------|-------------|
| **Student** | Browse & filter classes, cart (pre-enrollment), coupon codes, payment, wishlist, reviews, Q&A, announcements |
| **Instructor** | Class CRUD (image, price, seats), student roster, Q&A answers, announcements |
| **Admin** | User management (ban/unban, role change), class approval, instructor applications, revenue reports, audit logs, payment gateways |
| **Account** | Edit profile, change password, notification preferences, login history |
| **Payments** | Pluggable gateway system (Mock, Stripe Checkout, SSLCommerz, bKash, SurjoPay) |
| **Notifications** | In-app notification centre + per-user email/in-app preference toggles |

## Project Structure

```
EliteAcademy.Domain/          # Entities, enums — zero dependencies
EliteAcademy.Application/     # Services, DTOs, interfaces, Result<T>
EliteAcademy.Infrastructure/  # EF Core, Identity, FileStorage, Email, Payments
EliteAcademy.Web/             # Controllers, Razor views, ViewModels
```

## Development

```bash
dotnet build
dotnet ef migrations add <Name> --project EliteAcademy.Infrastructure --startup-project EliteAcademy.Web
```

See [USER_MANUAL.md](USER_MANUAL.md) for end-user documentation.
