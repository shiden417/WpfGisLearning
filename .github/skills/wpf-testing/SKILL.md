# WPF Testing Skill

このSkillはWpfGisLearningのテストと検証で使用します。

## 原則

最も安い十分な検証を選びます。
不要なfull-suite実行や重複確認を避けます。

## 検証レベル

### Trivial
必要な静的確認のみ。

### Small
関連テストを優先し、XAML/project変更など必要な場合だけbuildを追加。

### Medium以上
dotnet build
dotnet test

### 追加条件
- 依存関係変更: restore
- migration/persistence: 専用確認
- UI固有: 手動確認項目を明示
- Binding/derived state: initial/change/empty/reset/PropertyChangedを必要な範囲で確認

## QAの立場

QAは初回確認でproduction code/test codeを編集しません。
不足はDeveloperへ具体的な再現条件と期待結果を返します。

## 完了条件

- 受け入れ条件に対する証拠がある
- 必須build/testが成功、または環境制約が明示されている
- 既存testを弱めていない
