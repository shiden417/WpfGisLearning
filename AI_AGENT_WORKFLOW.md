# WpfGisLearning AI Agent Workflow

## 目的

AIに単発でコードを書かせるのではなく、タスクの大きさに応じて必要最小限の役割だけを使い、実装・検証・独立レビュー・必要時の自己改善を自動化する。

GitHubのAI Team Orchestrationの考え方に合わせ、軽い作業には軽いプロセス、重要な作業には厚いプロセスを使用する。

## 基本構成

~~~
User
  |
  v
WPF Development Manager
  |
  +-- Trivial -> 最小対応
  |
  +-- Small -> Developer -> QA
  |
  +-- Medium -> Planner -> Developer -> QA -> Reviewer
  |
  +-- Large/Risky -> Planner -> Developer -> QA -> Reviewer
  |                                 |
  |                                 +-> Security Reviewer (条件時)
  |
  +-- 学習価値あり -> Process Improver -> Reviewer -> Config Maintainer
~~~

## 役割

- Manager: 分類、委任、停止条件、結果統合。通常実装しない。
- Planner: Medium以上の調査と計画。
- Developer: 実装と自己検証。
- QA: 独立検証。初回QAでは修正しない。
- Reviewer: 独立レビュー。
- Security Reviewer: セキュリティ条件時のみ。
- Process Improver: 次回に効く設定改善の抽出。
- Agent Config Maintainer: 承認済み設定だけを適用。

## 検証

最も安い十分な検証を選ぶ。

Smallでは関連テスト中心。
Medium以上ではbuild/test。
UI固有事項は手動確認として残す。

## 修正

Developer -> QA -> Reviewerを最大3回。
収束しない場合は停止して報告。

## 自己改善

毎回は実行しない。
再発、重要な新規指摘、handoff問題、新しい設計知見、無駄な実行がある場合に起動する。

## 永続的な知識

タスクログを毎回大量に作らない。
将来のアーキテクチャや規約に影響する判断だけAI_DECISIONS.mdへ残す。

## Git

AIはcommit/push/merge/PR公開を自動化しない。
最終公開判断は人間が行う。

## ホスト差

Agent間の自動委任やCustom Agentの一部機能はホストにより対応が異なる。
機能が使えない場合は同じ役割順を手動で実行し、報告では実際に実行した内容だけを記載する。
