# Avalonia Port Plan

Cross-platform gain is the main motivation. Since the app controls physical hardware over UDP, Linux/macOS support is marginal. Only worth doing if cross-platform becomes a real requirement.

## What carries over unchanged
- All core/domain logic (`LedCube.Core.Common`, `LedCube.Sdf.Core`, plugins)
- `IHost`/`HostBuilder`, DI, `BackgroundService`, Serilog
- `CommunityToolkit.Mvvm` (fully Avalonia-compatible)
- UDP streamer

## Steps

**1. Theme / window chrome**
Replace MahApps.Metro with `Fluent.Avalonia`. Every window inherits `MetroWindow` — swap to Avalonia `Window` + Fluent theme. Replace all `MahApps.Brushes.*` and `MahApps.Styles.*` resource references. Replace IconPacks with Fluent/Segoe icons.

**2. Dialog system**
All 8 dialogs use WPF `ShowDialog()` / `DialogResult`. Rewrite to Avalonia's `await dialog.ShowAsync()` pattern. The `OpenXxxDialogMessage` / `IRecipient` wiring in `App.xaml.cs` stays, just the show/result mechanics change.

**3. XAML conversion**
31 files. Change namespaces (`winfx` → `avalonia`), fix markup differences. Bulk find/replace for the boilerplate, manual review for anything non-trivial. `IValueConverter` is compatible; remove WPF-only attributes (`Localizability`, `ValueConversion`).

**4. Custom controls**
- `DependencyProperty` → `AvaloniaProperty` (9+ properties across `CubeView2DGrid`, `CubeView2DLed`, `Led`)
- `CheckBox` subclasses → Avalonia `TemplatedControl`
- `ControlTemplate` in `Controls.xaml` → Avalonia control theme

**5. CubeView2DGrid (hardest part)**
552-line code-behind using `VisualTreeHelper.GetDescendantBounds/GetOffset` for drag-selection. No Avalonia equivalent — replace with explicit coordinate tracking (store element bounds manually or use `TranslatePoint`). Good opportunity to refactor the code-behind into proper MVVM.

**6. Threading**
Replace `Application.Current.Dispatcher.Invoke/BeginInvoke` (~6 files) with `Dispatcher.UIThread.InvokeAsync()`.

**7. Log viewer**
`Serilog.Sinks.RichTextBox.Wpf` is WPF-only. Write a simple custom Avalonia sink backed by an `ObservableCollection` and display with an `ItemsControl`.

## Effort
4–8 weeks for someone familiar with both WPF and Avalonia. Steps 1–4 and 6–7 are mechanical. Step 5 (CubeView2DGrid) is the only genuinely tricky part.
