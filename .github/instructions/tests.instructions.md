---
applyTo: "**/*Tests.cs,**/*.Tests/**/*.cs,**/WpfGisLearning.Tests/**/*.cs"
---

# テスト指示

- 既存test frameworkとconventionsを使用する。
- Arrange / Act / Assertを基本とする。
- implementation detailではなくobservable behaviorを確認する。
- bug fixにはregression testを追加する。
- success/failure/edge caseを必要な範囲で確認する。
- networkやmachine-specific stateへの依存を避ける。
- assertionを弱めたりtestを削除/skipして成功扱いにしない。
- production behaviorが変わったら既存testの妥当性を再確認する。
- bound filtered/count stateでは、該当する場合にinitial、change、empty、clear/reset、PropertyChangedを確認する。
