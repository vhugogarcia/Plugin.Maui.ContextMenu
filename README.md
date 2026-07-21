# Plugin.Maui.ContextMenu

`Plugin.Maui.ContextMenu` is a .NET MAUI library that lets you attach a **native context menu** to any view. On iOS and Mac Catalyst the menu is presented using `UIContextMenuInteraction` (with the familiar blurred preview), and on Android it is rendered as a native popup. By default the menu opens on **long press**, and it can optionally be triggered on click.

The menu is declared entirely in XAML using attached properties, supports commands and data binding, and works both on individual views and inside a `CollectionView`.

## Features

- 🧩 Attach a context menu to **any** `VisualElement` via an attached property
- 📱 Native presentation on **iOS**, **Mac Catalyst**, and **Android**
- 👆 Opens on **long press** or, optionally, on **click**
- 🗂️ **Groups** and nested **sub-menus** to organize actions
- 🎯 `Command` / `CommandParameter` support plus a `Clicked` event on each action
- ⚙️ Per-action state: `IsEnabled`, `IsVisible`, `IsDestructive`, `SubTitle`
- 🖼️ **Icons** — custom images (`Icon`) or SF Symbols / Android system icons (`SystemIcon`, iOS)
- 🔍 Customizable **preview** (rounded corners, background, padding) on iOS
- 📃 First-class **`CollectionView`** support, with per-item dynamic menus via data binding
- 🙈 Toggle the whole menu on/off with the bindable `IsVisible` attached property

## Supported platforms

| Platform | Minimum version |
| --- | --- |
| iOS | 15.0 |
| Mac Catalyst | 15.0 |
| Android | API 33 (13.0) |

> Note: Windows is not supported.

## Installation

Install the package from [NuGet](https://www.nuget.org/packages/Plugin.Maui.ContextMenu):

```bash
dotnet add package Plugin.Maui.ContextMenu
```

Then register the plugin in your `MauiProgram.cs` by calling `UseContextMenu()`:

```csharp
using Plugin.Maui.ContextMenu;

public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder
        .UseMauiApp<App>()
        .UseContextMenu(); // 👈 register the plugin

    return builder.Build();
}
```

> `UseContextMenu()` registers a custom `CollectionView` handler required for context menus inside a `CollectionView` on iOS. Even if you only use context menus on plain views, calling it is recommended.

## Getting started

Add the XAML namespace to your page:

```xml
xmlns:cm="clr-namespace:Plugin.Maui.ContextMenu;assembly=Plugin.Maui.ContextMenu"
```

Then attach a menu to any view with the `cm:ContextMenu.Menu` attached property. Long-press the view to open the menu:

```xml
<ContentView HeightRequest="200" WidthRequest="200" Background="GreenYellow">
    <cm:ContextMenu.Menu>
        <DataTemplate>
            <cm:Menu>
                <cm:Action Title="Upload documents" Command="{Binding UploadCommand}" CommandParameter="Hello" />
                <cm:Action Title="Copy" />
                <cm:Action Title="Paste" />
                <cm:Action Title="Secret" IsVisible="False" />
            </cm:Menu>
        </DataTemplate>
    </cm:ContextMenu.Menu>

    <Label Text="Long-press me" VerticalOptions="Center" HorizontalOptions="Center" />
</ContentView>
```

## Usage

### Action states

Each `Action` supports a number of properties to control its appearance and behavior:

```xml
<cm:Menu Title="Do something from here">
    <cm:Action Title="Copy" />
    <cm:Action Title="Paste" IsEnabled="False" />
    <cm:Action Title="Secret" IsVisible="False" />
    <cm:Action Title="Delete" IsDestructive="True" />
</cm:Menu>
```

| Property | Type | Description |
| --- | --- | --- |
| `Title` | `string` | The action's label. |
| `SubTitle` | `string` | Secondary text shown under the title (iOS). |
| `Command` | `ICommand` | Command executed when the action is tapped. |
| `CommandParameter` | `object` | Parameter passed to `Command`. |
| `Clicked` | event | Raised when the action is tapped. |
| `Icon` | `ImageSource` | Custom image icon (iOS). |
| `SystemIcon` | `string` | SF Symbol (iOS) / system icon (Android) name. |
| `IsEnabled` | `bool` | Whether the action can be tapped. Defaults to `true`. |
| `IsVisible` | `bool` | Whether the action is shown. Defaults to `true`. |
| `IsDestructive` | `bool` | Renders the action in a destructive style (e.g. red). |

The `Menu` itself and each `Group` also accept a `Title`.

### Groups and sub-menus

Use `Group` to visually separate related actions, and nest a `Menu` to create a sub-menu:

```xml
<cm:Menu>
    <cm:Action Title="Upload documents" />
    <cm:Group Title="Clipboard">
        <cm:Action Title="Copy" />
        <cm:Action Title="Paste" IsEnabled="False" />
    </cm:Group>
    <cm:Menu Title="More">
        <cm:Action Title="Rename" />
        <cm:Action Title="Move" />
    </cm:Menu>
    <cm:Action Title="Delete" IsDestructive="True" />
</cm:Menu>
```

### Icons

Provide a custom image with `Icon`, or reference a system icon with `SystemIcon` (SF Symbols on iOS, system resource names on Android):

```xml
<cm:Menu>
    <cm:Action Title="Upload documents" Icon="dotnet_bot.png" />
    <cm:Group Title="Clipboard">
        <cm:Action Title="Copy" SystemIcon="doc.on.clipboard" />
        <cm:Action Title="Delete" IsDestructive="True" SystemIcon="trash" />
    </cm:Group>
</cm:Menu>
```

> Android does not support per-item icons in native context menus, so icons are honored on iOS / Mac Catalyst only.

### Handling clicks with a command

Besides per-action commands, you can attach a single `ClickCommand` (with an optional `ClickCommandParameter`) to the host view. It is executed when the context menu interaction occurs — useful for interoperating with selection:

```xml
<ContentView Background="GreenYellow"
             cm:ContextMenu.ClickCommand="{Binding ClickCommand}"
             cm:ContextMenu.ClickCommandParameter="Hello">
    <cm:ContextMenu.Menu>
        <DataTemplate>
            <cm:Menu>
                <cm:Action Title="Copy" />
                <cm:Action Title="Paste" />
            </cm:Menu>
        </DataTemplate>
    </cm:ContextMenu.Menu>
</ContentView>
```

### Show the menu on click

Set `cm:ContextMenu.ShowMenuOnClick="True"` to open the menu on a normal tap instead of a long press:

```xml
<Button Text="Options" cm:ContextMenu.ShowMenuOnClick="True">
    <cm:ContextMenu.Menu>
        <DataTemplate>
            <cm:Menu>
                <cm:Action Title="Upload documents" />
                <cm:Action Title="Copy" />
            </cm:Menu>
        </DataTemplate>
    </cm:ContextMenu.Menu>
</Button>
```

> On iOS, `ShowMenuOnClick` is only supported for `Button`.

### Customizing the preview (iOS)

On iOS you can customize how the highlighted view is previewed while the menu is open — for example, rounding the corners:

```xml
<ContentView Background="GreenYellow">
    <cm:ContextMenu.Menu>
        <DataTemplate>
            <cm:Menu>
                <cm:Action Title="Copy" />
                <cm:Action Title="Paste" />
            </cm:Menu>
        </DataTemplate>
    </cm:ContextMenu.Menu>
    <cm:ContextMenu.Preview>
        <cm:Preview>
            <cm:Preview.VisiblePath>
                <RoundRectangle CornerRadius="10, 20, 30, 40" />
            </cm:Preview.VisiblePath>
        </cm:Preview>
    </cm:ContextMenu.Preview>
</ContentView>
```

`Preview` also exposes `PreviewTemplate` (a `DataTemplate` for a fully custom preview), `BackgroundColor`, and `Padding`.

### Toggling the menu on and off

The `cm:ContextMenu.IsVisible` attached property is bindable, so you can enable or disable the whole context menu at runtime:

```xml
<Switch x:Name="contextMenuToggle" IsToggled="True" />

<ContentView Background="GreenYellow"
             cm:ContextMenu.IsVisible="{Binding IsToggled, Source={x:Reference contextMenuToggle}}">
    <cm:ContextMenu.Menu>
        <DataTemplate>
            <cm:Menu>
                <cm:Action Title="Copy" />
            </cm:Menu>
        </DataTemplate>
    </cm:ContextMenu.Menu>
</ContentView>
```

### Using with a CollectionView

Attach the `Menu` to the `CollectionView` itself and every item gets its own context menu. The menu's `BindingContext` matches the `ItemTemplate`, so menus can be **dynamic per item**:

```xml
<CollectionView ItemsSource="{Binding Items}"
                cm:ContextMenu.ClickCommand="{Binding NotifyCommand}"
                cm:ContextMenu.IsVisible="{Binding IsContextMenuVisible}">
    <cm:ContextMenu.Menu>
        <DataTemplate>
            <cm:Menu Title="{Binding Text}">
                <cm:Action Title="{Binding Text}" />
                <cm:Action Title="Upload"
                           Command="{Binding BindingContext.TestCommand, Source={x:Reference this}}"
                           CommandParameter="{Binding}" />
            </cm:Menu>
        </DataTemplate>
    </cm:ContextMenu.Menu>

    <CollectionView.ItemTemplate>
        <DataTemplate>
            <ContentView Background="{Binding Color}">
                <Label Text="{Binding Text}" />
            </ContentView>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

> On iOS the `CollectionView` must use the handler registered by `UseContextMenu()` for the context menu event to work, so make sure you've called it in `MauiProgram.cs`.

## Sample app

A full sample app demonstrating every feature (simple menus, groups, icons, previews, click handling, show-on-click, and `CollectionView` integration) is available in the [`sample`](sample) folder of this repository.

## Special thanks

This plugin was **based on and inspired by** the excellent [The49.Maui.ContextMenu](https://github.com/the49ltd/The49.Maui.ContextMenu) library. A huge thank you to [**@paulvarache**](https://github.com/paulvarache), the author of the original work, whose implementation made this plugin possible. 🙏

## License

This project is licensed under the [MIT License](LICENSE.md).
