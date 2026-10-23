# JSONPlaceholder API tests (C#)

Automated API tests for the Users, Posts, Comments and Todos endpoints of
[JSONPlaceholder](https://jsonplaceholder.typicode.com/guide/).

Stack: .NET 9, NUnit 4, HttpClient, System.Text.Json.

## Quick start

```bash
dotnet test
```

Windows PowerShell, to open the report afterwards:

```powershell
Start-Process .\reports\report.html
```

`dotnet test` restores packages and builds on its own. Reports go to `reports/`:
`report.html` for reading and `results.trx` for CI tools. Every test keeps its HTTP log
(request, status, timing) in its output, so a failure in the report shows what was sent.

## Expected result

```text
Passed: 67, Skipped: 13, Total: 80
```

The 13 skipped tests are **known API defects**, not a setup problem. They check that the
API rejects invalid input, but JSONPlaceholder is a demo service without validation:

- **JP-001** (12 tests): invalid payloads are accepted with HTTP 201 instead of 400/422.
- **JP-004** (1 test): malformed JSON returns HTTP 500 instead of 400.

Details and reproduction steps: [docs/KNOWN_LIMITATIONS.md](docs/KNOWN_LIMITATIONS.md).
The check is strict: if the API starts validating input, these tests fail as a reminder
to remove the `KnownIssue.Expect` wrapper.

## Test scopes

| Command | What runs |
| --- | --- |
| `dotnet test` | Everything (80 tests) |
| `dotnet test --filter TestCategory=smoke` | 7 quick checks of the main endpoints |
| `dotnet test --filter TestCategory=full` | All 50 API tests |
| `dotnet test --filter TestCategory=negative` | Missing resources and invalid input |
| `dotnet test --filter TestCategory=unit` | 30 offline tests of the framework itself (no internet needed) |
| `dotnet test --filter TestCategory=users` (`posts`, `comments`, `todos`) | One resource |
| `dotnet test --filter "TestCategory=full&TestCategory!=contract"` | API tests without the known defects |

## Configuration

| Run parameter | Environment variable | Default |
| --- | --- | --- |
| `BaseUrl` | `API_BASE_URL` | `https://jsonplaceholder.typicode.com` |
| `ConnectTimeout` | `API_CONNECT_TIMEOUT` | 3 s |
| `RequestTimeout` | `API_REQUEST_TIMEOUT` | 10 s, per attempt |
| `Retries` | `API_RETRIES` | 2 (0..5) |

Run parameters override environment variables. The environment variable is the easy way:

```powershell
$env:API_RETRIES = "0"; dotnet test --filter TestCategory=smoke
```

A run parameter is passed after `--` (bash quoting shown; in PowerShell the environment
variable is simpler):

```bash
dotnet test -- 'TestRunParameters.Parameter(name="Retries",value="0")'
```

An invalid value stops the run before any API test starts, with a message naming the setting.

Only GET requests are retried, on temporary server errors (429, 5xx), network errors and
timeouts. Writes are never retried: a repeated POST could create the same resource twice.

## Project structure

```text
src/JsonPlaceholder.ApiTesting/       Framework, does not depend on NUnit
    ApiSettings.cs        Settings from run parameters / environment, with validation
    ApiClient.cs          HttpClient: per-attempt timeouts, retries, logging
    ApiResponse.cs        Response with the body already read
    ApiJson.cs            Strict JSON options, serialization, pretty printing
    ApiAssert.cs          Reusable checks: status, JSON, schema
    Api/                  UsersApi, PostsApi, CommentsApi, TodosApi — one method per endpoint
    Models/               Records for requests and responses
tests/JsonPlaceholder.ApiTests/
    Api/                  API tests and ApiSession (one shared client)
    Unit/                 Offline tests of the client, settings and contracts
    Support/              KnownIssue, NUnit logger
docs/KNOWN_LIMITATIONS.md
```

A test calls an API method, gets an `ApiResponse` and checks it with `ApiAssert`.
API methods contain no assertions, so negative tests can inspect 4xx/5xx responses.
On failure, the message shows the request, HTTP status and a pretty-printed response body.

Response models are strict on purpose: `"1"` is not accepted as a number, `user_id` is not
accepted as `userId`, `null` is not accepted where a string is required, and IDs must be
positive. A contract change in the API should fail the tests, not slip through.

## Requirement coverage

| # | Scenario | Test |
| --- | --- | --- |
| 1 | `GET /users` | `GetUsers_ReturnsValidListWithUniqueIds` |
| 2 | `GET /users/{id}` | `GetUserById_ReturnsThatUser` |
| 3 | `GET /users/9999` | `GetUser_WithMissingOrInvalidId_Returns404` |
| 4 | `GET /posts` | `GetPosts_ReturnsValidListWithUniqueIds` |
| 5 | `POST /posts` | `CreatePost_EchoesSubmittedFields` |
| 6 | `PUT /posts/{id}` | `UpdatePost_ReplacesAllFields` |
| 7 | `PATCH /posts/{id}` | `PatchPost_ChangesOnlySubmittedField` |
| 8 | `DELETE /posts/{id}` | `DeletePost_ReturnsEmptyObject` |
| 9 | `GET /posts/{id}/comments` | `GetCommentsForPost_ReturnsOnlyThatPostsComments` |
| 10 | Invalid payload | `CreatePost_WithInvalidPayload_IsRejected`, `CreatePost_WithMalformedJson_IsRejected` |

Extra coverage: todos (list, filters, by ID, missing), filtering posts by user,
Unicode / long / special-character content, invalid user IDs, a missing post,
comments of a missing post.

## Adding a test

1. Add an endpoint method to a class in `Api/` (or a new class based on `ResourceApi`)
   and a record to `Models/` if needed.
2. Write the test in `tests/JsonPlaceholder.ApiTests/Api/`. Create the API object from
   `ApiSession.Client`, mark the class `[Category("full")]` plus the resource category,
   and add `smoke` to a test if it is a key check.
3. Name the test `Action_ExpectedResult`: the name is what the report shows.

## CI

GitHub Actions checks formatting (`dotnet format`), builds with warnings as errors, then
runs the offline tests and the full API suite for every push and pull request.
Reports are kept as build artifacts for 14 days.
