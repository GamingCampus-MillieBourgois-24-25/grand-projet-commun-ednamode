using UnityEngine;

public class ShowRewardedAd : MonoBehaviour
{
    private DataSaver dataSaver;
    private void Start()
    {
        dataSaver = DataSaver.Instance;
        // Initialisation du SDK IronSource avec votre clé d'application
        IronSource.Agent.init("YOUR_APP_KEY", IronSourceAdUnits.REWARDED_VIDEO);

        // S'abonner aux événements de callback
        //IronSourceEvents.onRewardedVideoAdRewardedEvent += OnRewardedVideoAdRewarded;
        //IronSourceEvents.onRewardedVideoAdClosedEvent += OnRewardedVideoAdClosed;
        //IronSourceEvents.onRewardedVideoAdShowFailedEvent += OnRewardedVideoAdShowFailed;
        //IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += OnRewardedVideoAvailabilityChanged;

        Debug.Log("IronSource SDK initialisé.");
    }

    private void OnDestroy()
    {
        // Se désabonner des événements pour éviter les erreurs
        //IronSourceEvents.onRewardedVideoAdRewardedEvent -= OnRewardedVideoAdRewarded;
        //IronSourceEvents.onRewardedVideoAdClosedEvent -= OnRewardedVideoAdClosed;
        //IronSourceEvents.onRewardedVideoAdShowFailedEvent -= OnRewardedVideoAdShowFailed;
        //IronSourceEvents.onRewardedVideoAvailabilityChangedEvent -= OnRewardedVideoAvailabilityChanged;
    }

    public void Show()
    {
        // Vérifier si une vidéo récompensée est disponible
        if (IronSource.Agent.isRewardedVideoAvailable())
        {
            IronSource.Agent.showRewardedVideo();
            Debug.Log("Affichage de la vidéo récompensée.");
        }
        else
        {
            Debug.LogWarning("Aucune vidéo récompensée n'est disponible pour le moment.");
        }
    }

    // Callback : La vidéo a été regardée jusqu'à la fin
    private void OnRewardedVideoAdRewarded(IronSourcePlacement placement)
    {
        Debug.Log($"Vidéo récompensée terminée. Récompense : {placement.getRewardName()} {placement.getRewardAmount()}");

        dataSaver.addCoins(placement.getRewardAmount());
    }

    // Callback : La vidéo a été fermée
    private void OnRewardedVideoAdClosed()
    {
        Debug.Log("Vidéo récompensée fermée.");
    }

    // Callback : Échec de l'affichage de la vidéo
    private void OnRewardedVideoAdShowFailed(IronSourceError error)
    {
        Debug.LogError($"Échec de l'affichage de la vidéo récompensée : {error.getDescription()}");
    }

    // Callback : Disponibilité des vidéos récompensées
    private void OnRewardedVideoAvailabilityChanged(bool isAvailable)
    {
        Debug.Log($"Disponibilité des vidéos récompensées : {(isAvailable ? "Disponible" : "Indisponible")}");
    }
}
