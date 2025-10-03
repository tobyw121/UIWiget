using UnityEngine;

namespace YourGame.UI.Widgets.Cursors
{
    public sealed class EventCursorProvider : ICursorProvider
    {
        public EventCursorProvider(CursorDescriptor defaultCursor, CursorDescriptor loadingCursor)
        {
            DefaultCursor = defaultCursor ?? CursorDescriptor.FromId("Arrow");
            LoadingCursor = loadingCursor ?? CursorDescriptor.FromId("Loading");
        }

        public CursorDescriptor DefaultCursor { get; }
        public CursorDescriptor LoadingCursor { get; }

        public void ApplyCursor(CursorDescriptor descriptor)
        {
            var target = descriptor ?? DefaultCursor;
            if (target.Texture != null)
            {
                Cursor.SetCursor(target.Texture, target.Hotspot, CursorMode.Auto);
            }
        }
    }
}
