using Firebase;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using System;
using System.Threading.Tasks;
using UnityEngine;



public class remoteConfigScript : MonoBehaviour
{
    public ConfigData allConfigData;
    private void Start()
    {
        print("json:" + JsonUtility.ToJson(allConfigData));
        CheckRemoteConfigValues();
    }

    public Task CheckRemoteConfigValues()
    {
        Debug.Log("Fetching data...");
        
        FirebaseApp app = FirebaseApp.GetInstance("DripOrDrop");
        FirebaseRemoteConfig config = FirebaseRemoteConfig.GetInstance(app);

        Task fetchTask = config.FetchAsync(TimeSpan.Zero);
        return fetchTask.ContinueWithOnMainThread(FetchComplete);
    }
    private void FetchComplete(Task fetchTask)
    {
        if (!fetchTask.IsCompleted)
        {
            Debug.LogError("Retrieval hasn't finished.");
            return;
        }

        FirebaseApp app = FirebaseApp.GetInstance("DripOrDrop");


        FirebaseRemoteConfig remoteConfig = FirebaseRemoteConfig.GetInstance(app);
        var info = remoteConfig.Info;
        if (info.LastFetchStatus != LastFetchStatus.Success)
        {
            Debug.LogError($"{nameof(FetchComplete)} was unsuccessful\n{nameof(info.LastFetchStatus)}: {info.LastFetchStatus}");
            return;
        }

        // Fetch successful. Parameter values must be activated to use.
        remoteConfig.ActivateAsync()
          .ContinueWithOnMainThread(
            task => {
                Debug.Log($"Remote data loaded and ready for use. Last fetch time {info.FetchTime}.");

                string configData = remoteConfig.GetValue("all_Game_data").StringValue;
                allConfigData = JsonUtility.FromJson<ConfigData>(configData);

                /*  print("Total values: "+remoteConfig.AllValues.Count);

                  foreach (var item in remoteConfig.AllValues)
                  {
                      print("Key :" + item.Key);
                      print("Value: " + item.Value.StringValue);
                  }*/

            });
    }

}

[Serializable]
public class ConfigData
{
    public string playerName;
    public float gameVersion;
    public int crrLevel;
    public bool over18;
}