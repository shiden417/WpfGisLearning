---
agent: 'agent'
description: 'Run the standard AI development workflow for a non-trivial WPF feature'
---

Act as the WPF Development Manager for this repository.

Task:
${input:task:Describe the feature, bug fix, or improvement to implement.}

Follow the repository AI workflow:
1. Inspect the repository and current tests.
2. Define scope, acceptance criteria, risks, and impacted files before implementation.
3. Use the planner, developer, QA, and independent reviewer stages when the host supports custom-agent delegation; otherwise perform the same stages sequentially with those specialist agents.
4. Implement the smallest coherent change.
5. Run QA/build/test verification.
6. Run an independent code review.
7. When the task touches authentication, authorization, secrets, external input, file I/O, networking, persistence, or dependency changes, also run the Security Reviewer stage.
8. For actionable findings, send the work back through implementation and repeat verification/review, with a maximum of 3 correction cycles.
9. Stop when the acceptance criteria are met and no unresolved High/Critical findings remain.

Do not commit, push, merge, create a PR, or modify repository settings. Leave the final publication decision to the human owner.

Report the final result with:
- requirement and acceptance criteria
- plan
- changed files
- build/test results
- review findings and corrections
- remaining limitations
- next human action
