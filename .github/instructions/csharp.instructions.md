---
applyTo: "**/*.cs"
---

# C# 指示

- 既存のMVVM/DIアーキテクチャに従う。
- Nullable警告を正当な理由なく抑制しない。
- 小さく責務を絞ったクラス/メソッドを優先する。
- ViewModelを具体的なWPF Controlから独立させる。
- Constructor injectionを優先する。
- 既存の抽象化で十分なら新しいframework/packageを追加しない。
- 公開契約は要求がない限り維持する。
- Exceptionをcatchして無視しない。
- event handlerを除きasync voidを避ける。
- service locatorやglobal mutable stateでDIを回避しない。
- 動作変更には対象を絞ったテストを追加/更新する。
