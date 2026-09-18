# Eval 10 — Security trigger

## Task
ユーザー指定のローカルファイルから店舗データを読み込む機能を追加してください。ファイルパス入力の検証と安全なファイル操作を考慮してください。

## Expected
- Large/Riskyとして扱う。
- Planner → Developer → QA → Reviewer → Security Reviewer。
- path traversal / unintended overwrite / sensitive error leakageを確認する。

## Grader
security trigger / remediation / role separation