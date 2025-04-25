using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

public class ThemeUIManager : MonoBehaviour
{
    public static ThemeUIManager Instance { get; private set; }

    [Header("UI Catégorie")]
    public GameObject categoryPanel;
    public TMP_Text categoryText;
    //public Image categoryIcon;

    [Header("UI Thème")]
    public GameObject themePanel;
    public TMP_Text themeText;
    //public Image themeIcon;

    [Header("Options Animation")]
    public float categoryDisplayTime = 2f;
    public float themeDisplayTime = 3f;

    public float TotalDisplayTime => categoryDisplayTime + themeDisplayTime;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void DisplayThemeSequence(string categoryName, string themeName)
    {
        StartCoroutine(ThemeFullSequence(categoryName, themeName));
    }

    private IEnumerator ThemeFullSequence(string categoryName, string themeName)
    {
        // Affichage Catégorie
        if (categoryText == null || /*categoryIcon == null ||*/ categoryPanel == null)
        {
            Debug.LogError("[ThemeUIManager] Références UI Catégorie manquantes !");
            yield break;
        }

        categoryText.text = categoryName;
        //categoryIcon.color = GetCategoryColor(categoryName);
        categoryPanel.SetActive(true);

#if DOTWEEN
        categoryPanel.transform.localScale = Vector3.zero;
        categoryPanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
#endif

        yield return new WaitForSeconds(categoryDisplayTime);
        categoryPanel.SetActive(false);

        // Affichage Thème
        themeText.text = themeName;  // Toujours assigner le texte AVANT la condition

        if (themeName == "Imposteur !")
        {
            //themeIcon.gameObject.SetActive(false);
            themeText.color = Color.red;
            // AudioManager.Instance.Play("ImpostorReveal");
        }
        else
        {
            var themeData = ThemeManager.Instance.CurrentTheme;
/*            if (themeData != null && themeData.themeIcon != null)
            {
                themeIcon.sprite = themeData.themeIcon;
                themeIcon.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[ThemeUIManager] ThemeData ou son icône est null !");
                themeIcon.gameObject.SetActive(false);
            }*/
            themeText.color = Color.white;
        }


        themePanel.SetActive(true);
#if DOTWEEN
        themePanel.transform.localScale = Vector3.zero;
        themePanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutElastic);
#endif
        yield return new WaitForSeconds(themeDisplayTime);
        themePanel.SetActive(false);
    }

    private Color GetCategoryColor(string categoryName)
    {
        return categoryName switch
        {
            "Futuristic" => Color.cyan,
            "Fantasy" => new Color(0.6f, 0.4f, 0.8f),
            "Retro" => new Color(1f, 0.65f, 0f),
            "Casual" => Color.green,
            "Formal" => Color.gray,
            _ => Color.white,
        };
    }
}