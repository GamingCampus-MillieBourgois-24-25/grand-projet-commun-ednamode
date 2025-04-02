using TMPro;
using UnityEngine;

/// <summary>
/// Composant à placer sur "Session Player List Item" prefab.
/// </summary>
public class PlayerListItemUI : MonoBehaviour
{
    [SerializeField, Tooltip("Texte affichant le nom du joueur.")]
    private TMP_Text playerNameText;

    public void SetPlayerName(string name)
    {
        if (playerNameText != null)
            playerNameText.text = name;
    }
}
