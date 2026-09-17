# WPF Code Review Skill

Use this skill when independently reviewing a change before it is considered ready for human review.

## Review order
1. Confirm the implementation matches the stated requirement.
2. Inspect the diff for unintended changes.
3. Check architecture and WPF/MVVM conventions.
4. Check error handling, nullability, lifetime/DI behavior, and thread affinity where relevant.
5. Check tests and regression coverage.
6. Check maintainability and unnecessary complexity.
7. Report findings by severity and include the affected file and a concrete recommendation.

## Severity
- Critical: security, data loss, corruption, or severe functional failure.
- High: major requirement failure, crash, or likely regression.
- Medium: meaningful correctness, maintainability, or test coverage issue.
- Low: minor style or readability issue with limited impact.

## Independence
Do not assume the implementation agent's explanation is correct. Verify claims against the repository and the actual diff.

## Completion
Return `READY_FOR_HUMAN_REVIEW` only when no Critical or High findings remain and the available verification evidence supports the change.
Otherwise return `CHANGES_REQUESTED` with the findings that must be addressed first.
