---
description: "Code review instructions for GitHub Copilot"
applyTo: "**"
excludeAgent: ["coding-agent"]
---

# Review Guidelines

Review code changes against conventions and patterns established by project maintainers. Respect `.editorconfig` settings and linter violations.

**Reviewer mindset:** Be polite but very skeptical. Your job is to help speed the review process for maintainers, which includes not only finding problems the PR author may have missed but also questioning the completeness of the PR in its entirety. Treat the PR description and linked issues as claims to verify, not facts to accept. Question the stated direction, probe edge cases, and don't hesitate to flag concerns even when unsure.

## Review Language

When performing a code review, respond in **English**.

## Review Priorities

When performing a code review, prioritize issues in the following order:

### 🔴 CRITICAL (Block merge)

- **Security**: Vulnerabilities, exposed secrets, authentication/authorization issues
- **Correctness**: Logic errors, data corruption risks, race conditions
- **Breaking Changes**: API contract changes without corresponding updates in dependent clients, tests, or operational configuration
- **Data Loss**: Risk of data loss or corruption

### 🟡 IMPORTANT (Requires discussion)

- **Code Quality**: Severe violations of SOLID principles, excessive duplication
- **Test Coverage**: Missing tests for critical paths or new functionality
- **Performance**: Obvious performance bottlenecks (N+1 queries, memory leaks)
- **Architecture**: Significant deviations from established patterns

### 🟢 SUGGESTION (Non-blocking improvements)

- **Readability**: Poor naming, complex logic that could be simplified
- **Optimization**: Performance improvements without functional impact
- **Best Practices**: Minor deviations from conventions
- **Documentation**: Missing or incomplete comments/documentation

## General Review Principles

When performing a code review, follow these principles:

1. **Be specific**: Reference exact lines, files, and provide concrete examples
2. **Provide context**: Explain WHY something is an issue and the potential impact
3. **Suggest solutions**: Show corrected code when applicable, not just what's wrong
4. **Be constructive**: Focus on improving the code, not criticizing the author
5. **Recognize good practices**: Acknowledge well-written code and smart solutions
6. **Be pragmatic**: Not every suggestion needs immediate implementation
7. **Group related comments**: Avoid multiple comments about the same topic

## Code Quality Standards

When performing a code review, check for:

### Clean Code

- Descriptive and meaningful names for variables, functions, and classes
- DRY (Don't Repeat Yourself): no code duplication
- Functions should be small and focused (ideally < 40-50 lines)
- Avoid deeply nested code (max 3-4 levels)
- Avoid magic numbers and strings (use configuration values and constants)
- Comments should explain WHY, not WHAT. Code should be self-explanatory where possible.

### Error Handling

- Proper error handling at appropriate levels
- Meaningful error messages
- No silent failures or ignored exceptions
- Fail fast: validate inputs early
- Use appropriate error types/exceptions/http status codes

## Security Review

When performing a code review, check for security issues:

- **Sensitive Data**: No passwords, API keys, tokens, or secrets in code or logs
- **Input Validation**: All user inputs are validated and sanitized
- **SQL Injection**: Use parameterized queries, never string concatenation
- **Authentication**: Proper authentication checks before accessing resources
- **Authorization**: Verify user has permission to perform action
- **Dependency Security**: Check for known vulnerabilities in dependencies

## Testing Standards

When performing a code review, verify test quality:

- **Coverage**: Critical paths and new functionality must have tests
- **Test Names**: Descriptive names that explain what is being tested
- **Test Structure**: Clear Arrange-Act-Assert pattern
- **Independence**: Tests should not depend on each other or external state
- **Assertions**: Use specific assertions, avoid generic assertTrue/assertFalse
- **Edge Cases**: Test boundary conditions, null values, empty collections
- **Mock Appropriately**: Mock external dependencies, not domain logic

## Performance Considerations

When performing a code review, check for performance issues:

- **Database Queries**: Avoid N+1 queries, use proper indexing
- **Algorithms**: Appropriate time/space complexity for the use case
- **Caching**: Utilize ASP.NET HTTP, response, output, and in-memory caching for expensive or repeated operations where appropriate
- **Resource Management**: Proper cleanup of connections, files, streams and other IDisposable resources
- **Pagination**: Large result sets should be paginated
- **Lazy Loading**: Load data only when needed

## Architecture and Design

When performing a code review, verify architectural principles:

- **Separation of Concerns**: Clear boundaries between layers/modules
- **Dependency Direction**: High-level modules don't depend on low-level details
- **Interface Segregation**: Prefer small, focused interfaces
- **Loose Coupling**: Components should be independently testable
- **High Cohesion**: Related functionality grouped together
- **Consistent Patterns**: Follow established patterns in the codebase

## Documentation Standards

When performing a code review, check documentation:

- **API Documentation**: Public .net methods and REST API endpoints must have xml doc comments
- **Complex Logic**: Non-obvious logic should have explanatory comments
- **README Updates**: Update README when adding features or changing setup
- **Code Comments**: Avoid redundant comments that state the obvious

## Comment Format Template

When performing a code review, use this format for comments:

```markdown
**[PRIORITY] Category: Brief title**

Detailed description of the issue or suggestion.

**Why this matters:**
Explanation of the impact or reason for the suggestion.

**Suggested fix:**
[code example if applicable]

**Reference:** [link to relevant documentation or standard]
```

## Review Checklist

When performing a code review, systematically verify:

### Code Quality

- [ ] Code follows consistent style and conventions
- [ ] Names are descriptive and follow naming conventions
- [ ] Functions/methods are small and focused
- [ ] No code duplication
- [ ] Complex logic is broken into simpler parts
- [ ] Error handling is appropriate
- [ ] No commented-out code or TODO without tickets

### Security

- [ ] No sensitive data in code or logs
- [ ] Input validation on all user inputs
- [ ] No SQL injection vulnerabilities
- [ ] Authentication and authorization properly implemented
- [ ] Dependencies are up-to-date and secure

### Testing

- [ ] New code has appropriate test coverage
- [ ] Tests are well-named and focused
- [ ] Tests cover edge cases and error scenarios
- [ ] Tests are independent and deterministic
- [ ] No tests that always pass or are commented out

### Performance

- [ ] No obvious performance issues (N+1, memory leaks)
- [ ] Appropriate use of caching
- [ ] Efficient algorithms and data structures
- [ ] Proper resource cleanup

### Architecture

- [ ] Follows established patterns and conventions
- [ ] Proper separation of concerns
- [ ] No architectural violations
- [ ] Dependencies flow in correct direction

### Documentation

- [ ] Public APIs are documented
- [ ] Complex logic has explanatory comments
- [ ] README is updated if needed
