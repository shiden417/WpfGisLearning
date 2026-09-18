---
name: ai-task-routing
description: WpfGisLearningのタスク規模とリスクを判定し、必要最小限のAgentワークフローを選択する。
user-invocable: false
disable-model-invocation: false
---

# AI Task Routing

このSkillはManagerがタスク経路を決めるときに使用します。

## 分類

### Trivial
Markdown、文言、コメント、単純な設定など。設計判断や動作変更がない。

### Small
既存の設計内で完結する小変更。通常1〜数ファイル。
例: 既存プロパティ表示、軽いXAML調整、単純な既存ロジック修正。

### Medium
複数レイヤー、Binding/Command/状態管理、新規テスト、画面機能など。

### Large/Risky
Architecture、GIS、Persistence、File I/O、Network/API、Auth、Secrets、Dependencies、Migrations、Cross-cutting refactor。

## 経路

Trivial:
最小対応

Small:
Developer → QA

Medium:
Planner → Developer → QA → Reviewer

Large/Risky:
Planner → Developer → QA → Reviewer → Security Reviewer (条件該当時)

## 例外

分類に確信がない、または影響範囲が見えない場合は1段階上げます。

自己改善は別判定です。
学習価値がなければ実行しません。

## 出力

Managerは最終報告で、
- 分類
- 呼び出したAgent
- 省略したAgentと理由
を明示します。
