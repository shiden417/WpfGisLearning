# Experimental AI Agent Workflow

このリポジトリでは、GitHub Copilot Custom Agents を使ったAI開発チームを試験運用します。

## Agent構成

```text
User
  |
  v
wpf-development-manager
  |
  +--> wpf-planner
  |      |
  |      +--> 要件整理・既存コード調査・実装計画
  |
  +--> wpf-developer
  |      |
  |      +--> 実装・テスト追加・ビルド
  |
  +--> wpf-qa
  |      |
  |      +--> Build / Test / Regression check
  |
  +--> wpf-reviewer
         |
         +--> 独立レビュー
                |
                +--> 問題あり: Developerへ戻す
                +--> 問題なし: 完了
```

## 目的

AIに単発でコードを書かせるのではなく、Plan → Do → Check → Act のサイクルをAI側で回し、最後の公開判断だけを人間が行う構成を検証する。

## 基本ルール

1. Managerがタスク全体を管理する。
2. Plannerは計画のみを担当する。
3. Developerが実装する。
4. QAがBuild/Testを確認する。
5. ReviewerがDeveloperとは独立した視点でレビューする。
6. 問題があればDeveloper → QA → Reviewerを繰り返す。
7. 無限ループを防ぐため、通常は最大3回の修正サイクルとする。
8. AIはcommit / push / merge / PR公開を行わず、人間が最終確認してGit操作する。

## 開発開始時の例

CopilotのAgentモードで `wpf-development-manager` を選び、次のような依頼を行う。

> Shop一覧に検索機能を追加してください。既存のMVVM/DI構成を維持し、テストも追加してください。要件分析から実装、テスト、レビュー、必要な修正までチームとして進め、最後に変更内容と検証結果を報告してください。

このファイルは実験用です。実プロジェクトへ移行する際は、会社のCopilot利用ルール、MCP利用可否、PR/ブランチ運用、レビュー基準に合わせて見直してください。
