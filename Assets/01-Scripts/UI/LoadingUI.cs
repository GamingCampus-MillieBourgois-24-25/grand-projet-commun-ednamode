using Unity.Netcode;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class LoadingUI : NetworkBehaviour
{
    [SerializeField] private CanvasGroup loadingCanvasGroup;
    [SerializeField] private TextMeshProUGUI loadingText;
    private bool isLoading = false;

    private void Start()
    {
        if (!IsServer)
        {
            RequestUIActivationServerRpc();
        }
    }

    [ServerRpc]
    private void RequestUIActivationServerRpc()
    {
        ActivateUIClientRpc();
    }

    [ClientRpc]
    private void ActivateUIClientRpc()
    {
        if (loadingCanvasGroup == null) return;
        loadingCanvasGroup.gameObject.SetActive(true);
    }
    private void Awake()
    {
        if (loadingCanvasGroup == null)
        {
            Debug.LogError("[LoadingUI] Erreur : CanvasGroup non assign� !");
        }
        else
        {
            loadingCanvasGroup.alpha = 0; // Masqu� par d�faut
            loadingCanvasGroup.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// D�tecte le texte du bouton cliqu� et appelle ShowLoading() avec le bon param�tre.
    /// </summary>
    public void ShowLoadingFromButton(Button button)
    {
        if (button == null) return;

        // R�cup�re le texte du bouton et le met en minuscule pour �viter les erreurs
        string buttonText = button.GetComponentInChildren<TextMeshProUGUI>().text.ToLower();

        // V�rifie si le bouton est "Create" ou "Join"
        if (buttonText.Contains("create"))
        {
            ShowLoading("Create");
        }
        else if (buttonText.Contains("join"))
        {
            ShowLoading("Join");
        }
        else
        {
            ShowLoading("Loading"); // Cas par d�faut
        }
    }

    /// <summary>
    /// Affiche le bon message de chargement en fonction de l'action.
    /// </summary>
    public void ShowLoading(string actionType)
    {
        if (isLoading || loadingCanvasGroup == null || loadingText == null)
            return;

        isLoading = true;
        loadingCanvasGroup.gameObject.SetActive(true);
        loadingCanvasGroup.alpha = 0;

        switch (actionType)
        {
            case "Create":
                loadingText.text = "Creating session...";
                Debug.Log("[LoadingUI] Cr�ation de la session en cours.");
                break;
            case "Join":
                loadingText.text = "Joining session...";
                Debug.Log("[LoadingUI] Connexion � la session en cours.");
                break;
            case "Leave":
                loadingText.text = "Leaving session...";
                Debug.Log("[LoadingUI] D�connexion de la session en cours.");
                break;
            case "Kick":
                loadingText.text = "Kicking player...";
                Debug.Log("[LoadingUI] Expulsion du joueur en cours.");
                break;
            default:
                loadingText.text = "Loading...";
                Debug.Log("[LoadingUI] Chargement en cours.");
                break;
        }

        loadingCanvasGroup.DOFade(1, 0.3f);
        loadingCanvasGroup.transform.localScale = Vector3.one * 0.8f;
        loadingCanvasGroup.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

        loadingText.alpha = 0;
        loadingText.DOFade(1, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    public void HideLoading()
    {
        if (!isLoading || loadingCanvasGroup == null)
            return;

        isLoading = false;

        loadingText.DOKill();
        loadingCanvasGroup.DOFade(0, 0.3f).OnComplete(() => loadingCanvasGroup.gameObject.SetActive(false));

        Debug.Log("[LoadingUI] L��cran de chargement a �t� masqu�.");
    }

    public void ShowTemporaryMessage(string message)
    {
        ShowTemporaryMessageInternal(message, 2f);
    }

    private void ShowTemporaryMessageInternal(string message, float duration)
    {
        if (loadingCanvasGroup == null || loadingText == null)
            return;

        loadingText.DOKill();
        loadingCanvasGroup.gameObject.SetActive(true);
        loadingCanvasGroup.alpha = 1;
        loadingText.text = message;

        Debug.Log($"[LoadingUI] Message temporaire affich� : {message} (dur�e : {duration} secondes)");

        loadingCanvasGroup.DOFade(0, 0.5f).SetDelay(duration).OnComplete(() =>
        {
            loadingCanvasGroup.gameObject.SetActive(false);
        });
    }

    // Fonctions sp�cifiques pour Unity Events
    public void ShowErrorMessage()
    {
        ShowTemporaryMessage("Error!");
        Debug.Log("[LoadingUI] Une erreur est survenue.");
    }

    public void ShowSessionFailedMessage()
    {
        ShowTemporaryMessage("Connection failed!");
        Debug.Log("[LoadingUI] La connexion � la session a �chou�.");
    }

    public void ShowSessionNotFoundMessage()
    {
        ShowTemporaryMessage("Session not found!");
        Debug.Log("[LoadingUI] La session est introuvable.");
    }

    public void ShowKickMessage()
    {
        ShowTemporaryMessage("You kicked someone!");
        Debug.Log("[LoadingUI] Le joueur a �t� expuls�.");
    }
}
