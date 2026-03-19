using Maui.BindableProperty.Generator.Core;
using System.Collections;
using System.Collections.ObjectModel;

namespace Plugin.Maui.ContextMenu;

[ContentProperty(nameof(Children))]
public partial class Group : MenuElement
{
    [AutoBindable]
    ObservableCollection<MenuElement> children;

    public Group() : base()
    {
        Children = new ObservableCollection<MenuElement>();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        foreach (var item in Children)
        {
            SetInheritedBindingContext(item, BindingContext);
        }
    }
}
