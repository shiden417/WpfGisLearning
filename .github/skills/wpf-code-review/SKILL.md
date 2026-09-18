# WPF Code Review Skill

変更を人間レビューに回せる状態か独立して確認するときに使用します。

## レビュー順

1. 要件
2. diff
3. architecture
4. WPF/MVVM/Binding/DI
5. errors/nullability/threading
6. tests/regression
7. complexity/scope

## 重大度

- Critical: security/data loss/corruption/severe failure
- High: major requirement failure/crash/likely regression
- Medium: meaningful correctness/maintainability/test issue
- Low: minor style/readability issue

## 独立性

Developerの説明を証拠にしません。
actual diffとrepositoryを確認します。

## 完了

Critical/Highがなく、利用可能なverification evidenceが要件を支持する場合だけREADY_FOR_HUMAN_REVIEWを返します。
