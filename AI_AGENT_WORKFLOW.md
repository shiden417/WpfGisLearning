# Experimental AI Agent Workflow

このリポジトリでは、GitHub Copilot Custom Agents と関連するカスタマイズ機能を使ったAI開発チームを試験運用します。

## Agent構成

```text
User
  |
  v
WPF Development Manager
  |
  +--> wpf-planner
  |      +--> 要件整理・既存コード調査・実装計画
  |
  +--> wpf-developer
  |      +--> 実装・テスト追加・ビルド
  |
  +--> wpf-qa
  |      +--> Build / Test / Regression check
  |
  +--> wpf-reviewer
  |      +--> 独立レビュー
  |
  +--> security-reviewer (条件付き)
         +--> Security-sensitive change review
  |
  +--> 問題あり
         +--> Developer → QA → Review に戻る

問題なし
  |
v
Human final review
  |
v
Human decides commit / push / PR / merge
```

## Repository customization layers

- `AGENTS.md`: AIエージェント共通の常設ルール。
- `.github/copilot-instructions.md`: Copilot向けのリポジトリ共通ルール。
- `.github/instructions/`: C#、XAML、テスト、GIS/Mapsuiのパス固有ルール。
- `.github/agents/`: 専門Agentの定義。
- `.github/skills/`: 必要なときだけ読み込むWPFテスト・コードレビューの定型手順。
- `.github/prompts/`: `/implement-feature` や `/review-change` として再利用する作業テンプレート。

## 目的

AIに単発でコードを書かせるのではなく、Plan → Do → Check → Act のサイクルをAI側で回し、最後の公開判断だけを人間が行う構成を検証する。

## 基本ルール

1. Managerがタスク全体を管理する。
2. Plannerは計画のみを担当する。
3. Developerが実装する。
4. QAがBuild/Testを確認する。
5. ReviewerがDeveloperとは独立した視点でレビューする。
6. セキュリティに関係する変更ではSecurity Reviewerも実行する。
7. 問題があればDeveloper → QA → Reviewを繰り返す。
8. 無限ループを防ぐため、通常は最大3回の修正サイクルとする。
9. AIはcommit / push / merge / PR公開を行わず、人間が最終確認してGit操作する。

## 開発開始時の例

CopilotのAgentモードで Manager を選ぶか、`/implement-feature` プロンプトを呼び出し、次のような依頼を行う。

> Shop一覧に検索機能を追加してください。既存のMVVM/DI構成を維持し、テストも追加してください。要件分析から実装、テスト、レビュー、必要な修正までチームとして進め、最後に変更内容と検証結果を報告してください。

## レビューだけ行う場合

`/review-change` を使い、現在の変更に対してQA、独立コードレビュー、必要に応じたSecurity Reviewを行う。

## 注意事項

- Agent-to-agentの完全自動オーケストレーションは、利用するCopilotホスト・プラン・設定によって対応範囲が異なる。
- Prompt filesは現在public previewのため、将来仕様が変更される可能性がある。
- Skillsは対応するCopilot環境でのみ自動利用される。Visual Studioで利用できない場合でも、AgentやPromptから手順を実行する構成を維持する。
- MCPやHooksは有用だが、会社の利用規約や環境依存が大きいため、この実験ではまだ導入しない。

このファイルは実験用です。実プロジェクトへ移行する際は、会社のCopilot利用ルール、MCP利用可否、PR/ブランチ運用、レビュー基準に合わせて見直してください。
