namespace YourGame.UI.Widgets.Cursors
{
    public interface ICursorProvider
    {
        CursorDescriptor DefaultCursor { get; }
        CursorDescriptor LoadingCursor { get; }
        void ApplyCursor(CursorDescriptor descriptor);
    }
}
