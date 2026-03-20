namespace Plugin.Maui.ContextMenu;

public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder UseContextMenu(this MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(cfg =>
        {
            cfg.AddHandler<CollectionView, CollectionViewHandler>();
        });

        return builder;
    }
}
