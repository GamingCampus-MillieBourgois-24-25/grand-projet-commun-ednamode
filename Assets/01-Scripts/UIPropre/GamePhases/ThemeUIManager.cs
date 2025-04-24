using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

/// <summary>
/// Gère l'affichage visuel des sélections de catégorie et de thème.
/// Animation des panels via DoTween ou Coroutine.
/// </summary>
public class ThemeUIManager : MonoBehaviour
{
	#region Singleton

	public static ThemeUIManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	#endregion

	#region UI References

	[Header("UI Catégorie")]
	public GameObject categoryPanel;
	public TMP_Text categoryText;
	public Image categoryIcon;

	[Header("UI Thème")]
	public GameObject themePanel;
	public TMP_Text themeText;
	public Image themeIcon;

	[Header("Options Animation")]
	public float categoryDisplayTime = 2f;
	public float themeDisplayTime = 3f;

	#endregion

	#region Public Methods

	/// <summary>
	/// Lance l'affichage de la catégorie sélectionnée.
	/// </summary>
	public void ShowCategory(string categoryName)
	{
		StartCoroutine(CategorySequence(categoryName));
	}

	/// <summary>
	/// Lance l'affichage du thème sélectionné.
	/// </summary>
	public void ShowTheme(string themeName)
	{
		StartCoroutine(ThemeSequence(themeName));
	}

	#endregion

	#region Coroutines

	private IEnumerator CategorySequence(string categoryName)
	{
		categoryText.text = categoryName;
		categoryIcon.color = GetCategoryColor(categoryName);
		categoryPanel.SetActive(true);

		// Exemple d'animation simple (scale punch)
#if DOTWEEN
		categoryPanel.transform.localScale = Vector3.zero;
		categoryPanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
#endif

		yield return new WaitForSeconds(categoryDisplayTime);
		categoryPanel.SetActive(false);
	}

	private IEnumerator ThemeSequence(string themeName)
	{
		themeText.text = themeName;
		ThemeData themeData = ThemeManager.Instance.CurrentTheme;
		if (themeData != null && themeData.themeIcon != null)
			themeIcon.sprite = themeData.themeIcon;

		themePanel.SetActive(true);

#if DOTWEEN
		themePanel.transform.localScale = Vector3.zero;
		themePanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutElastic);
#endif

		yield return new WaitForSeconds(themeDisplayTime);
		themePanel.SetActive(false);
	}

	#endregion

	#region Utils

	/// <summary>
	/// Retourne une couleur en fonction de la catégorie pour l'icône.
	/// </summary>
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

	#endregion
}
