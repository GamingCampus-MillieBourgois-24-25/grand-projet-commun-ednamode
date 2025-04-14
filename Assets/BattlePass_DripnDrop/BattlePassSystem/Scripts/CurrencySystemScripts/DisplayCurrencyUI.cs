using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EasyBattlePass
{
    public class DisplayCurrencyUI : MonoBehaviour
    {
        // This just updates the currencies amount on the UI
        public TMP_Text displayText;
        public string currencyName;
        public SimpleCurrencySystem currencySystem;

        private void Start()
        {
            displayText.text = currencySystem.GetCurrency(currencyName).ToString();
        }

        private void Update()
        {
            displayText.text = currencySystem.GetCurrency(currencyName).ToString();
        }
    }
}


