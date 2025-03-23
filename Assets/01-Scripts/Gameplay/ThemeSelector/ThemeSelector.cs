using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;

public struct ThemeData : INetworkSerializable
{
    public string theme;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref theme);
    }
}

public class ThemeSelector : NetworkBehaviour
{
    private List<string> themes = new List<string>()
    {
        "Soirée Chic", "Streetwear", "Années 80", "Plage", "Futuriste",
        "Steampunk", "Gothique", "Kawaii", "Sport", "Haute Couture"
    };

    private NetworkVariable<int> randomSeed = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );
    private NetworkVariable<ThemeData> currentTheme = new NetworkVariable<ThemeData>(
        new ThemeData { theme = "" },
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private TMP_Text themeText;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log($"OnNetworkSpawn appelé - IsServer: {IsServer}, IsClient: {IsClient}, NetworkObjectId: {NetworkObjectId}, ThemeText assigné: {(themeText != null)}");

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton est null dans OnNetworkSpawn !");
            return;
        }

        // Si c'est le serveur, générer une graine
        if (IsServer)
        {
            if (randomSeed.Value == 0)
            {
                int seed = (int)(System.DateTime.Now.Ticks % int.MaxValue);
                randomSeed.Value = seed;
                Debug.Log($"Serveur - Graine générée : {seed}");
                GenerateThemeFromSeed();
            }
            else
            {
                Debug.Log($"Serveur - Graine déjà définie : {randomSeed.Value}");
                GenerateThemeFromSeed();
            }
        }

        // Écouter les changements
        randomSeed.OnValueChanged += OnSeedChanged;
        currentTheme.OnValueChanged += UpdateThemeUI;

        // Pour les clients
        if (!IsServer)
        {
            Debug.Log($"Client - Vérification initiale de la graine : {randomSeed.Value}, currentTheme : {currentTheme.Value.theme}");
            if (randomSeed.Value != 0 || !string.IsNullOrEmpty(currentTheme.Value.theme))
            {
                GenerateThemeFromSeed();
            }
            else
            {
                Invoke(nameof(TryGenerateThemeAfterDelay), 2f);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        randomSeed.OnValueChanged -= OnSeedChanged;
        currentTheme.OnValueChanged -= UpdateThemeUI;
        base.OnNetworkDespawn();
    }

    private void OnSeedChanged(int oldSeed, int newSeed)
    {
        Debug.Log($"Changement de graine détecté - Ancienne : {oldSeed}, Nouvelle : {newSeed}");
        if (newSeed != 0)
        {
            GenerateThemeFromSeed();
        }
    }

    private void TryGenerateThemeAfterDelay()
    {
        Debug.Log($"Client - Tentative de génération après délai - Graine actuelle : {randomSeed.Value}, currentTheme : {currentTheme.Value.theme}");
        if (randomSeed.Value != 0 || !string.IsNullOrEmpty(currentTheme.Value.theme))
        {
            GenerateThemeFromSeed();
        }
        else
        {
            Debug.Log("Client - Graine et thème toujours non définis, génération d'un thème temporaire...");
            Random.InitState((int)(System.DateTime.Now.Ticks % int.MaxValue));
            string tempTheme = themes[Random.Range(0, themes.Count)];
            UpdateThemeUI(new ThemeData(), new ThemeData { theme = tempTheme + " (temporaire)" });
        }
    }

    private void GenerateThemeFromSeed()
    {
        if (randomSeed.Value == 0 && string.IsNullOrEmpty(currentTheme.Value.theme))
        {
            Debug.Log("Graine et thème non encore définis, en attente...");
            return;
        }

        string initialTheme;
        if (randomSeed.Value != 0)
        {
            Random.InitState(randomSeed.Value);
            initialTheme = themes[Random.Range(0, themes.Count)];
            Debug.Log($"Thème initial généré localement avec graine {randomSeed.Value} : {initialTheme}");
        }
        else
        {
            initialTheme = currentTheme.Value.theme;
            Debug.Log($"Thème récupéré de currentTheme : {initialTheme}");
        }

        UpdateThemeUI(new ThemeData(), new ThemeData { theme = initialTheme });

        if (IsServer && string.IsNullOrEmpty(currentTheme.Value.theme))
        {
            currentTheme.Value = new ThemeData { theme = initialTheme };
            Debug.Log($"Serveur - currentTheme défini : {currentTheme.Value.theme}");
        }
    }

    private void UpdateThemeUI(ThemeData oldValue, ThemeData newValue)
    {
        Debug.Log($"UpdateThemeUI appelé - Ancien thème : {oldValue.theme}, Nouveau thème : {newValue.theme}");
        if (themeText != null)
        {
            if (string.IsNullOrEmpty(newValue.theme))
            {
                themeText.text = "Thème : Aucun thème sélectionné";
                Debug.LogWarning("Aucun thème assigné à currentTheme.Value.theme");
            }
            else
            {
                themeText.text = "Thème : " + newValue.theme;
                Debug.Log($"UI mise à jour - Thème affiché : {newValue.theme}");
            }
        }
        else
        {
            Debug.LogError("ThemeText n'est pas assigné dans l'inspecteur !");
        }
    }

    public string GetCurrentTheme()
    {
        return currentTheme.Value.theme;
    }

    void Update()
    {
        if (IsServer && Input.GetKeyDown(KeyCode.Space))
        {
            int newSeed = (int)(System.DateTime.Now.Ticks % int.MaxValue);
            randomSeed.Value = newSeed;
            Debug.Log($"Serveur - Nouvelle graine générée (via Space) : {newSeed}");
        }
    }
}