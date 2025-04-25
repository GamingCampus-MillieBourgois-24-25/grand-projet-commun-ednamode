using UnityEngine;

public class ShowRewardedAd : MonoBehaviour
{
    private DataSaver dataSaver;
    private void Start()
    {
        dataSaver = DataSaver.Instance;
        // Initialisation du SDK IronSource avec votre cl� d'application
        IronSource.Agent.init("YOUR_APP_KEY", IronSourceAdUnits.REWARDED_VIDEO);

        // S'abonner aux �v�nements de callback
        //IronSourceEvents.onRewardedVideoAdRewardedEvent += OnRewardedVideoAdRewarded;
        //IronSourceEvents.onRewardedVideoAdClosedEvent += OnRewardedVideoAdClosed;
        //IronSourceEvents.onRewardedVideoAdShowFailedEvent += OnRewardedVideoAdShowFailed;
        //IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += OnRewardedVideoAvailabilityChanged;

        Debug.Log("IronSource SDK initialis�.");
    }

    private void OnDestroy()
    {
        // Se d�sabonner des �v�nements pour �viter les erreurs
        //IronSourceEvents.onRewardedVideoAdRewardedEvent -= OnRewardedVideoAdRewarded;
        //IronSourceEvents.onRewardedVideoAdClosedEvent -= OnRewardedVideoAdClosed;
        //IronSourceEvents.onRewardedVideoAdShowFailedEvent -= OnRewardedVideoAdShowFailed;
        //IronSourceEvents.onRewardedVideoAvailabilityChangedEvent -= OnRewardedVideoAvailabilityChanged;
    }

    public void Show()
    {
        // V�rifier si une vid�o r�compens�e est disponible
        if (IronSource.Agent.isRewardedVideoAvailable())
        {
            IronSource.Agent.showRewardedVideo();
            Debug.Log("Affichage de la vid�o r�compens�e.");
        }
        else
        {
            Debug.LogWarning("Aucune vid�o r�compens�e n'est disponible pour le moment.");
        }
    }

    // Callback : La vid�o a �t� regard�e jusqu'� la fin
    private void OnRewardedVideoAdRewarded(IronSourcePlacement placement)
    {
        Debug.Log($"Vid�o r�compens�e termin�e. R�compense : {placement.getRewardName()} {placement.getRewardAmount()}");

        dataSaver.addCoins(placement.getRewardAmount());
    }

    // Callback : La vid�o a �t� ferm�e
    private void OnRewardedVideoAdClosed()
    {
        Debug.Log("Vid�o r�compens�e ferm�e.");
    }

    // Callback : �chec de l'affichage de la vid�o
    private void OnRewardedVideoAdShowFailed(IronSourceError error)
    {
        Debug.LogError($"�chec de l'affichage de la vid�o r�compens�e : {error.getDescription()}");
    }

    // Callback : Disponibilit� des vid�os r�compens�es
    private void OnRewardedVideoAvailabilityChanged(bool isAvailable)
    {
        Debug.Log($"Disponibilit� des vid�os r�compens�es : {(isAvailable ? "Disponible" : "Indisponible")}");
    }
}
