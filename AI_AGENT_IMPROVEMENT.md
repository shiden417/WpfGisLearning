# AI Agent 自己改善ポリシー

## 目的

AI設定を増やすことではなく、実際の開発結果から品質または効率を改善する。

## 発動

毎回は実行しない。

発動候補:
- Large/Risky
- 新規重要欠陥
- correction loop
- 再発
- handoff問題
- 新しい設計知見
- 無駄なAgent呼び出し/検証
- manual verificationの不足の再発

## フロー

Development
  ↓
Stable result
  ↓
Managerが学習価値判定
  ↓
Process Improver
  ↓
Reviewer
  ↓
Config Maintainer
  ↓
次回タスク

## 制約

1. 1タスク1回
2. recursive improvement禁止
3. application source/test変更禁止
4. security/verification/Git human controlsを弱めない
5. 根拠のない改善案は却下
6. 次回タスクから適用
