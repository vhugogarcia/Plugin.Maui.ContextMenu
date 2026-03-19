using Maui.BindableProperty.Generator.Core;

namespace Plugin.Maui.ContextMenu;

public partial class MenuElement : Element
{
    [AutoBindable]
    readonly string title;
}
