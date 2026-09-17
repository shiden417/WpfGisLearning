---
applyTo: "**/Map/**/*.cs,**/*Map*.cs,**/*Map*.xaml,**/*Map*.xaml.cs"
---
# GIS / Mapsui instructions

- Keep Mapsui-specific UI/control dependencies isolated behind existing adapters or controllers where the repository already provides them.
- Prefer the existing `IMapControlAdapter`, `MapControlAdapter`, and map controller abstractions before introducing direct `MapControl` dependencies.
- Keep coordinate conversion, viewport calculations, layer/feature logic, and UI event wiring separated when practical.
- Be explicit about coordinate systems and conversions; do not assume screen pixels, geographic coordinates, and projected coordinates are interchangeable.
- Preserve map interaction behavior such as pan, zoom, selection, and viewport updates unless the requirement changes it.
- Avoid putting non-UI map business logic directly into a WPF window when an existing map service/controller abstraction can own it.
- Add focused tests for coordinate calculations, map-state decisions, and adapter/controller behavior when practical.
- Avoid network-dependent map tests unless the requirement explicitly concerns network behavior.
