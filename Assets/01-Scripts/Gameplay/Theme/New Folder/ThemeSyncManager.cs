using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class ThemeSyncManager : NetworkBehaviour
{
    public static ThemeSyncManager Instance { get; private set; }

    [Tooltip("Thème actuellement sélectionné pour la session.")]
    private NetworkVariable<FixedString64Bytes> currentTheme = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public string CurrentTheme => currentTheme.Value.ToString();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetTheme(string theme)
    {
        if (IsServer)
        {
            currentTheme.Value = new FixedString64Bytes(theme);
        }
    }
}
