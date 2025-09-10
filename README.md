# 📇 Contact Manager App

A small educational project for managing contacts, created as part of a test task.

---

## 🚀 Features

- **View contacts** in a styled table with alternating row colors and pagination.
- **Create, edit and delete contacts**.
- **Inline editing** of fields (without full page reload).
---

## 🛠️ Technologies

### Backend
- **ASP.NET Core MVC** — main framework.
- **C# / .NET 9** — language and platform.
- **Entity Framework Core** — ORM for database access.
- **SQL Server LocalDB** — database.

### Frontend / UI
- **Razor Views (MVC)** — UI rendering.
- **Bootstrap 5** — styling for tables, buttons, and forms.
- **Custom table design**: alternating row colors, action buttons with different colors.

### Development Tools
- **Git + GitHub** — version control and code hosting.

### Testing
- **xUnit** — unit testing framework.
- **NSubstitute** — mocking library.

---

## ⚙️ Installation & Run

**Clone the repository:**
```bash
git clone git@github.com:IvanovvvRuslan/ContactManagerApp_test_task.git
cd ContactManagerApp_test_task
```

****Configure the database:****
Update the connection string in appsettings.json (SQL Server LocalDB).
Apply migrations:
```bash
dotnet ef database update
```

****Run the application:****
```bash
dotnet run
```
or start from IDE.

****The app will be available at:****
👉 http://localhost:5220/ (default for this project, but the port may differ on other machines).

## 📂 Project Structure
- **Controllers/** — MVC controllers.
- **Views/** — Razor views.
- **Models/** — data models.
- **wwwroot/** — static files (CSS, JS).
- **Data/** — EF Core DbContext.
- **Tests/** — unit tests (xUnit).

## 🔧 Possible Extensions
- Add search and filtering for contacts.
- Extend the contact model (phone, email, address).
- Expose a REST API.
- Add a modern frontend (Blazor or React).
