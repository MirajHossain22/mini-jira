# Local Verification Report

Verified on October 26, 2026 against the Docker Compose environment at `http://localhost:8080`.

## Result

The Task Tracker API was successfully tested end to end. Authentication, project ownership, PostgreSQL access, task and comment operations, filtering and pagination, Redis caching, and the raw SQL report all behaved as expected.

## Verified capabilities

| Capability | Test performed | Result |
|---|---|---|
| JWT authentication | Registered a user, logged in, and called protected endpoints with the returned Bearer token | Passed |
| Project ownership | Accessed an owned project and requested the same project as a different user | Passed; the second user received `404 Not Found` |
| PostgreSQL access | Applied the EF Core migration and created/read users, projects, tasks, and comments | Passed |
| Project operations | Started with an empty list, created a project, and retrieved it from the project list | Passed |
| Task operations | Created assigned and unassigned tasks and updated a task from `Todo` to `InProgress` | Passed |
| Workflow validation | Attempted the invalid transition `Todo -> Done` | Passed; the API returned `409 Conflict` |
| Comment operations | Added a comment to an owned task and retrieved the task's comments | Passed |
| Filtering and pagination | Filtered Todo tasks by assignee, sorted by due date, and requested page 1 with page size 10 | Passed; the expected single task was returned with pagination metadata |
| Redis caching | Requested list endpoints repeatedly and inspected the generated list/version keys in Redis | Passed |
| Cache invalidation | Updated task/project data and confirmed version keys were incremented | Passed |
| Raw SQL report | Requested task totals grouped by status for an owned project | Passed; the report returned the expected Todo and InProgress totals |
| Swagger/OpenAPI | Loaded the interactive Swagger UI and OpenAPI JSON | Passed; the document returned HTTP 200 |
| Service tests | Ran the xUnit service-layer suite | Passed; 10 of 10 tests succeeded |

## Example verified workflow

The local walkthrough created one project with two tasks:

```text
Localhost Demo
|-- Build login screen       InProgress  High
`-- Write API documentation  Todo        Medium
```

The status report returned:

```json
[
  {
    "status": "InProgress",
    "count": 1
  },
  {
    "status": "Todo",
    "count": 1
  }
]
```

The invalid direct transition from `Todo` to `Done` returned:

```json
{
  "status": 409,
  "title": "Cannot move a task from Todo directly to Done."
}
```

## Environment status

The verified stack consisted of:

- ASP.NET Core API exposed on port `8080`
- PostgreSQL exposed on port `5432`
- Redis exposed on port `6379`
- EF Core `InitialCreate` migration applied
- PostgreSQL and Redis Docker health checks passing

## Reproduce the verification

Start the environment:

```powershell
docker compose up --build -d
docker compose ps
```

Open Swagger at `http://localhost:8080/swagger`, register a user, authorize with the returned JWT, and exercise the endpoints documented in the main README.

Run the automated tests:

```powershell
.\.dotnet\dotnet.exe test TaskTracker.sln
```

Expected test summary:

```text
Passed: 10
Failed: 0
Skipped: 0
```

Screenshots of the rendered API documentation and test run are available in [`docs/screenshots`](screenshots/).
