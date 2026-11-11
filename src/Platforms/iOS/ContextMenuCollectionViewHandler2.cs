using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Handlers.Items2;
using UIKit;

namespace Plugin.Maui.ContextMenu;

/// <summary>
/// CollectionView handler based on .NET MAUI's Items2 handler (the default on iOS since .NET MAUI 10).
/// Adds per-item context menus (<see cref="ContextMenu.MenuProperty"/> set on the CollectionView)
/// and <see cref="ContextMenu.ClickCommandProperty"/> support.
/// Registered by <see cref="MauiAppBuilderExtensions.UseContextMenu"/> on iOS.
/// </summary>
public class ContextMenuCollectionViewHandler2 : CollectionViewHandler2
{
    public static readonly PropertyMapper<ReorderableItemsView, CollectionViewHandler2> ContextMenuMapper = CreateMapper();

    static PropertyMapper<ReorderableItemsView, CollectionViewHandler2> CreateMapper()
    {
        var mapper = new PropertyMapper<ReorderableItemsView, CollectionViewHandler2>(Mapper);
        mapper.ModifyMapping(SelectableItemsView.SelectionModeProperty.PropertyName, (handler, view, previous) =>
        {
            previous?.Invoke(handler, view);
            (handler as ContextMenuCollectionViewHandler2)?.ContextMenuController?.ApplySelectionSettings();
        });
        return mapper;
    }

    public ContextMenuCollectionViewHandler2() : base(ContextMenuMapper)
    {
    }

    /// <summary>The controller created for this handler (Items2 doesn't expose it publicly).</summary>
    public ContextMenuCollectionViewController2 ContextMenuController { get; private set; }

    protected override ItemsViewController2<ReorderableItemsView> CreateController(ReorderableItemsView itemsView, UICollectionViewLayout layout)
        => ContextMenuController = new ContextMenuCollectionViewController2(itemsView, layout);

    protected override void DisconnectHandler(UIView platformView)
    {
        base.DisconnectHandler(platformView);
        ContextMenuController = null;
    }
}

public class ContextMenuCollectionViewController2 : ReorderableItemsViewController2<ReorderableItemsView>
{
    public ContextMenuCollectionViewController2(ReorderableItemsView itemsView, UICollectionViewLayout layout) : base(itemsView, layout)
    {
    }

    protected override UICollectionViewDelegateFlowLayout CreateDelegator()
        => new ContextMenuCollectionViewDelegator2(ItemsViewLayout, this);

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        ApplySelectionSettings();
    }

    /// <summary>
    /// A tap must reach ItemSelected even with SelectionMode.None so ContextMenu.ClickCommand fires.
    /// </summary>
    internal void ApplySelectionSettings()
    {
        if (!IsViewLoaded || CollectionView == null || ItemsView == null)
        {
            return;
        }
        CollectionView.AllowsSelection = true;
        CollectionView.AllowsMultipleSelection = ItemsView.SelectionMode == SelectionMode.Multiple;
    }

    public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
    {
        var cell = base.GetCell(collectionView, indexPath);
        // Selection is force-enabled for ClickCommand; don't show a selection highlight when the
        // app asked for SelectionMode.None.
        if (ItemsView.SelectionMode == SelectionMode.None)
        {
            cell.SelectedBackgroundView = null;
        }
        return cell;
    }

    internal object GetItem(NSIndexPath indexPath) => ItemsSource?[indexPath];
}

public class ContextMenuCollectionViewDelegator2 : ReorderableItemsViewDelegator2<ReorderableItemsView, ContextMenuCollectionViewController2>
{
    NSIndexPath _highlightedIndexPath;

    public ContextMenuCollectionViewDelegator2(UICollectionViewLayout itemsViewLayout, ContextMenuCollectionViewController2 itemsViewController)
        : base(itemsViewLayout, itemsViewController)
    {
    }

    ReorderableItemsView ItemsView => ViewController?.ItemsView;

    UIContextMenuConfiguration CreateConfiguration(NSIndexPath indexPath)
    {
        var itemsView = ItemsView;
        if (itemsView == null || !ContextMenu.GetIsVisible(itemsView))
        {
            return null;
        }
        var menuTemplate = ContextMenu.GetMenu(itemsView);
        if (menuTemplate?.CreateContent() is not Menu menu)
        {
            return null;
        }
        BindableObject.SetInheritedBindingContext(menu, ViewController.GetItem(indexPath));
        return UIContextMenuConfiguration.Create(indexPath, null, _ => ContextMenu.CreateMenu(menu));
    }

    UITargetedPreview CreatePreview(UICollectionView collectionView, UIContextMenuConfiguration configuration, NSIndexPath indexPath)
    {
        if (indexPath == null || ItemsView == null || collectionView.CellForItem(indexPath) is not { } cell)
        {
            return null;
        }
        return ContextMenu.CreateTargetedPreview(cell, configuration, ViewController.GetItem(indexPath), ItemsView);
    }

    // iOS 16+ (multi-item API). When implemented, UIKit uses these instead of the deprecated ones below.
    public override UIContextMenuConfiguration GetContextMenuConfiguration(UICollectionView collectionView, NSIndexPath[] indexPaths, CGPoint point)
    {
        if (indexPaths == null || indexPaths.Length != 1)
        {
            return null;
        }
        return CreateConfiguration(indexPaths[0]);
    }

    public override UITargetedPreview GetContextMenuConfigurationHighlightPreview(UICollectionView collectionView, UIContextMenuConfiguration configuration, NSIndexPath indexPath)
        => CreatePreview(collectionView, configuration, indexPath);

    public override UITargetedPreview GetContextMenuConfigurationDismissalPreview(UICollectionView collectionView, UIContextMenuConfiguration configuration, NSIndexPath indexPath)
        => CreatePreview(collectionView, configuration, indexPath);

    // iOS 15 (single-item API, deprecated in iOS 16).
    public override UIContextMenuConfiguration GetContextMenuConfiguration(UICollectionView collectionView, NSIndexPath indexPath, CGPoint point)
        => CreateConfiguration(indexPath);

    public override UITargetedPreview GetPreviewForHighlightingContextMenu(UICollectionView collectionView, UIContextMenuConfiguration configuration)
    {
        _highlightedIndexPath = configuration.Identifier as NSIndexPath;
        return CreatePreview(collectionView, configuration, _highlightedIndexPath);
    }

    public override UITargetedPreview GetPreviewForDismissingContextMenu(UICollectionView collectionView, UIContextMenuConfiguration configuration)
    {
        var preview = CreatePreview(collectionView, configuration, _highlightedIndexPath);
        _highlightedIndexPath = null;
        return preview;
    }

    public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
    {
        var itemsView = ItemsView;
        if (itemsView == null)
        {
            return;
        }
        if (itemsView.SelectionMode == SelectionMode.None)
        {
            // Selection was only enabled for ClickCommand; don't keep the item selected so it can be tapped again.
            collectionView.DeselectItem(indexPath, false);
        }
        else
        {
            base.ItemSelected(collectionView, indexPath);
        }
        ContextMenu.ExecuteClickCommand(itemsView, ViewController.GetItem(indexPath));
    }
}
