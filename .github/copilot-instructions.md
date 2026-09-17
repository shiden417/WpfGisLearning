# WpfGisLearning Copilot Instructions

## Project context
- WPF desktop application targeting `net10.0-windows10.0.26100.0`.
- Nullable reference types and implicit usings are enabled.
- Current notable packages include CommunityToolkit.Mvvm, Microsoft.Extensions.Hosting, Mapsui.Wpf, and ClosedXML.
- The repository includes `WpfGisLearning.Tests` for automated tests.
- Prefer the existing project architecture and patterns over introducing new dependencies.

## Development principles
- Use MVVM consistently for application behavior.
- Keep dependency injection registrations and lifetimes intentional and consistent.
- Keep UI-only behavior in Views/code-behind only when it is inherently view-specific WPF behavior.
- Keep Mapsui-specific WPF control coupling behind the repository's existing map abstractions where applicable.
- Avoid unrelated refactors during feature work.
- Preserve existing behavior unless the requirement explicitly changes it.
- Do not hide compiler/test failures by suppressing diagnostics or weakening tests.

## Verification
When applicable, validate with:
- `dotnet build`
- `dotnet test`

## AI team workflow
For substantial tasks, prefer the custom-agent workflow:
`WPF Development Manager` → `wpf-planner` → `wpf-developer` → `wpf-qa` → `wpf-reviewer` → conditional `security-reviewer` → correction loop as needed.

Use the Security Reviewer when changes involve authentication, authorization, secrets, external input, file I/O, networking, persistence, or dependency changes.

The reviewer(s) must remain independent from the implementation agent. The manager should stop after a bounded number of correction cycles rather than looping indefinitely.

## Customization layers
- `AGENTS.md`: standing rules shared across agent-oriented workflows.
- `.github/instructions/*.instructions.md`: path-specific C#, XAML, tests, and GIS/Mapsui rules.
- `.github/skills/`: reusable task-specific workflows.
- `.github/prompts/`: reusable user-invoked workflows for implementation and review.
- `.github/agents/`: specialist agent definitions.

Do not duplicate long task procedures in always-on instructions when a skill or prompt is sufficient.

## Git safety
AI agents may edit and verify the working tree, but must not publish work automatically. `git commit`, `git push`, `git merge`, PR creation, PR merge, and other irreversible Git publication actions are human-controlled unless explicitly changed by a future project rule.
