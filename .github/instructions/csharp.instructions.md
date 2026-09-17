---
applyTo: "**/*.cs"
---
# C# instructions

- Keep nullable reference types enabled and address nullable warnings rather than suppressing them without justification.
- Follow the existing MVVM and dependency-injection architecture.
- Prefer small, focused classes and methods.
- Keep ViewModels independent from concrete WPF controls.
- Prefer constructor injection for dependencies.
- Do not introduce a new framework or package when an existing project abstraction is sufficient.
- Preserve existing public contracts unless the task requires a breaking change.
- Handle exceptions at an appropriate boundary; do not catch and ignore failures.
- Avoid `async void` except for inherently event-based WPF event handlers.
- Do not use service locators or global mutable state to bypass DI.
- Keep business/application behavior testable without requiring a running WPF UI when practical.
- When changing behavior, update or add focused tests.
- Run `dotnet build` and `dotnet test` after substantive changes when the environment permits.
