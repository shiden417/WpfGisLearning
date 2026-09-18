---
name: agent-self-improvement
description: 完了タスクから次回に効くAI設定改善だけを1回だけ抽出・審査・適用する。
user-invocable: false
disable-model-invocation: false
---

# Agent 自己改善

## 発動条件

Managerが次のいずれかを認識した場合だけ使用します。

- Large/Riskyタスクが完了した
- Reviewerが新規重要欠陥を発見した
- correction loopが発生した
- 同じ問題が再発した
- Agent handoffの問題が発生した
- 新しいarchitecture/conventionが確立した
- 不要なAgent呼び出しや検証が発生した
- manual verificationの不足が繰り返し発生した

単純に「タスクが終わった」だけでは発動しません。

## 手順

1. 完了タスクの要件、変更、検証、レビュー、修正を集める。
2. 関係Agentから最大2件ずつ改善候補を集める。
3. Process Improverが重複、推測、不要なtoken消費を除外する。
4. Reviewerが独立して承認/却下/保留を判断する。
5. Config Maintainerが承認済みAI設定だけを最小変更で適用する。
6. 現在の機能タスクを再実行しない。

## 成功条件

次回タスクの品質または効率が改善され、現在のアプリケーション結果は変わらないこと。

## ガードレール

- 1タスク1回
- recursive improvement禁止
- application source/test変更禁止
- security/verification/Git human controlsを弱めない
- 文章の言い換えだけでは設定を変更しない
