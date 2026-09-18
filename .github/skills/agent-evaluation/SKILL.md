---
name: agent-evaluation
description: Agent設定・モデル・ワークフローの変更を固定した開発課題で比較評価する。
user-invocable: false
disable-model-invocation: false
---

# Agent Evaluation Skill

## 起動条件
- Agent定義を変更した
- routing/orchestrationを変更した
- self-improvementを変更した
- 使用モデルを変更した
- HostのAgent機能を変更した
- 重要な品質回帰を疑う

## 手順
1. 評価対象の構成とrepository commitを固定する。
2. evals/casesから対象Taskを選ぶ。
3. 入力、期待結果、graderを確認する。
4. 同じ条件でTrialを実行する。
5. outcomeと実行履歴を記録する。
6. deterministic graderを先に実施する。
7. 必要な場合だけhuman/LLM judgeを使う。
8. baselineと比較する。
9. capability改善とregressionを両方確認する。

## 禁止事項
- Agentが成功と言っただけで成功扱いしない。
- token削減だけで品質低下を見逃さない。
- Agent数増加だけを改善とみなさない。
- 曖昧なTaskやgraderで失敗をAgentの能力不足と決めない。

## 出力
suite, task, trial, config, model, outcome, routing, verification, corrections, safety, token, time, regression, findingsを報告する。