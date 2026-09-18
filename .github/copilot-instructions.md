# WpfGisLearning Copilot 指示

通常の開発入口は WPF Development Manager です。

## 開発原則

- 既存MVVM/DI/Binding/Navigationを維持する。
- 変更範囲を最小化する。
- 無関係なrefactorをしない。
- テストを弱めない。
- Git公開操作を自動化しない。

## タスク経路

Trivial:
最小対応

Small:
Developer → QA

Medium:
Planner → Developer → QA → Reviewer

Large/Risky:
Planner → Developer → QA → Reviewer → 条件時Security Reviewer

問題があればDeveloper → QA → Reviewerを最大3回。

## 自己改善

学習価値がある場合だけ1回実行します。

## Agent評価

Agent定義、routing、model、Host機能などを変更した場合、agent-evaluation Skillを使って固定Taskの比較評価を行います。
通常の機能開発では評価を実行しません。

詳細な役割・検証は各Agent/Skillを参照します。
