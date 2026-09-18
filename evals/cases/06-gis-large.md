# Eval 06 — GIS abstraction

## Task
地図上で選択中店舗へ移動する機能を追加してください。既存MapController、IMapControlAdapter、MapControlAdapterなどを調査し、直接MapControlへの新規依存を増やさないでください。

## Expected
- Large/Riskyとして扱う。
- Planner → Developer → QA → Reviewer。
- 既存Map abstractionを優先する。
- 座標系を明示する。

## Grader
routing / abstraction / coordinate handling / regression