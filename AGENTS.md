# WpfGisLearning Agent Instructions

## Project purpose
This repository is a WPF learning project and a sandbox for experimenting with AI-assisted development workflows.

## Technology baseline
- .NET 10 desktop application.
- WPF with `net10.0-windows10.0.26100.0`.
- Nullable reference types and implicit usings are enabled.
- CommunityToolkit.Mvvm.
- Microsoft.Extensions.Hosting / dependency injection.
- Mapsui.Wpf.
- ClosedXML.
- xUnit-based tests are kept in `WpfGisLearning.Tests`.

## Architecture
- Prefer MVVM for application behavior.
- Prefer dependency injection for services and ViewModel dependencies.
- Keep UI-specific WPF behavior in Views/code-behind only when it cannot reasonably belong elsewhere.
- Prefer existing abstractions and patterns over introducing new frameworks or dependencies.
- Preserve existing behavior unless the task explicitly changes it.
- Avoid unrelated refactoring during feature work.

## Development workflow
For a non-trivial task:
1. Inspect the existing implementation and tests before changing code.
2. Make a concise implementation plan.
3. Implement the smallest coherent change.
4. Run `dotnet build` and `dotnet test` where applicable.
5. Review the diff for unintended changes.
6. Perform an independent review before declaring the task complete.
7. After the task is stable, run one bounded AI process-improvement pass.

## AI team workflow
Preferred role separation:
- Manager: coordinates the workflow and correction loop.
- Planner: investigates the repository and proposes the implementation plan.
- Developer: changes production and test code.
- QA: verifies behavior, tests, build, and regression risk.
- Reviewer: independently reviews the changes and reports issues.
- Security Reviewer: inspect security-sensitive changes when authentication, authorization, file I/O, external input, secrets, networking, or data persistence are involved.
- Process Improver: analyzes completed work and proposes evidence-based improvements to AI configuration.
- Agent Config Maintainer: applies only approved configuration improvements; it must not modify application code.

Do not treat the Developer's own conclusion that code is correct as sufficient evidence. QA and Reviewer must independently inspect the result.

Limit automated correction cycles to a small bounded number. If repeated attempts do not converge, stop and report the remaining problem instead of looping indefinitely.

## AI configuration self-improvement
After a task reaches a stable result:
1. Each relevant Agent performs a brief retrospective of its own guidance and the handoff with neighboring roles.
2. The Process Improver consolidates evidence-based proposals.
3. The Reviewer checks the proposals for duplication, contradictions, unsafe changes, and unnecessary complexity.
4. The Agent Config Maintainer applies only accepted configuration changes.
5. The improvement pass runs at most once and never recursively triggers itself.
6. Configuration changes apply to subsequent development tasks, not the current task.

Configuration improvement must never weaken verification, security controls, role separation, bounded loops, or human Git publication controls.

## Verification rules
- Never remove or weaken tests merely to make them pass.
- Do not suppress compiler warnings or diagnostics to hide defects unless the suppression is explicitly justified and documented.
- A successful build alone does not prove the requirement is satisfied.
- When a test cannot be run because of an environment limitation, report that limitation clearly.
- AI configuration changes must remain separate from application behavior changes.

## Git safety
AI agents may inspect, edit, build, and test the working tree.

Do not automatically:
- push to a remote
- merge branches
- merge pull requests
- delete branches
- publish releases
- modify repository settings

The human owner performs the final review and decides whether to commit/push or create/merge a pull request.
