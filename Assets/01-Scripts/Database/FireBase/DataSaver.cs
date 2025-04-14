using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.Database;
using Firebase.Auth;
using TMPro;
using Firebase;
using CharacterCustomization;

public class DataSaver : MonoBehaviour
{
    public static DataSaver Instance { get; private set; } // Singleton Instance

    public dataToSave dts;
    public string userId;
    private DatabaseReference dbRef;
    private FirebaseAuth auth;

    [TextArea]
    public string jsonConfig;

    [SerializeField]
    private TMP_Text dataDisplayText; // Référence au composant Text pour afficher les données

    private void Awake()
    {
        // Singleton logic
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Détruire les doublons
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persiste entre les scènes

        // Initialisation Firebase
        AppOptions options = AppOptions.LoadFromJsonConfig(jsonConfig);
        FirebaseApp app = FirebaseApp.Create(options, "DripOrDrop");
        auth = FirebaseAuth.GetAuth(app);

        dbRef = FirebaseDatabase.GetInstance(app).RootReference;

        if (auth.CurrentUser != null)
        {
            userId = auth.CurrentUser.UserId;
            Debug.Log($"Utilisateur connecté avec UID : {userId}");

            LoadDataFn(); // Charge les données à l'initialisation
        }
        else
        {
            Debug.LogError("Aucun utilisateur connecté. Assurez-vous que l'utilisateur est authentifié.");
        }
    }

    public void SaveDataFn()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("Impossible de sauvegarder les données : userId est null ou vide.");
            return;
        }

        string json = JsonUtility.ToJson(dts);
        dbRef.Child("users").Child(userId).SetRawJsonValueAsync(json);
    }

    public void LoadDataFn()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("Impossible de charger les données : userId est null ou vide.");
            return;
        }

        StartCoroutine(LoadDataEnum());
    }

    IEnumerator LoadDataEnum()
    {
        var serverData = dbRef.Child("users").Child(userId).GetValueAsync();
        yield return new WaitUntil(predicate: () => serverData.IsCompleted);

        print("process is complete");

        DataSnapshot snapshot = serverData.Result;
        string jsonData = snapshot.GetRawJsonValue();

        if (jsonData != null)
        {
            print("server data found");

            dts = JsonUtility.FromJson<dataToSave>(jsonData);

            UpdateDataDisplay(); // Met à jour l'affichage des données
        }
        else
        {
            print("no data found");
        }
    }

    public void UpdateDataDisplay()
    {
        if (dataDisplayText != null)
        {
            dataDisplayText.text = $"Nom d'utilisateur : {dts.userName}\n" +
                                   $"Total de pièces : {dts.totalCoins}\n" +
                                   $"Total de bijoux : {dts.totalJewels}\n" +
                                   $"Niveau actuel : {dts.crrLevel}\n" +
                                   $"Progression du niveau actuel : {dts.crrLevelProgress}/{dts.totalLevelProgress}";
        }
        else
        {
            Debug.LogError("Le composant Text pour afficher les données n'est pas assigné.");
        }
    }

    public void addCoins(int coins)
    {
        dts.totalCoins += coins;
        SaveDataFn();
        UpdateDataDisplay();
    }

    public void removeCoins(int coins)
    {
        dts.totalCoins -= coins;
        SaveDataFn();
        UpdateDataDisplay();
    }
    public int GetCoins() { return dts.totalCoins; }
    public void addJewels(int jewels)
    {
        dts.totalJewels += jewels;
        SaveDataFn();
        UpdateDataDisplay();
    }

    public void removeJewels(int jewels)
    {
        dts.totalJewels -= jewels;
        SaveDataFn();
        UpdateDataDisplay();
    }
    public int GetJewels() { return dts.totalJewels; }
    public void addLevelProgress(int levelProgress)
    {
        dts.totalLevelProgress += levelProgress;
        CheckForLevelUp(levelProgress);
        SaveDataFn();
        UpdateDataDisplay();
    }

    public void CheckForLevelUp(int levelProgress)
    {
        dts.crrLevelProgress += levelProgress;
        if (dts.crrLevelProgress > dts.totalLevelProgress)
        {
            dts.crrLevel++;
            dts.crrLevelProgress = dts.crrLevelProgress - dts.totalLevelProgress;
        }
        if (dts.crrLevelProgress == dts.totalLevelProgress)
        {
            dts.crrLevel++;
            dts.crrLevelProgress = 0;
        }
        SaveDataFn();
        UpdateDataDisplay();
    }

    [ContextMenu("Save")]
    public void Save()
    {
        SaveDataFn();
    }

    [ContextMenu("Load")]
    public void Load()
    {
        LoadDataFn();
    }
    public void AddItem(CustomizationItem item)
    {
        dts.ownedItems.Add(item);
        SaveDataFn();
    }
}

[Serializable]
public class dataToSave
{
    public string userName;
    public int totalCoins;
    public int totalJewels;
    public int crrLevel;
    public int crrLevelProgress;
    public int totalLevelProgress;
    public List<CustomizationItem> ownedItems;
    public List<Character> customCharacters;
}
