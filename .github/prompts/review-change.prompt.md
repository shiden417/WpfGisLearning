---
agent: 'agent'
description: '現在の変更を最小コストでQA・独立レビューし、必要時だけSecurity Reviewと自己改善を行う'
---

現在のWpfGisLearningの変更をAIチームとしてレビューしてください。

1. diffと要件を確認する。
2. タスク規模に応じて必要な検証を行う。
3. WPF/MVVM/Binding/DI/テストを独立レビューする。
4. Security条件に該当する場合だけSecurity Reviewerを使用する。
5. 問題があれば重大度・根拠・対象・修正条件を報告する。
6. レビュー段階ではアプリコードとtest codeを変更しない。
7. 安定後、学習価値がある場合だけ自己改善Skillを1回実行する。
8. Git公開操作を行わない。

問題なしならREADY_FOR_HUMAN_REVIEWを返してください。
