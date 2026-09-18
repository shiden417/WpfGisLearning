# WpfGisLearning AI Agent Workflow

## 目的

タスク規模に応じて必要最小限のAgentを使い、実装・検証・レビュー・必要時の自己改善を自動化する。

## 基本構成

~~~
User
  |
  v
WPF Development Manager
  |
  +-- Trivial -> 最小対応
  +-- Small -> Developer -> QA
  +-- Medium -> Planner -> Developer -> QA -> Reviewer
  +-- Large/Risky -> Planner -> Developer -> QA -> Reviewer
  |                                 |
  |                                 +-> Security Reviewer (条件時)
  |
  +-- 学習価値あり -> Process Improver -> Reviewer -> Config Maintainer
  |
  +-- Agent設定等の変更 -> Agent Evaluation -> 比較結果
~~~

## Agentの役割

- Manager: 分類、委任、停止条件、結果統合
- Planner: Medium以上の調査と計画
- Developer: 実装と自己検証
- QA: 独立検証
- Reviewer: 独立レビュー
- Security Reviewer: セキュリティ条件時のみ
- Process Improver: 次回に効く設定改善
- Agent Config Maintainer: 承認済み設定だけを適用

## 検証

Smallでは関連テスト中心。
Medium以上ではbuild/test。
UI固有事項は手動確認として明示。

## Agent Evaluation

Agent設定、routing、model、Host機能を変更した場合に固定Eval Taskを実行する。
同じ条件で複数Trialを行い、resolution rateだけでなくverification、correction、不要なAgent起動、safety、取得可能ならtoken/latencyも比較する。

Capability Taskが安定したらRegression Taskとして継続利用する。

## 自己改善

毎回は実行しない。
再発、新規重要指摘、handoff問題、新しい設計知見、不要な実行などがある場合に起動する。

## Git

AIはcommit/push/merge/PR公開を自動化しない。
最終判断は人間が行う。
