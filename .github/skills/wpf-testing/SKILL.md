# WPF Testing Skill

Use this skill when implementing, debugging, or reviewing tests for this repository.

## Workflow
1. Inspect the affected production code and existing tests.
2. Identify the observable behavior that must be preserved or changed.
3. Add or update focused tests before considering the change complete.
4. Run `dotnet test` for the solution.
5. If the test suite fails, classify each failure as a product defect, test defect, or environment limitation.
6. Never fix a failing test by weakening its assertions without a clear requirement-based reason.

## WPF-specific guidance
- Prefer testing ViewModel, service, and application logic without starting a real WPF window when practical.
- For UI-specific behavior, keep tests narrowly scoped and avoid coupling them to incidental layout details.
- Treat Dispatcher/threading behavior as a separate concern and verify it explicitly when it is part of the requirement.
- When commands are involved, test command execution and resulting state rather than private implementation details.

## Completion criteria
A task is not test-complete until:
- relevant tests exist or the reviewer has a documented reason why they are unnecessary;
- `dotnet test` succeeds, or the failure is clearly reported as an environment limitation;
- no existing test was removed or weakened solely to obtain a passing result.
