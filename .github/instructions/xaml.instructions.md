---
applyTo: "**/*.xaml"
---

# WPF XAML 指示

- Application logicはBinding/CommandとViewModelへ寄せる。
- 既存DataContext conventionsを維持する。
- Grid/StackPanel/ContentControlなど既存の明確な構造を優先する。
- 固定resourceはStaticResource、runtime replacementが必要な場合だけDynamicResource。
- 既存ResourceDictionaryを優先する。
- Binding mode/update triggerが曖昧なら明示する。
- 表示値の変更ではDataContext、property path、formatting/conversion、target、update mechanismを確認する。
- 不要なElementName/code-behind couplingを避ける。
- interactive controlsのkeyboard/accessibilityを考慮する。
- 無関係なvisual behaviorを変更しない。
- TemplateやResourceDictionaryの変更は利用箇所への影響を確認する。
