# WpfGisLearning Copilot Instructions

## Project context
- WPF desktop application targeting `net10.0-windows10.0.26100.0`.
- Nullable reference types and implicit usings are enabled.
- Current notable packages include CommunityToolkit.Mvvm, Microsoft.Extensions.Hosting, Mapsui.Wpf, and ClosedXML.
- Prefer the existing project architecture and patterns over introducing new dependencies.

## Development principles
- Use MVVM consistently for application behavior.
- Keep dependency injection registrations and lifetimes intentional and consistent.
- Keep UI-only behavior in Views/code-behind only when it is inherently view-specific WPF behavior.
- Avoid unrelated refactors during feature work.
- Preserve existing behavior unless the requirement explicitly changes it.
- Do not hide compiler/test failures by suppressing diagnostics or weakening tests.

## Verification
When applicable, validate with:
- `dotnet build`
- `dotnet test`

## AI team workflow
For substantial tasks, prefer the custom-agent workflow:
`wpf-development-manager` → `wpf-planner` → `wpf-developer` → `wpf-qa` → `wpf-reviewer` → correction loop as needed.

The reviewer must remain independent from the implementation agent. The manager should stop after a bounded number of correction cycles rather than looping indefinitely.

## Git safety
AI agents may edit and verify the working tree, but must not publish work automatically. `git commit`, `git push`, `git merge`, PR creation, PR merge, and other irreversible Git publication actions are human-controlled unless explicitly changed by a future project rule.
