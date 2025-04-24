using UnityEngine;

[CreateAssetMenu(fileName = "ThemeData", menuName = "Game/Theme Data")]
public class ThemeData : ScriptableObject
{
    public string themeName;
    public Sprite themeIcon;
    [TextArea]
    public string description;
    public bool isFavorite;

    // Bonus : Difficulté ou catégorie du thème
    public enum ThemeCategory { Casual, Formal, Fantasy, Retro, Futuristic }
    public ThemeCategory category;
}
