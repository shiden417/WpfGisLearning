---
agent: WPF Development Manager
description: Run the independent QA, code review, and conditional security review workflow for existing changes.
---

Review the current working tree as an AI development team.

1. Inspect the current diff and identify the intended requirement from the user's task/context.
2. Run QA checks appropriate to the repository, including `dotnet build` and `dotnet test` when applicable.
3. Perform an independent WPF/MVVM/code-quality review.
4. Run the Security Reviewer when changes involve authentication, authorization, secrets, external input, file I/O, networking, persistence, or dependency changes.
5. Report findings by severity with file references and concrete remediation.
6. Do not modify files during the review phase. Report `READY_FOR_HUMAN_REVIEW` only when no Critical/High issue remains and verification evidence is sufficient.

Do not commit, push, merge, create a PR, or modify repository settings.
