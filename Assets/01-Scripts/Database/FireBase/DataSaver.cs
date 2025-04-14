using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.Database;
using Firebase.Auth;
using TMPro;
using UnityEditor;
using Firebase; // Import nécessaire pour TextMeshPro

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
    private TMP_Text dataDisplayText; // Référence au composant Text pour afficher les données
    
    #region UserId Management
    public string GetUserId()
    {
        return userId;
    }

    public void SetUserId(string newUserId)
    {
        userId = newUserId;
    }
    #endregion

    private void Awake()
    {
        // Implémentation du Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Rend l'objet persistant entre les scènes
        }
        else
        {
            Destroy(gameObject); // Détruit les doublons
            return;
        }

        // Initialisation Firebase
        AppOptions options = AppOptions.LoadFromJsonConfig(jsonConfig);
        FirebaseApp app = FirebaseApp.Create(options, "DripOrDrop");
        auth = FirebaseAuth.GetAuth(app);

        dbRef = FirebaseDatabase.GetInstance(app).RootReference; // Utilise l'instance spécifique de FirebaseApp

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

    #region Data saving/loading
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

            if (dataDisplayText != null)
            {
                UpdateAccountDataDisplay(); // Met à jour l'affichage des données
            }
        }
        else
        {
            print("no data found");
        }
    }
    #endregion

    public void UpdateAccountDataDisplay()
    {
        if (dataDisplayText != null)
        {
            dataDisplayText.text = $"Nom d'utilisateur : {dts.GetUserName()}\n" +
                                   $"Total de pièces : {dts.GetTotalCoins()}\n" +
                                   $"Total de bijoux : {dts.GetTotalJewels()}\n" +
                                   $"Niveau actuel : {dts.GetCrrLevel()}\n" +
                                   $"Progression du niveau actuel : {dts.GetCrrLevelProgress()}/{dts.GetTotalLevelProgress()}";
        }
        else
        {
            Debug.LogError("Le composant Text pour afficher les données n'est pas assigné.");
        }
    }

#region To Use Functions
    public void addCoins(int coins)
    {
        int currentCoins = dts.GetTotalCoins();
        dts.SetTotalCoins(currentCoins + coins);
        SaveDataFn();
    }

    public void removeCoins(int coins)
    {
        int currentCoins = dts.GetTotalCoins();
        dts.SetTotalCoins(currentCoins - coins);
        SaveDataFn();
    }

    public void addJewels(int jewels)
    {
        int currentJewels = dts.GetTotalJewels();
        dts.SetTotalJewels(currentJewels + jewels);
        SaveDataFn();
    }

    public void removeJewels(int jewels)
    {
        int currentJewels = dts.GetTotalJewels();
        dts.SetTotalJewels(currentJewels - jewels);
        SaveDataFn();
    }

    public void addLevelProgress(int levelProgress)
    {
        int currentTotalProgress = dts.GetTotalLevelProgress();
        dts.SetTotalLevelProgress(currentTotalProgress + levelProgress);
        CheckForLevelUp(levelProgress);
        SaveDataFn();
    }

    public void CheckForLevelUp(int levelProgress)
    {
        int currentLevelProgress = dts.GetCrrLevelProgress();
        dts.SetCrrLevelProgress(currentLevelProgress + levelProgress);

        if (dts.GetCrrLevelProgress() > dts.GetTotalLevelProgress())
        {
            int currentLevel = dts.GetCrrLevel();
            dts.SetCrrLevel(currentLevel + 1);

            int overflowProgress = dts.GetCrrLevelProgress() - dts.GetTotalLevelProgress();
            dts.SetCrrLevelProgress(overflowProgress);
        }

        if (dts.GetCrrLevelProgress() == dts.GetTotalLevelProgress())
        {
            int currentLevel = dts.GetCrrLevel();
            dts.SetCrrLevel(currentLevel + 1);
            dts.SetCrrLevelProgress(0);
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
    #endregion

    #region Clothing Management
    public void UnlockClothingItem(string itemId)
    {
        if (!dts.GetUnlockedClothes().Contains(itemId))
        {
            dts.GetUnlockedClothes().Add(itemId);
            SaveDataFn();
            Debug.Log($"Item débloqué : {itemId}");
        }
        else
        {
            Debug.Log($"Item déjà débloqué : {itemId}");
        }
    }

    public bool IsItemUnlocked(string itemId)
    {
        return dts.GetUnlockedClothes().Contains(itemId);
    }
    #endregion
}

[Serializable]
public class dataToSave
{
    private string _userName;
    private int _totalCoins;
    private int _totalJewels;
    private int _crrLevel;
    private int _crrLevelProgress;
    private int _totalLevelProgress;
    private List<string> _unlockedClothes = new List<string>();

    #region Get Data
    public string GetUserName()
    {
        return _userName;
    }

    public void SetUserName(string userName)
    {
        _userName = userName;
    }

    public int GetTotalCoins()
    {
        return _totalCoins;
    }

    public void SetTotalCoins(int totalCoins)
    {
        _totalCoins = totalCoins;
    }

    public int GetTotalJewels()
    {
        return _totalJewels;
    }

    public void SetTotalJewels(int totalJewels)
    {
        _totalJewels = totalJewels;
    }

    public int GetCrrLevel()
    {
        return _crrLevel;
    }

    public void SetCrrLevel(int crrLevel)
    {
        _crrLevel = crrLevel;
    }

    public int GetCrrLevelProgress()
    {
        return _crrLevelProgress;
    }

    public void SetCrrLevelProgress(int crrLevelProgress)
    {
        _crrLevelProgress = crrLevelProgress;
    }

    public int GetTotalLevelProgress()
    {
        return _totalLevelProgress;
    }

    public void SetTotalLevelProgress(int totalLevelProgress)
    {
        _totalLevelProgress = totalLevelProgress;
    }

    public List<string> GetUnlockedClothes()
    {
        return _unlockedClothes;
    }

    public void SetUnlockedClothes(List<string> unlockedClothes)
    {
        _unlockedClothes = unlockedClothes;
    }


    #endregion
}