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
- Git

## Quick Start (Recommended for beginners)

Run database in Docker, backend in Visual Studio, frontend in VS Code.

1. Clone and open terminal at project root:

```powershell
git clone <your-repo-url>
cd asp-net
```

2. Start SQL Server container:

```powershell
docker compose up -d sqlserver
docker ps
```

You should see `notesapp-sqlserver` with status `healthy`.

3. Run backend in Visual Studio:
- Open `backend/NotesApi/NotesApi.csproj`
- Select profile `https` (top toolbar)
- Press `F5`
- Confirm API is running at `https://localhost:7287`

4. Run frontend in VS Code terminal:

```powershell
cd frontend
copy .env.example .env
npm install
npm run dev
```

5. Open app:
- `http://localhost:5173`

## Option A: Required workflow (same as quick start)

- DB: Docker (`sqlserver` service)
- Backend: Visual Studio
- Frontend: VS Code terminal

## Option B: Full Docker stack

```powershell
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

## Common issues (for new users)

1. `SqlException: Cannot open database 'NotesAppDb' requested by the login`
- Fix:
```powershell
docker compose down -v
docker compose up -d sqlserver
```
- Then restart backend from Visual Studio.

2. Frontend cannot call API / CORS / network error
- Confirm backend is running at `https://localhost:7287`.
- Confirm `frontend/.env` contains:
```env
VITE_API_BASE_URL=https://localhost:7287/api
```
- Restart `npm run dev` after changing `.env`.

3. HTTPS certificate warning in browser
- For local development only, proceed once and trust localhost dev certificate if prompted.

4. Port already in use (`1433`, `5173`, or `7287`)
- Stop the process/container using that port, then retry.

## Stop services

```powershell
docker compose down
```

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
