# WpfGisLearning Copilot 指示

このリポジトリでは WPF Development Manager を通常の開発入口とします。

## 優先事項

1. 既存アーキテクチャを調査する
2. タスク規模に応じて最小のAgent構成を選ぶ
3. 変更範囲を最小化する
4. 独立検証を必要に応じて行う
5. Security Reviewは条件付き
6. 自己改善は学習価値がある場合だけ

## 標準ワークフロー

Trivial:
最小対応

Small:
Developer → QA

Medium:
Planner → Developer → QA → Reviewer

Large/Risky:
Planner → Developer → QA → Reviewer → 必要時Security Reviewer

修正:
Developer → QA → Reviewer を最大3回

## 共通

- MVVM/DI/Bindingを維持する
- WPF固有動作は必要な場合だけView/code-behindに置く
- Mapsui依存は既存抽象化を優先する
- テストを弱めない
- Git公開操作を自動化しない

詳細な役割と検証手順は各Agent/Skillを参照します。
