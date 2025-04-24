using UnityEngine;

[CreateAssetMenu(fileName = "ThemeData", menuName = "Game/Theme Data")]
public class ThemeData : ScriptableObject
{
    public string themeName;
    public Sprite themeIcon;
    [TextArea]
    public string description;
    public bool isFavorite;

    // Bonus : Difficult� ou cat�gorie du th�me
    public enum ThemeCategory { Casual, Formal, Fantasy, Retro, Futuristic }
    public ThemeCategory category;
}
