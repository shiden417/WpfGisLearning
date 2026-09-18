---
applyTo: "**/*Tests.cs,**/*.Tests/**/*.cs,**/WpfGisLearning.Tests/**/*.cs"
---
# Test instructions

- Use the existing test framework and project conventions.
- Prefer focused unit tests with clear Arrange / Act / Assert structure.
- Test observable behavior, not implementation details, unless implementation constraints are part of the requirement.
- Add regression tests when fixing a defect.
- Cover both successful and relevant failure/edge cases.
- Keep tests deterministic and independent from network access or machine-specific state whenever practical.
- Do not weaken assertions, delete tests, or skip tests merely to make the suite pass.
- When production code changes, inspect whether existing tests remain meaningful and add coverage where behavior changed.
- For bound filtered/count properties, cover initial state, filter changes, empty results, clearing/resetting, and `PropertyChanged` notifications when those transitions are relevant.
- Run `dotnet test` after substantive changes and report any environment-related limitation.
