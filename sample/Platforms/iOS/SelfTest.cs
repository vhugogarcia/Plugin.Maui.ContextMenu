using CoreGraphics;
using Foundation;
using Microsoft.Maui.Platform;
using Plugin.Maui.ContextMenu.Sample.Pages;
using UIKit;

namespace Plugin.Maui.ContextMenu.Sample;

/// <summary>
/// iOS smoke test, runs only when launched with CONTEXTMENU_SELFTEST=1
/// (xcrun simctl launch --console-pty with SIMCTL_CHILD_CONTEXTMENU_SELFTEST=1).
/// The simulator can't be tapped from the command line, so this calls the same UIKit delegate
/// methods a long-press / tap would, and prints "CMSELFTEST PASS|FAIL ..." lines.
/// </summary>
static class SelfTest
{
    public static bool Enabled => Environment.GetEnvironmentVariable("CONTEXTMENU_SELFTEST") == "1";

    static void Report(bool ok, string what) => Console.WriteLine($"CMSELFTEST {(ok ? "PASS" : "FAIL")} {what}");

    public static async Task RunAsync(Page root)
    {
        try
        {
            await Task.Delay(2000);
            var nav = root.Navigation;

            // 1) Menu set on the CollectionView itself (Items2 handler + delegator)
            var cvPage = new CollectionViewPage();
            await nav.PushModalAsync(new NavigationPage(cvPage));
            await Task.Delay(2500);
            await CheckCollectionView(cvPage);
            await nav.PopModalAsync();
            await Task.Delay(1000);

            // 2) Chat bubbles (Border menus inside an item template) in a modal page
            var chat = new ChatPage("Modal (self-test)");
            await nav.PushModalAsync(chat);
            await Task.Delay(2500);
            CheckBorders(chat);
            await nav.PopModalAsync();
            await Task.Delay(1000);

            // 3) Same page, non-modal
            var chat2 = new ChatPage("Non-modal (self-test)");
            await nav.PushAsync(chat2);
            await Task.Delay(2500);
            CheckBorders(chat2);
            await nav.PopAsync();
        }
        catch (Exception ex)
        {
            Report(false, "exception: " + ex);
        }
        Console.WriteLine("CMSELFTEST DONE");
    }

    static async Task CheckCollectionView(CollectionViewPage page)
    {
        var cv = page.GetVisualTreeDescendants().OfType<CollectionView>().First();
        Report(cv.Handler is ContextMenuCollectionViewHandler2, $"CollectionView handler is {cv.Handler?.GetType().Name}");

        var controller = (cv.Handler as ContextMenuCollectionViewHandler2)?.ContextMenuController;
        var collectionView = controller?.CollectionView;
        var del = collectionView?.Delegate as UICollectionViewDelegate;
        Report(collectionView?.AllowsSelection == true, $"selection enabled for ClickCommand (SelectionMode={cv.SelectionMode})");
        Report(del is ContextMenuCollectionViewDelegator2, $"UICollectionView delegate is {del?.GetType().Name}");
        if (collectionView == null || del is not ContextMenuCollectionViewDelegator2 delegator)
        {
            return;
        }

        var ip = NSIndexPath.FromItemSection(1, 0);
        var respondsNew = delegator.RespondsToSelector(new ObjCRuntime.Selector("collectionView:contextMenuConfigurationForItemsAtIndexPaths:point:"));
        Report(respondsNew, "delegate responds to contextMenuConfigurationForItemsAtIndexPaths (iOS 16+ API)");

        var config = delegator.GetContextMenuConfiguration(collectionView, new[] { ip }, CGPoint.Empty);
        Report(config != null, "long-press on item 1 returns a menu configuration");
        var preview = config == null ? null : delegator.GetContextMenuConfigurationHighlightPreview(collectionView, config, ip);
        Console.WriteLine($"CMSELFTEST INFO highlight preview: {(preview == null ? "system default (no ContextMenu.Preview set)" : "custom")}");

        // IsVisible=false must suppress the menu
        ContextMenu.SetIsVisible(cv, false);
        Report(delegator.GetContextMenuConfiguration(collectionView, new[] { ip }, CGPoint.Empty) == null, "no menu when ContextMenu.IsVisible=false");
        ContextMenu.SetIsVisible(cv, true);

        // Tap -> ClickCommand (the sample's NotifyCommand pushes a detail page with the item text)
        // (NotifyCommand pushes onto the window's root NavigationPage, which is under this modal)
        var rootNav = Application.Current!.Windows[0].Page!.Navigation;
        var before = rootNav.NavigationStack.Count;
        delegator.ItemSelected(collectionView, ip);
        await Task.Delay(1500);
        var top = rootNav.NavigationStack.LastOrDefault();
        Report(rootNav.NavigationStack.Count == before + 1 && top is CollectionViewDetailPage,
            $"tap on item 1 runs ClickCommand (root stack {before} -> {rootNav.NavigationStack.Count}, top={top?.GetType().Name})");
        Report(collectionView.GetIndexPathsForSelectedItems()?.Length == 1, "item stays selected (SelectionMode=Single, base selection kept)");
        if (top is CollectionViewDetailPage)
        {
            // PopAsync on the root stack while a modal covers it doesn't complete; just remove the page.
            rootNav.RemovePage(top);
        }
    }

    static void CheckBorders(ChatPage page)
    {
        var borders = page.GetVisualTreeDescendants().OfType<Border>()
            .Where(b => ContextMenu.GetMenu(b) != null && b.IsVisible && b.Handler != null).ToList();
        Report(borders.Count >= 3, $"{page.Mode}: found {borders.Count} visible Borders with menus");

        var withInteraction = 0;
        var withConfig = 0;
        foreach (var b in borders)
        {
            var view = (UIView)b.Handler.PlatformView;
            var interaction = view.Interactions.OfType<UIContextMenuInteraction>().FirstOrDefault();
            if (interaction == null)
            {
                continue;
            }
            withInteraction++;
            if (interaction.Delegate is IUIContextMenuInteractionDelegate d
                && new ContextMenuInteractionDelegate().GetConfigurationForMenu(interaction, CGPoint.Empty) != null)
            {
                withConfig++;
            }
        }
        Report(borders.Count > 0 && withInteraction == borders.Count, $"{page.Mode}: {withInteraction}/{borders.Count} Borders have a UIContextMenuInteraction");
        Report(borders.Count > 0 && withConfig == borders.Count, $"{page.Mode}: {withConfig}/{borders.Count} return a menu configuration");
    }
}
