---
applyTo: "**/Map/**/*.cs,**/*Map*.cs,**/*Map*.xaml,**/*Map*.xaml.cs"
---

# GIS / Mapsui 指示

- 既存のMap adapter/controller abstractionを優先する。
- 直接MapControlへ依存するよりIMapControlAdapter/MapControlAdapterなど既存抽象化を使用する。
- coordinate conversion、viewport calculation、layer/feature logic、UI event wiringを可能な範囲で分離する。
- screen pixel、geographic coordinate、projected coordinateを混同しない。
- pan、zoom、selection、viewport updateなど既存のmap interactionを要求がない限り維持する。
- 非UI map business logicをWindowへ直接配置しない。
- coordinate/map-state/adapter/controller behaviorには必要な範囲でfocused testsを追加する。
- network-dependent map testsはnetwork behaviorが要件でない限り避ける。
