# AI Agent Self-Improvement Policy

WpfGisLearning uses a bounded post-task feedback loop to improve its AI development configuration.

## Improvement cycle

```
Development task
  ↓
Planner → Developer → QA → Reviewer
  ↓
Stable result
  ↓
Each relevant Agent retrospective
  ↓
Cross-agent comparison
  ↓
AI Process Improver
  ↓
Agent Config Maintainer
  ↓
Configuration changes
  ↓
Next development task uses the updated rules
```

## Purpose

The goal is to improve the development process from actual evidence rather than allowing Agents to rewrite their own instructions arbitrarily.

Examples of useful feedback:
- A recurring missed edge case indicates a missing review rule.
- A repeated handoff problem indicates a missing Manager/Planner/Developer contract.
- Repeated test omissions indicate a QA instruction gap.
- A reviewer finding repeatedly missed by Developer indicates a useful new implementation rule.
- Duplicate or contradictory instructions indicate configuration cleanup is needed.

## Rules

1. Each relevant Agent may propose configuration improvements after a completed task.
2. Proposals must be evidence-based and limited in scope.
3. The Agents review both their own role and relevant neighboring roles.
4. The Process Improver consolidates cross-agent proposals.
5. The Agent Config Maintainer applies only approved changes.
6. There is at most one improvement pass per development task.
7. Improvement does not recursively trigger another improvement pass.
8. Configuration changes affect subsequent tasks only.
9. Application source code and tests are out of scope for this improvement phase.
10. Git commit, push, merge, PR publication, and release remain human-controlled.

## Human role

The human owner remains the final authority. Configuration improvements are intended to reduce repeated mistakes, not to replace human judgment about architecture, requirements, security, or project policy.
