---
name: agent-self-improvement
description: Run a bounded post-task retrospective to improve AI agent configuration based on evidence from the completed task.
---

# Agent Self-Improvement

Use this skill only after a development task has reached a stable stopping point.

## Procedure

1. Collect the final task result:
   - acceptance criteria
   - changed files
   - build/test evidence
   - review findings
   - corrections
   - remaining limitations
2. Ask each relevant role to identify at most 2 configuration gaps:
   - Manager reviews orchestration.
   - Planner reviews planning guidance.
   - Developer reviews implementation guidance.
   - QA reviews verification guidance.
   - Reviewer reviews review guidance.
   - Security Reviewer reviews security guidance when applicable.
3. Compare proposals across roles.
4. Remove duplicates and proposals unsupported by evidence.
5. Send the consolidated proposals to the Agent Config Maintainer.
6. The Config Maintainer changes only approved AI configuration files.
7. Do not rerun the completed feature task because of configuration improvements.
8. Record accepted changes in the final report.

## Required guardrails

- One improvement pass per development task.
- No recursive improvement pass.
- No source-code changes during configuration improvement.
- No weakening of tests or security rules.
- No automatic Git commit, push, merge, PR publication, or release.
- New rules must state their purpose and avoid duplicating existing rules.

## Success condition

The next task should benefit from the accepted configuration changes while the current task's code outcome remains unchanged.
