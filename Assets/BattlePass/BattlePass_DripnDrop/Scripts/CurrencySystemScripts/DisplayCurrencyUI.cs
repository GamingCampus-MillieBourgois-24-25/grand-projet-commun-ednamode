using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EasyBattlePass
{
    public class DisplayCurrencyUI : MonoBehaviour
    {
        [Tooltip("Mettre \"Coins\" ou \"Gems\" selon l’élément à afficher")]
        public string currencyName;
        public TMP_Text displayText;

        void Update()
        {
            // Si le DataSaver n'est pas encore initialisé, ou si le TMP_Text n'est pas branché, on sort
            if (DataSaver.Instance == null || displayText == null)
                return;

            int value = currencyName == "Coins"
                ? DataSaver.Instance.dts.totalCoins
                : DataSaver.Instance.dts.totalJewels;

            displayText.text = value.ToString();
        }
    }

}
