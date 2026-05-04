# File Analysis Website

File Analysis Website is a three-tier incident reporting and triage application for security-style file analysis workflows. Users submit reports and evidence files, analysts review reports and convert them into incidents, and admins maintain lookup data and user access.

This project was developed for **CS-GY 6083: Principles of Database Systems** at NYU. The focus of the project is to demonstrate relational database design, normalization, E-R modeling, SQL implementation, and a working application that interacts with a MySQL database through an application server.

The presentation can be found in [this link](https://youtu.be/b75tdhrZ9RE).

## High-Level Application Model

The application follows a three-tier architecture:

```text
User / Analyst / Admin
        ↓
Blazor Server Client
        ↓
ASP.NET Core Web API
        ↓
MySQL Database
```

The frontend does not access the database directly. All database operations are handled through the backend API.

## User Roles

The system supports three main access levels:

- `user`: can register, log in, submit reports, upload evidence files, and manage their own reports.
- `analyst`: can review submitted reports, create incidents, link reports to incidents, manage incident details, and add investigative comments.
- `admin`: can manage users, roles, categories, severity values, lookup data, and administrative CRUD screens.

## Architecture

The solution is split into three main projects:

- `client`: Blazor Server UI for authentication, report submission, incident search, incident detail workflows, file upload/download, dashboards, and admin CRUD screens.
- `server`: ASP.NET Core Web API with JWT authentication, role-based authorization checks, EF Core persistence, upload storage, and dashboard/report/incident endpoints.
- `Shared`: entity models and shared types used by both the API and UI.

The database tier is MySQL. EF Core maps the domain model in:

```bash
server/Data/ApplicationDbContext.cs
```

The full SQL rebuild script is located in:

```bash
Database_Schema/full_database_build.sql
```

This script includes table creation, constraints, triggers, procedures, views, and demo data.

## Database Schema

The database is organized around incident reporting, evidence files, and analyst triage.

### Core Tables

- `roles`: stores access roles such as user, analyst, and admin.
- `users`: stores account information, authentication data, role assignment, and timestamps.
- `reports`: stores reports submitted by users.
- `incidents`: stores analyst/admin-created investigations.
comments: stores discussion records attached to incidents.
- `files`: stores evidence file metadata. The API does not expose internal server file paths to the client.
- `report_files`: many-to-many relationship between reports and files.
- `incident_files`: many-to-many relationship between incidents and files.
- `categories`: lookup table for incident categories.
- `severity`: lookup table for incident severity values.

### Important Relationships

```E-R
users ──< reports
users ──< incidents
users ──< comments

incidents ──< reports
incidents ──< comments

reports >──< files
incidents >──< files

categories ──< incidents
severity ──< incidents
```

Relationship explanation:

- One user can submit many reports.
- One analyst/admin can create many incidents.
- One incident can be linked to many reports.
- One incident can have many comments.
- One user can write many comments.
- One report can reference many files.
- One file can be attached to many reports.
- One incident can reference many files.
- One file can be attached to many incidents.
- Each incident has one category and one severity value.

### Normalization

The database design follows relational normalization principles to reduce redundancy, improve data consistency, and make updates safer.

#### Third Normal Form, 3NF

The schema satisfies 3NF because non-key attributes do not depend on other non-key attributes.

Examples:

- `users.role_id` references `roles.role_id`, so role names are not repeated in `users`.
`incidents.category_id` references `categories.category_id`, so category names are not repeated in `incidents`.
- `incidents.severity_id` references `severity.severity_id`, so severity names are not repeated in `incidents`.
- Report status is stored directly on `reports` because it describes the report itself.

### Why the Design Is Close to BCNF but Not Fully BCNF

The database design aims to be close to BCNF because most functional dependencies are based on primary keys, candidate keys, or foreign keys. This reduces redundancy and prevents update anomalies while still keeping the schema practical for a working application.

However, the design does not strictly force every table into perfect BCNF because the project also needs to support application usability, authorization, reporting, and efficient queries. In a real application, strict BCNF can sometimes create too many small tables or require excessive joins, making the system harder to query, maintain, and explain.

For this project, I chose a balanced design:

- Lookup values such as roles, categories, and severity levels are separated into their own tables.
- Many-to-many relationships are handled through join tables.
- Repeated file metadata is stored once in the `files` table.
- User, report, incident, and comment records keep the attributes that directly describe them.

Some practical choices may keep the schema slightly below strict BCNF. For example, status values are stored directly in `reports` as an enum instead of being moved into a separate `report_statuses` lookup table. This is acceptable because report status is a small controlled set of values that is tightly connected to report workflow logic. Similarly, timestamps such as `created_at`, `updated_at`, and `resolved_at` are stored directly on the records they describe, even though they support application behavior and reporting rather than representing separate entities.

Therefore, the schema is normalized enough to avoid unnecessary redundancy, but it is not over-normalized to the point where the application becomes harder to build or use. The goal was to produce a clean, mostly normalized relational model that supports the incident reporting workflow efficiently.

### E-R Diagrams

This project includes two versions of the database design diagram.

#### Principles of Database Systems Notation

One E-R diagram uses the notation from the Principles of Database Systems / Database System Concepts style. This version emphasizes:

- entity sets,
- relationship sets,
- key attributes,
- many-to-one and many-to-many relationships,
- weak/associative relationship structures,
- and participation constraints.

This diagram is useful for explaining the conceptual design of the database before implementation.

![Database E-R Model](/artifacts/database_E-R(DSC6th).png)

#### DataGrip Diagram

The second diagram uses the default DataGrip model notation. This version emphasizes:

- physical table structure,
- primary keys,
- foreign keys,
- join tables,
- column data types,
- and implementation-level relationships.

This diagram is useful for showing how the conceptual E-R model was translated into a MySQL relational schema.
![Database E-R Model](/artifacts/database_E-R_diagram(datatypes).png)

## Setup

1. Install:
    1. .NET 10 SDK
    2. MySQL 8
2. Create the database from the project root:

```PowerShell
cmd /c "mysql -u root -p < Database_Schema\full_database_build.sql"
```

1. Update server/appsettings.json with your local MySQL password:

```JSON
"DefaultConnection": "Server=localhost;Database=file_analysis_website;User Id=root;Password=your-password;"
```

1. Restore dependencies:

```PowerShell
dotnet restore
```

1. Build the solution:

```PowerShell
dotnet build
```

## Run

Start the API:

```bash
dotnet run --project server
```

Start the Blazor client in another terminal:

```bash
dotnet run --project client
```

The client uses the following API base URL by default: `http://localhost:5272/`

If the API port changes, update:

```bash
client/Program.cs
```

## Test Accounts

The runtime seeder and SQL rebuild script create these demo accounts:

| Role | Username | Email | Password |
|------|----------|-------|----------|
| Admin | `admin` | `admin@example.com` | `Admin123!` |
| Analyst | `analyst` | `analyst@example.com` | `Analyst123!` |
| User | `user` | `user@example.com` | `User123!` |

To add or refresh these rows in an existing database without rebuilding everything, run `Database_Schema/add_demo_users.sql` against the `file_analysis_website` database.

## Tests

Run integration tests with:

```bash
dotnet test
```

The tests cover:

- authentication,
- report CRUD,
- incident CRUD,
- comments,
- file upload/download/linking,
- admin CRUD,
- dashboard endpoints,
- and the report-to-incident workflow.

## Project Summary

File Analysis Website demonstrates a normalized relational database design implemented through a three-tier web application. The project separates the client interface, application logic, and database storage into independent layers. The database design uses lookup tables, foreign keys, join tables, timestamps, and constraints to support secure incident reporting and file evidence tracking.
