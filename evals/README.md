# AI Agent Evaluation Suite

WpfGisLearningのAI開発チームを比較評価する初期12課題。

| ID | 種類 | 主な評価 |
|---|---|---|
| 01 | Trivial | routing |
| 02 | Small | routing / QA |
| 03 | Medium | planning / MVVM / Binding |
| 04 | Medium | Command / state |
| 05 | Large | DI / architecture |
| 06 | Large | GIS / abstraction |
| 07 | Verification | test / evidence |
| 08 | Review | independence |
| 09 | Scope | unrelated change avoidance |
| 10 | Security | conditional review |
| 11 | Self-improvement | skip unnecessary improvement |
| 12 | Self-improvement | trigger behavior |

各Taskには入力、期待経路、成否条件、目的を記載する。

共通graderは outcome / routing / verification / scope / Git safety。