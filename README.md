User Permissions App
A permission management system built with Angular, ASP.NET Core Web API, Entity Framework Core, and SQL Server.

This application allows administrators to manage users, groups, and permissions, and control access by assigning permissions to users through groups.

Features
Create and manage users
Create and manage groups
Create and manage permissions
Assign users to groups
Assign permissions to groups
Manage access control relationships
Tech Stack
Frontend
Angular
TypeScript
HTML / CSS
Backend
ASP.NET Core Web API
C#
Entity Framework Core
SQL Server
Project Structure
user-permissions-app/
  frontend/   # Angular application
  api/        # ASP.NET Core Web API
  README.md
  .gitignore
Architecture Overview
The system follows a group-based permission model:

A User can belong to one or more Groups
A Group can have one or more Permissions
A user's effective access is determined by the permissions assigned to the groups they belong to
Example
User: John
Group: Managers
Permissions:
Create User
Edit User
Assign Permission
If John belongs to the Managers group, then John inherits those permissions.

Main Modules
Frontend Modules
Users
Groups
Permissions
Assignments
Backend Modules
Controllers
Services
Repositories
Models / Entities
DTOs
Data Access
Getting Started
Prerequisites
Make sure you have the following installed:

Node.js
Angular CLI
.NET SDK
SQL Server
Run the Frontend
cd frontend
npm install
ng serve
By default, the Angular app runs on:

http://localhost:4200
Run the API
cd api
dotnet restore
dotnet run
The API runs on the ASP.NET Core ports configured in the project.

Database
The application uses Microsoft SQL Server as the database.

Database-related assets such as the following may be added to this repository later:

Table creation scripts
Stored procedures
Views
Seed scripts
Database documentation
Suggested future database folder structure:

database/
  tables/
  stored-procedures/
  views/
  seeds/
Suggested API Resources
The backend may expose endpoints such as:

/api/users
/api/groups
/api/permissions
/api/user-groups
/api/group-permissions
Future Improvements
Authentication and authorization
Role-based access control
Audit logging
Search and filtering
Pagination
Admin dashboard
Unit and integration tests
Docker support
CI/CD using GitHub Actions
Author
Created by charliedeveloper