---
applyTo: "**/*.xaml"
---
# WPF XAML instructions

- Use Binding and commands rather than embedding application logic in XAML/code-behind.
- Follow the existing MVVM structure and DataContext conventions.
- Keep layout readable and prefer clear Grid/StackPanel/ContentControl structure over unnecessary nesting.
- Use StaticResource for fixed resources and DynamicResource only when runtime resource replacement is required.
- Prefer existing ResourceDictionary resources before adding duplicate styles/templates.
- Make binding modes and update triggers explicit when the default behavior would be unclear.
- Avoid unnecessary element-name or code-behind coupling.
- Keep accessibility and keyboard navigation in mind for interactive controls.
- Do not change visual behavior unrelated to the requested feature.
- When modifying a ControlTemplate, DataTemplate, navigation surface, or resource dictionary, check for impact on all consumers.
