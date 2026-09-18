# WpfGisLearning Copilot 指示

## プロジェクトの背景
- \`net10.0-windows10.0.26100.0\` を対象とするWPFデスクトップアプリケーション。
- Nullable参照型とimplicit usingsを有効化。
- 主なパッケージはCommunityToolkit.Mvvm、Microsoft.Extensions.Hosting、Mapsui.Wpf、ClosedXML。
- 自動テストは \`WpfGisLearning.Tests\` に配置。
- 新しい依存関係を導入するより、既存のアーキテクチャとパターンを優先する。

## 開発原則
- アプリケーションの動作には一貫してMVVMを使用する。
- 依存性注入の登録とライフタイムを意図的かつ一貫したものにする。
- UI固有のWPF動作に限り、View/code-behindへ置く。
- 必要な場合は、Mapsui固有のWPFコントロールへの結合を既存のマップ抽象化の背後に置く。
- 機能開発中は無関係なリファクタリングを行わない。
- 要件で明示的に変更しない限り既存動作を維持する。
- コンパイラやテストの失敗を診断抑制やテスト弱化で隠さない。

## 検証
必要に応じて以下を実行する。
- \`dotnet build\`
- \`dotnet test\`

## AIチームのワークフロー
重要なタスクでは、カスタムAgentワークフローを優先する。
\`WPF Development Manager\` → \`wpf-planner\` → \`wpf-developer\` → \`wpf-qa\` → \`wpf-reviewer\` → 条件付き \`security-reviewer\` → 必要に応じた修正ループ。

認証、認可、秘密情報、外部入力、ファイルI/O、ネットワーク、永続化、依存関係の変更を伴う場合はSecurity Reviewerを使用する。

Reviewerは実装Agentから独立していなければならない。Managerは無限ループせず、修正サイクルを範囲付きで停止する。

## タスク後の自己改善
タスク結果が安定した後、1回だけ範囲付きの改善パスを行う。
- 各関連Agentは、自分または隣接Agentのガイダンスについて、根拠のある改善案を最大2件まで提示する。
- Process Improverが提案を統合し、重複や根拠のない変更を除外する。
- Reviewerが統合された提案を評価する。
- Agent Config Maintainerが承認された変更だけをAI設定ファイルへ反映する。
- このフェーズではアプリケーションのソースコードを変更しない。
- 改善は次のタスクから有効になり、別の改善サイクルを起動しない。

## カスタマイズの階層
- \`AGENTS.md\`: Agent系ワークフロー共通の常設ルール。
- \`.github/instructions/*.instructions.md\`: C#、XAML、テスト、GIS/Mapsuiのパス固有ルール。
- \`.github/skills/\`: 再利用可能なタスク固有ワークフロー。
- \`.github/prompts/\`: 再利用可能なユーザー起動型ワークフロー。
- \`.github/agents/\`: 専門Agentの定義。

常時読み込まれる指示へ長いタスク手順を重複して記載せず、SkillやPromptで十分な場合はそちらを利用する。

## Gitの安全性
AI Agentは作業ツリーを編集・検証してよいが、自動で公開してはならない。commit、push、merge、Pull Requestの作成/mergeなど、不可逆なGit公開操作は人間が管理する。ただし、プロジェクト所有者がルールを明示的に変更した場合を除く。
