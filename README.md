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

## CI/CD Pipeline
The project uses GitHub Actions workflows to enforce code quality:

### Workflow Structure
- **Code Quality**: Validates code formatting using CSharpier
- **Compilation**: Ensures the solution builds successfully
- **Code Coverage**: Runs tests and verifies 80% minimum test coverage

### Key Features
- Optimized with caching for faster builds
- Automated checks on PRs to main branch
- Workflows work independently
- Manual triggers for development testing
- Failed checks block PR merging

## Architecture & Implementation Notes

### Database Design
I've created two tables for development:

1. `markShipment`
2. `markShipment_Lines`

Without access to the intended database schemas, I made some assumptions about structure. The `shipmentId` serves as the primary key for the markShipment table, allowing for straightforward duplicate entry testing.

### Input Sanitation
While not explicitly required, Input sanitation was implemented as a security measure.

1. `shipmentId`: Allows alphanumerics and hyphens
2. `sku`: Strictly alphanumeric
3. Invalid characters are removed rather than causing validation failures

### Retry Logic
The retry mechanism operates within the function rather than rescheduling messages in the queue:
- Custom recursive retry implementation
- Considered Polly but opted for a bespoke solution to demonstrate TDD and unit testing capabilities

### Testing Strategy
The project uses xUnit for testing with Moq for isolation. Key aspects include:

- Interface-based design for effective mocking
- Comprehensive unit test coverage
- TDD approach for core components (Retry, Sanitation)
- Interfaces and wrappers for previously closed classes to enable mocking/testing

### Webhook Integration
- Success notifications sent to [webhook.site](https://webhook.site/#!/view/5b83a7db-90e0-4ae8-ab49-a5df2474665f/9514a315-20c0-42cb-9e2b-7be2c3355277/1)
- Webhook URL configurable to avoid rate limiting and enable environment-specific endpoints