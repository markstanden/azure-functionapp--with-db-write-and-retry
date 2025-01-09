# Coding Challenge
# Coding Challenge - Azure FunctionApp

## Development Setup

### Prerequisites
- .NET 8.0
- CSharpier (Global Tool): `dotnet tool install -g csharpier`

### Git Hooks
This project uses Git hooks for automated code formatting. To set up:

1. Ensure CSharpier is installed (see prerequisites)
2. Configure Git to use the project hooks:
   ```bash
   git config core.hooksPath .hooks
   ```

The pre-commit hook will automatically format C# files using CSharpier before each commit.

### Code Style
- Code formatting is handled automatically by CSharpier
- Configuration can be found in `.csharpierrc`

## Notes

### DB

I've created my own DB tables to progress with development, I've called these

1. `markShipment`
1. `markShipment_Lines`

Without access to the 'real' database tables / scripts I've had to make a few assumptions about how the DB should be structured.

1. I've used the shipmentID as the primary key for the markShipment table.  This allows for testing of SQL addition errors by attempting to insert an entry already in the table.

### Input Sanitation

I've assumed that sanitation of input is required, although not explicitly requested. The API endpoint *is* protected by an API key, but it is for external use, and I would consider sanitation of supplied data to be essential. 

1. `shipmentID` contains only alphanumerics in the sample JSON, but to make the sanitation a bit more interesting, I have assumed hyphens are also allowable.
1. `sku` is sanitized as alphanumerics only.
1. I have assumed that attempting to write a sanitized string is preferable to failing validation for invalid characters, so inputs with additional characters will be cleansed and then written.

### Retry Logic

The retry logic waits in function, as suggested by the infrastructure diagram, as opposed to overcomplicating the functionapp by rescheduling a message back into the queue.
- I have implemented my own recursive retry method
- I could have used [Polly](https://www.pollydocs.org/) for out of the box retry (and in fact did in a past version), but I felt I could demonstrate both unit-testing and coding skill by writing my own.

### Unit Testing

I have unit tested using xUnit as a test framework and Moq for class isolation and mocking.

I favour test isolation using interfaces.

I did have some trouble with closed classes, which I have wrapped with interfaces to allow mocks to be created.

I have conducted non-exhaustive unit testing as part of the development, and conducted development for the Retry and Sanitation classes using TDD.
Although not part of the requirements for the challenge, I feel it illustrates a key part of my workflow.

### Webhook Http Request

- The functionApp makes a GET request to [webhook.site](https://webhook.site/#!/view/5b83a7db-90e0-4ae8-ab49-a5df2474665f/9514a315-20c0-42cb-9e2b-7be2c3355277/1) on success
- When testing I hit the limit a few times to I have moved the webhook url into configuration to allow this to be changed without a re-deployment.