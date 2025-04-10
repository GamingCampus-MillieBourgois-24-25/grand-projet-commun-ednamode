using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.Database;
using Firebase.Auth;
using TMPro;
using UnityEditor; // Import n�cessaire pour TextMeshPro

public class DataSaver : MonoBehaviour
{
    public dataToSave dts;
    public string userId;
    private DatabaseReference dbRef;
    private FirebaseAuth auth;

    [SerializeField]
    private TMP_Text dataDisplayText; // R�f�rence au composant Text pour afficher les donn�es

    private void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        if (auth.CurrentUser != null)
        {
            userId = auth.CurrentUser.UserId;
            Debug.Log($"Utilisateur connect� avec UID : {userId}");

            LoadDataFn(); // Charge les donn�es � l'initialisation
        }
        else
        {
            Debug.LogError("Aucun utilisateur connect�. Assurez-vous que l'utilisateur est authentifi�.");
        }
    }

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

            UpdateDataDisplay(); // Met � jour l'affichage des donn�es
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
                                   $"Total de pi�ces : {dts.totalCoins}\n" +
                                   $"Total de bijoux : {dts.totalJewels}\n" +
                                   $"Niveau actuel : {dts.crrLevel}\n" +
                                   $"Progression du niveau actuel : {dts.crrLevelProgress}/{dts.totalLevelProgress}";
        }
        else
        {
            Debug.LogError("Le composant Text pour afficher les donn�es n'est pas assign�.");
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
}


