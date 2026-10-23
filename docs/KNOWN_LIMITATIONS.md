# Known limitations of JSONPlaceholder

JSONPlaceholder is a public demo API. Some of its behavior differs from what a real API
would do, and the tests account for that.

## JP-001: Invalid post payloads are accepted

The negative tests assume that `POST /posts` requires a nonempty `title`, a nonempty `body`
and a positive integer `userId`, and that invalid input returns HTTP 400 or 422.
This is our assumption for the assignment; JSONPlaceholder does not promise validation.

In reality the service returns HTTP 201 and echoes back whatever was sent:

```http
POST /posts
Content-Type: application/json

{}
```

```http
HTTP/1.1 201 Created

{"id": 101}
```

The 12 cases of `CreatePost_WithInvalidPayload_IsRejected` (missing fields, wrong types,
empty strings, zero/negative `userId`, empty object, no body) are wrapped in
`KnownIssue.Expect("JP-001", ...)`. While the issue reproduces, they are reported as
**Skipped** with the issue ID, and the full request/response is written to the test output.
If the API starts validating input, the tests **fail** with a reminder to remove the wrapper.

To see only these tests:

```bash
dotnet test --filter TestCategory=contract --logger "console;verbosity=detailed"
```

## JP-002: Writes are not persisted

POST, PUT, PATCH and DELETE are simulated: the response looks real, but nothing is saved
([official guide](https://jsonplaceholder.typicode.com/guide/)). So tests check each write
response on its own and do not read the data back. With a real API, tests would also
read back the result and clean up the created data.

## JP-003: Public service with fixed data

The API tests need internet access and rely on existing data: users 1, 5, 10 and
posts 1, 50, 100. Offline tests (`--filter TestCategory=unit`) use a stub HTTP handler instead.

## JP-004: Malformed JSON causes a server error

```http
POST /posts
Content-Type: application/json

{not a json
```

The expected response is HTTP 400; the service returns HTTP 500 with a JSON parser stack
trace. `CreatePost_WithMalformedJson_IsRejected` is wrapped in `KnownIssue.Expect("JP-004", ...)`.
