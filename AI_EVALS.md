# AI Agent Evaluation

WpfGisLearningでは、Agent設定・モデル・ワークフローの変更前後を固定した開発課題で比較評価する。

## 目的
- 要件達成率
- 正しいAgent経路
- 検証品質
- 修正回数
- 不要なAgent起動
- Git安全性
- 取得可能ならtoken使用量と実行時間

## 用語
- Task: 1つの評価課題
- Trial: 同じTaskの1回の実行
- Outcome: 実行後の実際のリポジトリ状態
- Grader: 成否を判定するチェック
- Transcript: Agentの実行履歴・ツール呼び出し・中間結果

## Capability / Regression
- Capability: 新しい構成で何ができるかを測る。
- Regression: 以前できていたことを壊していないかを測る。

## 初期スイート
`evals/cases/` に12課題を配置する。最初は少数から始め、実際の失敗事例を追加する。

## Grader
まず決定的なチェックを使う。
- outcome
- expected routing
- required verification
- scope
- Git safety
LLM-as-judgeは決定的に判定できない品質だけに限定する。

## 指標
- resolution rate = 成功Trial / 全Trial
- verification pass rate = 必須検証を満たしたTrial / 全Trial
- average correction cycles
- unnecessary Agent invocations
- safety violations
- token usage（取得可能な場合）
- wall-clock duration（取得可能な場合）

## 実行条件
通常の機能開発では実行しない。
Agent設定、モデル、routing、self-improvement、Host機能を変更した場合、または品質劣化を疑う場合に実行する。

## 原則
同じTaskを可能な限り同じリポジトリ状態と条件で比較する。品質改善とtoken削減を片方だけで評価しない。