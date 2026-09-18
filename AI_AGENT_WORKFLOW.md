# 実験用AI Agentワークフロー

このリポジトリでは、GitHub Copilot Custom Agentsと関連するカスタマイズ機能を使ったAI開発チームを試験運用します。

## Agent構成

\`\`\`
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
  |      +--> ビルド・テスト・回帰確認
  |
  +--> wpf-reviewer
  |      +--> 独立レビュー
  |
  +--> security-reviewer (条件付き)
  |      +--> セキュリティに関係する変更のレビュー
  |
  +--> process-improvement
  |      +--> 完了タスクのAI設定ギャップ分析
  |
  +--> agent-config-maintainer
         +--> 承認済みAI設定の反映

問題あり
  +--> Developer → QA → Review に戻る

安定完了
  |
  v
各Agentの振り返り
  |
  v
Process Improver
  |
  v
Reviewer
  |
  v
Config Maintainer
  |
  v
次回タスクから改善設定を使用

人間による最終レビュー
  |
  v
人間がcommit / push / Pull Request / mergeを判断
\`\`\`

## リポジトリのカスタマイズ階層

- \`AGENTS.md\`: AI Agent共通の常設ルール。
- \`.github/copilot-instructions.md\`: Copilot向けのリポジトリ共通ルール。
- \`.github/instructions/\`: C#、XAML、テスト、GIS/Mapsuiのパス固有ルール。
- \`.github/agents/\`: 専門Agentの定義。
- \`.github/skills/\`: 必要なときだけ読み込むWPFテスト、コードレビュー、AI自己改善の定型手順。
- \`.github/prompts/\`: \`/implement-feature\` や \`/review-change\` として再利用する作業テンプレート。

## 目的

AIに単発でコードを書かせるのではなく、Plan → Do → Check → Act → Improve のサイクルをAI側で回し、最後の公開判断だけを人間が行う構成を検証する。

## AI自己改善ポリシー

1. 各関連Agentは安定完了後に、自分の設定と隣接Agentとの受け渡しを短く振り返る。
2. 各Agentは最大2件まで、実際のタスク結果に根拠がある改善案を出す。
3. Process Improverが重複・矛盾・根拠不足の案を整理する。
4. Reviewerが改善案を独立評価する。
5. Config Maintainerが承認済みのAI設定だけを反映する。
6. 1タスクにつき改善パスは1回だけ。
7. 改善パスから別の改善パスを再帰的に起動しない。
8. 設定改善は現在のタスクには遡及せず、次のタスクから有効になる。
9. アプリ本体コード・テストは自己改善フェーズの対象外。
10. commit / push / merge / Pull Request公開は人間が管理する。

## 開発開始時の例

CopilotのAgentモードでManagerを選ぶか、\`/implement-feature\` プロンプトを呼び出し、次のような依頼を行う。

> Shop一覧に検索機能を追加してください。既存のMVVM/DI構成を維持し、テストも追加してください。要件分析から実装、テスト、レビュー、必要な修正、最後にAI設定の振り返りまでチームとして進め、変更内容と検証結果を報告してください。

## レビューだけ行う場合

\`/review-change\` を使い、現在の変更に対してQA、独立コードレビュー、必要に応じたSecurity Reviewを行う。

## 注意事項

- Agent間の完全自動オーケストレーションは、利用するCopilotホスト・プラン・設定によって対応範囲が異なる。
- Prompt filesは現在public previewのため、将来仕様が変更される可能性がある。
- Skillsは対応するCopilot環境でのみ自動利用される。利用できない場合でも、同じ手順をAgentやPromptから実行する構成を維持する。
- MCPやHooksは有用だが、会社の利用規約や環境依存が大きいため、この実験ではまだ導入しない。

このファイルは実験用です。実プロジェクトへ移行する際は、会社のCopilot利用ルール、MCP利用可否、PR/ブランチ運用、レビュー基準に合わせて見直してください。
