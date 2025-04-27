using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EasyBattlePass
{
    public class DisplayCurrencyUI : MonoBehaviour
    {
        [Tooltip("Mettre \"Coins\" ou \"Gems\" selon l��l�ment � afficher")]
        public string currencyName;
        public TMP_Text displayText;

        void Update()
        {
            // Si le DataSaver n'est pas encore initialis�, ou si le TMP_Text n'est pas branch�, on sort
            if (DataSaver.Instance == null || displayText == null)
                return;

            int value = currencyName == "Coins"
                ? DataSaver.Instance.dts.totalCoins
                : DataSaver.Instance.dts.totalJewels;

            displayText.text = value.ToString();
        }
    }

}
