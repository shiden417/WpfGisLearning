---
applyTo: "**/*.xaml"
---
# WPF XAML 指示

- XAML/code-behindにアプリケーションロジックを埋め込むのではなく、BindingとCommandを使用する。
- 既存のMVVM構成とDataContextの慣例に従う。
- レイアウトを読みやすく保ち、不要なネストよりも明確なGrid/StackPanel/ContentControl構造を優先する。
- 固定リソースにはStaticResourceを使用し、実行時にリソースを差し替える必要がある場合だけDynamicResourceを使用する。
- 重複したStyle/Templateを追加する前に、既存のResourceDictionaryリソースを優先する。
- デフォルト動作が不明確になる場合は、Bindingモードと更新トリガーを明示する。
- 表示状態のためにViewModelやサービスコードを変更する前に、DataContext、プロパティパス、書式/変換、Target Property、更新機構を確認し、十分なBindingまたはUI検証を最小コストで選択する。
- 不要なElementName参照やcode-behindへの結合を避ける。
- インタラクティブなコントロールではアクセシビリティとキーボードナビゲーションを考慮する。
- 要求された機能と無関係な表示動作を変更しない。
- ControlTemplate、DataTemplate、ナビゲーション領域、ResourceDictionaryを変更する場合は、すべての利用箇所への影響を確認する。
