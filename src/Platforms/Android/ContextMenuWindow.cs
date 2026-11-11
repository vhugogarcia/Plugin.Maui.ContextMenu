using Android;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using Kotlin.Jvm.Functions;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;

namespace Plugin.Maui.ContextMenu;

internal class ContextMenuBackgroundView : FrameLayout
{
    public event EventHandler DismissEvent;
    public ContextMenuBackgroundView(Context context) : base(context)
    { }

    protected override void OnAttachedToWindow()
    {
        base.OnAttachedToWindow();
        var color = Application.Current.GetResourceOrDefault(ContextMenu.PreviewWindowBackgroundColorResource, Colors.Gray);
        var opacity = Application.Current.GetResourceOrDefault(ContextMenu.PreviewWindowBackgroundColorResource, .05f);
        Background = new ColorDrawable(color.ToPlatform());
        Background.Alpha = (int)Math.Round(255 - opacity * 255);
    }

    public override bool OnTouchEvent(MotionEvent e)
    {
        if (e.Action == MotionEventActions.Down)
        {
            Dismiss();
            return true;
        }
        else
        {
            return base.OnTouchEvent(e);
        }
    }

    void Dismiss()
    {
        DismissEvent?.Invoke(this, EventArgs.Empty);
    }
}

internal class ContextMenuTemplatedPreview : ContextMenuPreview
{
    public ContextMenuTemplatedPreview(Context context, Preview? preview) : base(context, preview)
    {
    }
}

internal class CustomOutline : ViewOutlineProvider
{
    Context _context;
    int _width;
    int _height;
    Preview? _preview;

    public CustomOutline(Context context, int width, int height, Preview? preview)
    {
        _context = context;
        _width = width;
        _height = height;
        _preview = preview;
    }
    public override void GetOutline(AView view, Outline outline)
    {
        if (_preview == null)
        {
            outline.SetRect(0, 0, _width, _height);
        }
        else
        {
            var density = _context.Resources.DisplayMetrics.Density;
            var bounds = new Microsoft.Maui.Graphics.Rect(
                _preview.Padding.Left,
                _preview.Padding.Top,
                _width / density - _preview.Padding.HorizontalThickness,
                _height / density - _preview.Padding.VerticalThickness
            );
            outline.SetPath(_preview.VisiblePath.PathForBounds(bounds).AsAndroidPath(0, 0, density, density));
        }
    }
}

internal class ContextMenuPreview : FrameLayout
{
    Context _context;
    Preview? _preview;
    protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
    {
        if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
        {
            OutlineProvider = new CustomOutline(_context, w, h, _preview);
        }
    }

    public ContextMenuPreview(Context context, Preview? preview) : base(context)
    {
        _context = context;
        _preview = preview;
        ClipToOutline = true;
        Elevation = ViewUtils.DpToPx(8);
    }
    public override bool OnTouchEvent(MotionEvent e)
    {
        return true;
    }
}

internal class ContextMenuSnapshotPreview : ContextMenuPreview
{
    public ContextMenuSnapshotPreview(Context context, Bitmap bitmap, Preview? preview) : base(context, preview)
    {
        Background = new BitmapDrawable(context.Resources, bitmap);
    }

    protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
    {
        base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        SetMeasuredDimension(Background.IntrinsicWidth, Background.IntrinsicHeight);
    }
}

/// <summary>
/// Root of the overlay window. Routes the Back gesture/key to <see cref="BackPressed"/> so that
/// Back dismisses the menu instead of reaching the page underneath.
/// </summary>
internal class ContextMenuDecorView : FrameLayout
{
    BackCallback _backCallback;

    public event EventHandler BackPressed;

    public ContextMenuDecorView(Context context) : base(context)
    {
        // Needed so the overlay window takes key focus and receives KEYCODE_BACK.
        Focusable = true;
        FocusableInTouchMode = true;
    }

    protected override void OnAttachedToWindow()
    {
        base.OnAttachedToWindow();
        RequestFocus();
        // Android 13+: with predictive back (on by default for apps targeting API 36), the system
        // no longer sends KEYCODE_BACK; it invokes the callbacks of the focused window instead.
        if (OperatingSystem.IsAndroidVersionAtLeast(33) && FindOnBackInvokedDispatcher() is { } dispatcher)
        {
            _backCallback = new BackCallback(() => BackPressed?.Invoke(this, EventArgs.Empty));
            dispatcher.RegisterOnBackInvokedCallback(Android.Window.IOnBackInvokedDispatcher.PriorityOverlay, _backCallback);
        }
    }

    protected override void OnDetachedFromWindow()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(33) && _backCallback is not null)
        {
            FindOnBackInvokedDispatcher()?.UnregisterOnBackInvokedCallback(_backCallback);
            _backCallback = null;
        }
        base.OnDetachedFromWindow();
    }

    public override bool DispatchKeyEvent(KeyEvent e)
    {
        // Legacy back dispatch (predictive back disabled, or older behavior).
        if (e?.KeyCode == Keycode.Back)
        {
            if (e.Action == KeyEventActions.Up && !e.IsCanceled)
            {
                BackPressed?.Invoke(this, EventArgs.Empty);
            }
            return true;
        }
        return base.DispatchKeyEvent(e);
    }

    sealed class BackCallback : Java.Lang.Object, Android.Window.IOnBackInvokedCallback
    {
        readonly System.Action _action;
        public BackCallback(System.Action action) => _action = action;
        public void OnBackInvoked() => _action();
    }
}

internal class ContextMenuWindow
{
    public const int DismissAnimationDuration = 120;
    public const int ShowAnimationDuration = 200;
    public const int ContextMenuSpacing = 16;
    public const int ContextMenuEdgeSpacing = 16;

    // Extra time after the dismiss animation before the fallback forces the teardown.
    const int DismissFallbackSlack = 250;

    Context _context;
    IWindowManager _windowManager;
    ContextMenuDecorView _decorView;
    AView _parent;
    AView _content;
    ContextMenuBackgroundView _backgroundView;
    ContextMenuPopup _menu;
    private int _offsetX;
    private int _offsetY;

    bool _dismissing;
    bool _finished;
    bool _parentHidden;
    ViewStates _parentOriginalVisibility = ViewStates.Visible;
    readonly Handler _handler = new Handler(Looper.MainLooper!);

    public ContextMenuWindow(Context context, AView parent, Preview? p)
    {
        var bitmap = GetViewSnapshot(parent);
        var preview = new ContextMenuSnapshotPreview(context, bitmap, p);
        Setup(context, parent, preview);
    }

    public ContextMenuWindow(Context context, AView parent, AView content, Preview? p)
    {
        var preview = new ContextMenuTemplatedPreview(context, p);
        preview.AddView(content, new FrameLayout.LayoutParams(FrameLayout.LayoutParams.MatchParent, FrameLayout.LayoutParams.MatchParent));
        Setup(context, parent, preview);
    }

    void Setup(Context context, AView parent, ContextMenuPreview content)
    {
        _context = context;
        _parent = parent;
        _content = content;
        _content.ScaleX = ContextMenu.LongPressScaleFactor;
        _content.ScaleY = ContextMenu.LongPressScaleFactor;
        _windowManager = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

        _menu = new ContextMenuPopup(content);

        _menu.DismissEvent += OnMenuClosed;
    }

    private void OnMenuClosed(object sender, EventArgs e)
    {
        Dismiss();
    }

    public IMenu Menu => _menu.Menu;

    ContextMenuBackgroundView CreateBackgroundView()
    {
        var backgroundView = new ContextMenuBackgroundView(_context);
        backgroundView.DismissEvent += (s, e) =>
        {
            Dismiss();
        };
        return backgroundView;
    }
    ContextMenuDecorView CreateDecorView(AView background, AView content)
    {
        var decorView = new ContextMenuDecorView(_context);
        decorView.AddView(background, new FrameLayout.LayoutParams(FrameLayout.LayoutParams.MatchParent, FrameLayout.LayoutParams.MatchParent));
        decorView.AddView(content, new FrameLayout.LayoutParams(FrameLayout.LayoutParams.WrapContent, FrameLayout.LayoutParams.WrapContent));
        decorView.BackPressed += (s, e) => Dismiss();
        return decorView;
    }

    /// <summary>
    /// Animated dismiss. Safe to call more than once (the popup's DismissEvent calls back into it).
    /// The parent view is always restored by <see cref="Finish"/>, which does not depend on the
    /// animation completing: it also runs from a timeout fallback and when the parent leaves the window.
    /// </summary>
    void Dismiss()
    {
        if (_dismissing || _finished)
        {
            return;
        }
        _dismissing = true;

        _menu?.Dismiss();

        if (_decorView?.IsAttachedToWindow != true || !_parent.IsAttachedToWindow)
        {
            Finish();
            return;
        }

        var pos = new int[2];
        _parent.GetLocationInWindow(pos);
        _content.Animate()
            .ScaleX(1)
            .ScaleY(1)
            .Y(pos[1])
            .SetDuration(DismissAnimationDuration)
            .WithEndAction(Finish)
            .Start();

        AnimateBackgroundOut();

        // The end action is skipped when the animation is cancelled or the view is detached.
        _handler.PostDelayed(Finish, DismissAnimationDuration + DismissFallbackSlack);
    }

    /// <summary>Tears everything down immediately and restores the parent view. Idempotent.</summary>
    void Finish()
    {
        if (_finished)
        {
            return;
        }
        _finished = true;
        _dismissing = true;

        _handler.RemoveCallbacksAndMessages(null);
        _parent.ViewDetachedFromWindow -= OnParentDetached;

        RestoreParent();

        try
        {
            _menu?.Dismiss();
        }
        catch (Java.Lang.Exception)
        {
            // The popup's window may already be gone (e.g. its host window was removed).
        }

        if (_decorView?.Parent != null)
        {
            _content.Animate().Cancel();
            _backgroundView?.Animate().Cancel();
            try
            {
                _windowManager.RemoveViewImmediate(_decorView);
            }
            catch (Java.Lang.Exception)
            {
                // Window was already removed together with its parent window.
            }
        }
    }

    void RestoreParent()
    {
        _parent.ScaleX = 1f;
        _parent.ScaleY = 1f;
        // Only undo our own change: if something else (e.g. a bound IsVisible) changed the
        // visibility while the menu was open, that value wins.
        if (_parentHidden && _parent.Visibility == ViewStates.Invisible)
        {
            _parent.Visibility = _parentOriginalVisibility;
        }
        _parentHidden = false;
    }

    void OnParentDetached(object sender, AView.ViewDetachedFromWindowEventArgs e)
    {
        // The page (or modal dialog) hosting the view went away while the menu was open.
        Finish();
    }

    public static Bitmap GetViewSnapshot(AView v)
    {
        Bitmap b = Bitmap.CreateBitmap(v.Width, v.Height, Bitmap.Config.Argb8888);
        Canvas c = new Canvas(b);
        v.Draw(c);
        return b;
    }

    public void Show(int offsetX = 0, int offsetY = 0)
    {
        _offsetX = offsetX;
        _offsetY = offsetY;

        // Attach the overlay to the window that hosts the view. Modal pages live in their own
        // dialog window; using the activity's window would put the overlay *under* the modal.
        var token = _parent.WindowToken;
        if (token == null)
        {
            return;
        }

        _backgroundView = CreateBackgroundView();
        _decorView = CreateDecorView(_backgroundView, _content);

        var layoutParams = new WindowManagerLayoutParams(
            WindowManagerLayoutParams.MatchParent, WindowManagerLayoutParams.MatchParent,
            WindowManagerTypes.ApplicationPanel,
            WindowManagerFlags.LayoutNoLimits,
            Format.Translucent)
        {
            Token = token,
        };
        _windowManager.AddView(_decorView, layoutParams);

        _parent.ViewDetachedFromWindow += OnParentDetached;

        _parent.ScaleX = ContextMenu.LongPressScaleFactor;
        _parent.ScaleY = ContextMenu.LongPressScaleFactor;

        _backgroundView.Alpha = 0;

        ViewKt.DoOnLayout(_backgroundView, new Function1Impl<AView>(v =>
        {
            ShowAfterLayout();
        }));
    }

    void ShowAfterLayout()
    {
        if (_dismissing || _finished)
        {
            return;
        }

        _parent.ScaleX = 1f;
        _parent.ScaleY = 1f;
        Update();

        var overlayHeight = _backgroundView.Height;

        var menuHeight = _menu.GetHeight();

        var previewHeight = _content.MeasuredHeight;

        var endOfStack = _content.GetY()
            + previewHeight
            + ViewUtils.DpToPx(ContextMenuSpacing)
            + menuHeight
            + ViewUtils.DpToPx(ContextMenuEdgeSpacing);

        var willOverflow = endOfStack > overlayHeight;

        var endY = _content.GetY();

        if (willOverflow)
        {
            endY = overlayHeight
                - ViewUtils.DpToPx(ContextMenuEdgeSpacing)
                - menuHeight
                - ViewUtils.DpToPx(ContextMenuSpacing)
                - previewHeight;
        }

        _parentOriginalVisibility = _parent.Visibility;
        _parentHidden = true;
        _parent.Visibility = ViewStates.Invisible;

        _content.Animate()
            .ScaleX(1)
            .ScaleY(1)
            .Y(endY)
            .SetDuration(ShowAnimationDuration)
            .WithEndAction(() =>
            {
                if (_dismissing || _finished || _decorView?.IsAttachedToWindow != true)
                {
                    return;
                }
                _menu.Show(_offsetX, _offsetY + ViewUtils.DpToPx(ContextMenuWindow.ContextMenuSpacing));
            })
            .Start();

        AnimateBackgroundIn();
    }

    void AnimateBackgroundIn()
    {
        _backgroundView.Animate()
            .Alpha(1)
            .SetDuration(ShowAnimationDuration)
            .Start();
    }

    void AnimateBackgroundOut()
    {
        _backgroundView.Animate()
            .Alpha(0)
            .SetDuration(DismissAnimationDuration)
            .Start();
    }

    void Update()
    {
        var pos = new int[2];
        _parent.GetLocationInWindow(pos);
        float x = pos[0];
        float y = pos[1];

        var cX = (int)Math.Round(x + _parent.Width / 2);
        var cY = (int)Math.Round(y + _parent.Height / 2);

        _content.SetX(cX - _content.MeasuredWidth / 2);
        _content.SetY(cY - _content.MeasuredHeight / 2);
    }
}

class Function1Impl<T> : Java.Lang.Object, IFunction1 where T : Java.Lang.Object
{
    private readonly Action<T> OnInvoked;

    public Function1Impl(Action<T> onInvoked)
    {
        OnInvoked = onInvoked;
    }

    public Java.Lang.Object Invoke(Java.Lang.Object objParameter)
    {
        try
        {
            T parameter = (T)objParameter;
            OnInvoked?.Invoke(parameter);
            return null;
        }
        catch (Exception ex)
        {
            // Exception handling, if needed
        }
        return null;
    }
}