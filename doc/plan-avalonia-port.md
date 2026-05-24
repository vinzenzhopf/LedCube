# Avalonia Port Plan

Cross-platform gain is the main motivation. Since the app controls physical hardware over UDP, Linux/macOS support is marginal. Only worth doing if cross-platform becomes a real requirement.

**Lighter alternative:** If the only goal is fixing MahApps bugs (e.g. window drag), replace MahApps with [`wpf-ui`](https://github.com/lepoco/wpfui) — same WPF stack, Fluent Design chrome, similar attached-property patterns. ~2–4 days vs weeks for a full Avalonia port.

## What carries over unchanged
- All core/domain logic (`LedCube.Core.Common`, `LedCube.Sdf.Core`, plugins)
- `IHost`/`HostBuilder`, DI, `BackgroundService`, Serilog
- `CommunityToolkit.Mvvm` (fully Avalonia-compatible)
- UDP streamer

## Steps

**1. Theme / window chrome**
4 projects use MahApps. 11 windows inherit `MetroWindow` — swap to Avalonia `Window` + `Fluent.Avalonia` theme. Replace all `MahApps.Brushes.*` and `MahApps.Styles.*` resource references across ~21 XAML files. Replace `PackIconFontAwesome` / `PackIconModern` with Fluent/Segoe icons.

**2. Dialog system**
7 dialogs use WPF `ShowDialog()` / `DialogResult`. Rewrite to Avalonia's `await dialog.ShowAsync()` pattern. The `OpenXxxDialogMessage` / `IRecipient` wiring in `App.xaml.cs` stays; only the show/result mechanics change.

**3. XAML conversion**
~31 files. Change namespaces (`winfx` → `avalonia`), fix markup differences. Bulk find/replace for boilerplate, manual review for non-trivial cases. `IValueConverter` is compatible; remove WPF-only attributes (`Localizability`, `ValueConversion`).

**4. Custom controls**
- `DependencyProperty` → `AvaloniaProperty` (9+ properties across `CubeView2DGrid`, `CubeView2DLed`, `Led`)
- `CheckBox` subclasses → Avalonia `TemplatedControl`
- `ControlTemplate` in `Controls.xaml` → Avalonia control theme
- `mah:MultiSelectorHelper.SelectedItems` in `CubeView2D` — replace with a behavior or explicit selection binding

**5. CubeView2DGrid (hardest part)**
552-line code-behind using `VisualTreeHelper.GetDescendantBounds/GetOffset` for drag-selection. No Avalonia equivalent — replace with explicit coordinate tracking (store element bounds manually or use `TranslatePoint`). Good opportunity to refactor into proper MVVM.

**6. Threading**
Replace `Application.Current.Dispatcher.Invoke/BeginInvoke` (~6 files) with `Dispatcher.UIThread.InvokeAsync()`.

**7. Log viewer**
`Serilog.Sinks.RichTextBox.Wpf` is WPF-only. Write a simple custom Avalonia sink backed by an `ObservableCollection` and display with an `ItemsControl`.

## Effort
4–8 weeks for someone familiar with both WPF and Avalonia. Steps 1–4 and 6–7 are mechanical. Step 5 (CubeView2DGrid) is the only genuinely tricky part.
