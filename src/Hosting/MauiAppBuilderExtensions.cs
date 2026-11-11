namespace Plugin.Maui.ContextMenu;

public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder UseContextMenu(this MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(cfg =>
        {
#if IOS
            // Items2-based handler (MAUI's default on iOS). The Items1-based CollectionViewHandler is
            // obsolete in .NET MAUI 11; apps can still opt into it by registering it after UseContextMenu().
            cfg.AddHandler<CollectionView, ContextMenuCollectionViewHandler2>();
#elif ANDROID
            cfg.AddHandler<CollectionView, CollectionViewHandler>();
#endif
            // Mac Catalyst: no context menu implementation, keep .NET MAUI's default handler.
        });

        return builder;
    }
}
