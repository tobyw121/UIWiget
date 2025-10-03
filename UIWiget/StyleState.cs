using UnityEngine;
using System;

// This class is now in its own file to be accessible by all other UI scripts.
[Serializable]
public class StyleState
{
    [Tooltip("Die Farbe der Hauptgrafik für diesen Zustand.")]
    public Color color = Color.white;

    [Tooltip("Das Sprite, das für diesen Zustand verwendet wird (optional).")]
    public Sprite sprite;
}