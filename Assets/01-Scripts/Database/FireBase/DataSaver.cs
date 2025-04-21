using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.Database;
using Firebase.Auth;
using TMPro;
using UnityEditor;
using Firebase; // Import n�cessaire pour TextMeshPro
using CharacterCustomization; // Ajout de l'espace de noms correct


public class DataSaver : MonoBehaviour
{

    public static DataSaver Instance; // Singleton

    public dataToSave dts;
    public string userId;
    private DatabaseReference dbRef;
    private FirebaseAuth auth;

    [TextArea]
    public string jsonConfig;

    [SerializeField]
    private TMP_Text dataDisplayText; // R�f�rence au composant Text pour afficher les donn�es

    #region UserId Management
    public string GetUserId()
    {
        return userId;
    }

    public void SetUserId(string newUserId)
    {
        userId = newUserId;
    }
     private void Awake()
    {
        // Impl�mentation du Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (dts == null)
        {
            dts = new dataToSave(); // Initialisation de l'objet
        }

        try
        {
            // Initialisation Firebase
            AppOptions options = AppOptions.LoadFromJsonConfig(jsonConfig);
            FirebaseApp app = FirebaseApp.Create(options, "DripOrDrop");
            auth = FirebaseAuth.GetAuth(app);
            dbRef = FirebaseDatabase.GetInstance(app).RootReference;
            Debug.Log("Firebase initialis� avec succ�s.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erreur lors de l'initialisation de Firebase : {ex.Message}");
        }
    }
    public void InitializeDataSaver()
    {

        if (auth.CurrentUser != null)
        {
            userId = auth.CurrentUser.UserId;
            Debug.Log($"Utilisateur connect� avec UID : {userId}");

            LoadDataFn();
        }
        else
        {
            Debug.LogError("Aucun utilisateur connect�. Assurez-vous que l'utilisateur est authentifi�.");
        }
    }

    #region Data saving/loading
    public void SaveDataFn()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("Impossible de sauvegarder les donn�es : userId est null ou vide.");
            return;
        }

        string json = JsonUtility.ToJson(dts);
        dbRef.Child("users").Child(userId).SetRawJsonValueAsync(json);
    }

    public void LoadDataFn()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("Impossible de charger les donn�es : userId est null ou vide.");
            return;
        }

        StartCoroutine(LoadDataEnum());
    }

    private IEnumerator LoadDataEnum()
    {
        Debug.Log("Tentative de r�cup�ration des donn�es depuis Firebase...");
        var serverData = dbRef.Child("users").Child(userId).GetValueAsync();
        yield return new WaitUntil(() => serverData.IsCompleted);

        if (serverData.Exception != null)
        {
            Debug.LogError($"Erreur lors de la r�cup�ration des donn�es : {serverData.Exception.Message}");
            yield break;
        }

        DataSnapshot snapshot = serverData.Result;
        string jsonData = snapshot.GetRawJsonValue();

        if (!string.IsNullOrEmpty(jsonData))
        {
            Debug.Log($"Donn�es r�cup�r�es : {jsonData}");
            dataToSave loadedData = JsonUtility.FromJson<dataToSave>(jsonData);

            // Remplacement des setters par des acc�s directs aux propri�t�s
            dts.userName = loadedData.userName;
            dts.totalCoins = loadedData.totalCoins;
            dts.totalJewels = loadedData.totalJewels;
            dts.crrLevel = loadedData.crrLevel;
            dts.crrLevelProgress = loadedData.crrLevelProgress;
            dts.totalLevelProgress = loadedData.totalLevelProgress;
            dts.unlockedClothes = loadedData.unlockedClothes;

            Debug.Log("Donn�es appliqu�es avec succ�s.");
        }
        else
        {
            Debug.LogWarning("Aucune donn�e trouv�e pour cet utilisateur.");
        }
    }
    #endregion


    #region To Use Functions
    public void addCoins(int coins)
    {
        int currentCoins = dts.totalCoins; // Acc�s direct � la propri�t�
        dts.totalCoins = currentCoins + coins; // Modification directe
        SaveDataFn();
    }

    public void removeCoins(int coins)
    {
        int currentCoins = dts.totalCoins;
        dts.totalCoins = currentCoins - coins;
        SaveDataFn();
    }

    public void addJewels(int jewels)
    {
        int currentJewels = dts.totalJewels;
        dts.totalJewels = currentJewels + jewels;
        SaveDataFn();
    }

    public void removeJewels(int jewels)
    {
        int currentJewels = dts.totalJewels;
        dts.totalJewels = currentJewels - jewels;
        SaveDataFn();
    }

    public void addLevelProgress(int levelProgress)
    {
        int currentTotalProgress = dts.totalLevelProgress;
        dts.totalLevelProgress = currentTotalProgress + levelProgress;
        CheckForLevelUp(levelProgress);
        SaveDataFn();
    }

    public void CheckForLevelUp(int levelProgress)
    {
        int currentLevelProgress = dts.crrLevelProgress;
        dts.crrLevelProgress = currentLevelProgress + levelProgress;

        if (dts.crrLevelProgress > dts.totalLevelProgress)
        {
            int currentLevel = dts.crrLevel;
            dts.crrLevel = currentLevel + 1;

            int overflowProgress = dts.crrLevelProgress - dts.totalLevelProgress;
            dts.crrLevelProgress = overflowProgress;
        }

        if (dts.crrLevelProgress == dts.totalLevelProgress)
        {
            int currentLevel = dts.crrLevel;
            dts.crrLevel = currentLevel + 1;
            dts.crrLevelProgress = 0;
        }

        SaveDataFn();
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

    #region Clothing Management
    public void AddItem(Item item)
    {
        dts.unlockedClothes.Add(item);
        SaveDataFn();
    }

    public List<Item> GetItems()
    {
        return dts.unlockedClothes;
    }
    #endregion
    /*public bool IsItemUnlocked(string itemId)
    {
        return dts.unlockedClothes.Contains(itemId);
    }*/
    #endregion
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
    public List<Item> unlockedClothes = new List<Item>(); // Remplacement par une liste d'Item
}


#endregion