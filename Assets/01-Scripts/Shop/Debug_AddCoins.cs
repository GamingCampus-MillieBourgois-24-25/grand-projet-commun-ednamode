using UnityEngine;
using UnityEngine.UI;

public class Debug_AddCoins : MonoBehaviour
{
    private DataSaver dataSaver;
    private Button button;
    private void Start()
    {
        dataSaver = DataSaver.Instance;

       button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => dataSaver.addJewels(1000));
        }
        else
        {
            Debug.LogWarning("Aucun composant Button trouvé sur cet objet !");
        }
    }
}
