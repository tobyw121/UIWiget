using System;
using UnityEngine;

namespace YourGame.UI.Widgets.Cursors
{
    [Serializable]
    public class CursorDescriptor
    {
        public string Id = "Arrow";
        public Texture2D Texture;
        public Vector2 Hotspot = Vector2.zero;

        public static CursorDescriptor FromId(string id)
        {
            return new CursorDescriptor { Id = id };
        }
    }
}
