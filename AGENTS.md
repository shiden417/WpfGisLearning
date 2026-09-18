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
- Security条件に該当する変更は独立Security Reviewを行う。
- AI設定改善はアプリ実装と分離する。

## AI開発

通常の入口は WPF Development Manager。

- Trivial: 最小対応
- Small: Developer → QA
- Medium: Planner → Developer → QA → Reviewer
- Large/Risky: Planner → Developer → QA → Reviewer → 必要時Security Reviewer

修正ループは最大3回。
自己改善は学習価値がある場合だけ1回。

## AI Agent評価

Agent設定、routing、model、Host機能などを変更した場合は、必要に応じて evals/cases の固定Taskで変更前後を比較する。
通常の機能開発では評価スイートを実行しない。

主要指標はresolution rate、verification quality、correction cycles、不要なAgent起動、safety。
取得可能ならtoken/latencyも比較する。

## Git安全性

AIは確認、編集、build、testを行ってよいが、自動公開しない。
人間が最終的に commit / push / PR / merge を判断する。
