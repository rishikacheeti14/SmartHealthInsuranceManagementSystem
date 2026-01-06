# Smart Health Insurance Management System

A modern, full-stack web application for managing health insurance policies, claims, and members. Built with **ASP.NET Core 8 Web API** and **Angular**.

##  Features
- **Role-Based Access Control (RBAC):** Admin, Agent, Claims Officer, Hospital, and Customer.
- **Policy Management:** Create and manage insurance plans.
- **Claims Processing:** End-to-end claim submission and approval workflow.
- **Reporting:** Analytical dashboards for claims and premiums.
- **Secure:** JWT Authentication and Password Hashing.

## Technology Stack
- **Backend:** C#, .NET 8 Web API, Entity Framework Core, SQL Server
- **Frontend:** Angular 18+, TypeScript, Angular Material / Bootstrap
- **Database:** SQL Server
- **Testing:** xUnit (Backend)

---

## Setup Instructions

### 1. Prerequisites
Ensure you have the following installed:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (LTS version)
- SQL Server (LocalDB or full instance)
- Angular CLI (`npm install -g @angular/cli`)

### 2. Backend Setup (.NET API)
1.  Navigate to the API project folder:
    `cd SmartHealthInsurance.Api`
2.  Update the connection string in `appsettings.json` if needed (default is SQL Server LocalDB).
3.  Apply database migrations (this will create the DB):
    `dotnet ef database update`
4.  Run the backend:
    `dotnet run`
    *The API will start at http://localhost:5000 (or configured port).*

### 3. Frontend Setup (Angular)
1.  Navigate to the UI project folder:
    `cd smart-health-insurance-ui`
2.  Install dependencies:
    `npm install`
3.  Run the application:
    `ng serve`
    *The application will be available at http://localhost:4200.*

---

## Testing

### Backend Tests
Run the following command in the solution root:
`dotnet test`

### Frontend Tests
Run the following command in the `smart-health-insurance-ui` folder:
`ng test`

---

## Default Credentials (Data Seeder)
The application automatically seeds a default Admin user on first run.

| Role | Email | Password |
|------|-------|----------|
| **Admin** | `admin@health.com` | `Admin@123` |

*Note: You can create other users (Agents, Hospital Managers) via the Admin Dashboard.*
