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
            if (DataSaver.Instance == null) return;

            int value = currencyName == "Coins"
                ? DataSaver.Instance.dts.totalCoins
                : DataSaver.Instance.dts.totalJewels;

            displayText.text = value.ToString();
        }
    }

}
