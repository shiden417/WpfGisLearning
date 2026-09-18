# WpfGisLearning Agent Instructions

## プロジェクト

WPF learning project and AI-assisted development sandbox.

## 技術基盤

- .NET 10
- WPF / net10.0-windows10.0.26100.0
- CommunityToolkit.Mvvm
- Microsoft.Extensions.Hosting / DI
- Mapsui.Wpf
- ClosedXML
- xUnit / WpfGisLearning.Tests

## 共通ルール

- 既存MVVM/DI/Binding/Navigation/Map abstractionを優先する。
- 変更は要求された範囲に限定する。
- 無関係なリファクタリングをしない。
- テストを削除/弱化して成功扱いにしない。
- 警告やエラーを隠さない。
- Developerの自己確認だけを独立検証とみなさない。
- セキュリティ上重要な変更は独立Security Reviewを行う。
- AI設定改善はアプリ実装と分離する。

## AI開発

通常の入口は WPF Development Manager です。

Managerはタスク規模に応じて必要なAgentだけを起動します。

- Trivial: 最小対応
- Small: Developer → QA
- Medium: Planner → Developer → QA → Reviewer
- Large/Risky: Planner → Developer → QA → Reviewer → 必要時Security Reviewer

問題がある場合の修正ループは最大3回です。

## AI自己改善

自己改善は常に実行するわけではありません。

再発・新規重要欠陥・引き継ぎ問題・新しい設計知見・無駄なAgent呼び出しなど、次回に効く学習がある場合だけManagerが起動します。

実行時は、
Process Improver → Reviewer → Config Maintainer
の1回だけです。

## Git安全性

AIは確認、編集、build、testを行ってよいが、自動公開しない。

人間が最終的に commit / push / PR / merge を判断します。
