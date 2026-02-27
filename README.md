# Notes Application

Full-stack Notes app:
- Frontend: Vue + TypeScript + TailwindCSS (Vite)
- Backend: ASP.NET Core Web API + Dapper
- Database: SQL Server
- Auth: JWT + refresh tokens

## Requirement coverage

- Create note with required `Title` and optional `Content`
- Auto-generated `CreatedAt` and `UpdatedAt`
- Read/list notes with detail view
- Update note title/content and update timestamp
- Delete note and remove it from list
- Search, sorting, pagination
- Optional login/register (implemented)
- Owner-only access to notes (implemented)

## Project structure

- `backend/NotesApi` ASP.NET Core API
- `frontend` Vite app
- `docker-compose.yml` SQL Server + optional API + optional frontend containers

## Prerequisites

- .NET SDK 10+
- Visual Studio (for backend)
- Node.js 20+ and VS Code (for frontend)
- Docker Desktop

## Option A: Required workflow (Frontend in VS Code, Backend in Visual Studio, DB in Docker)

1. Start SQL Server container:

```powershell
cd D:\MyProject\asp-net
docker compose up -d sqlserver
```

2. Run backend in Visual Studio:
- Open `backend/NotesApi/NotesApi.csproj`
- Use `https` profile (API at `https://localhost:7287`)

3. Run frontend in VS Code terminal:

```powershell
cd D:\MyProject\asp-net\frontend
copy .env.example .env
npm install
npm run dev
```

Frontend runs at `http://localhost:5173`.

## Option B: Full Docker stack

```powershell
cd D:\MyProject\asp-net
docker compose up -d --build
```

- Frontend: `http://localhost:5173`
- API: `http://localhost:8080`
- SQL Server: `localhost,1433`

## Configuration notes

- Frontend API base URL is configured by `VITE_API_BASE_URL`.
- Default frontend fallback is `https://localhost:7287/api` for Visual Studio backend.
- Docker frontend build uses `http://localhost:8080/api`.
- If you change SQL password in `docker-compose.yml`, update:
  - Local backend: `backend/NotesApi/appsettings.Development.json`
  - Docker API env: `ConnectionStrings__DefaultConnection`

## API endpoints

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `GET /api/notes?search=&sortBy=updatedAt|createdAt|title&sortDir=asc|desc&page=1&pageSize=10`
- `GET /api/notes/{id}`
- `POST /api/notes`
- `PUT /api/notes/{id}`
- `DELETE /api/notes/{id}`
