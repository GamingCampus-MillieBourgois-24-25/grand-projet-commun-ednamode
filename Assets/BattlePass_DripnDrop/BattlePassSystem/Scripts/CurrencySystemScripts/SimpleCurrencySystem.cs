using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyBattlePass
{
    public class SimpleCurrencySystem : MonoBehaviour
    {
        [System.Serializable]
        public class Currency
        {
            public string name;           // Par exemple : "Currency_Gem" ou "Currency_Gold"
            public int amount;            // Montant actuel
            public int defaultAmount;     // Valeur par défaut (non utilisée ici, mais conservée si besoin)
            public Sprite icon;
        }

        public List<Currency> currencies;
        private const string SAVE_KEY = "CURRENCY_AMOUNTS";

        private void Start()
        {
            // Au lancement, si aucune donnée sauvegardée n'existe,
            // on initialise toutes les devises à 250.
            if (!PlayerPrefs.HasKey(SAVE_KEY))
            {
                InitializeCurrencies();
                SaveCurrencies();
            }
            else
            {
                LoadCurrencies();
            }
        }

        // Initialise toutes les devises à 250
        private void InitializeCurrencies()
        {
            for (int i = 0; i < currencies.Count; i++)
            {
                currencies[i].amount = 250;
            }
        }

        public void SetCurrency(string currencyName, int amount)
        {
            for (int i = 0; i < currencies.Count; i++)
            {
                if (currencies[i].name == currencyName)
                {
                    currencies[i].amount = amount;
                    SaveCurrencies();
                    return;
                }
            }
            Debug.LogError("Currency with name " + currencyName + " not found.");
        }

        public void RewardCurrency(string currencyName, int amount)
        {
            for (int i = 0; i < currencies.Count; i++)
            {
                if (currencies[i].name == currencyName)
                {
                    currencies[i].amount += amount;
                    SaveCurrencies();
                    return;
                }
            }
            Debug.LogError("Currency with name " + currencyName + " not found.");
        }

        public bool SpendCurrency(string currencyName, int amount)
        {
            for (int i = 0; i < currencies.Count; i++)
            {
                if (currencies[i].name == currencyName)
                {
                    if (currencies[i].amount >= amount)
                    {
                        currencies[i].amount -= amount;
                        SaveCurrencies();
                        return true;
                    }
                    else
                    {
                        Debug.LogError("Not enough " + currencyName + " to spend.");
                        return false;
                    }
                }
            }
            Debug.LogError("Currency with name " + currencyName + " not found.");
            return false;
        }

        public int GetCurrency(string currencyName)
        {
            for (int i = 0; i < currencies.Count; i++)
            {
                if (currencies[i].name == currencyName)
                {
                    return currencies[i].amount;
                }
            }
            return 0;
        }

        private void SaveCurrencies()
        {
            string currencyData = "";
            foreach (Currency currency in currencies)
            {
                currencyData += currency.name + "," + currency.amount + ";";
            }
            EncryptionManager.Save(SAVE_KEY, currencyData);
        }

        private void LoadCurrencies()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string currencyData = EncryptionManager.Load<string>(SAVE_KEY);
                string[] currencyEntries = currencyData.Split(';');
                foreach (string entry in currencyEntries)
                {
                    if (!string.IsNullOrEmpty(entry))
                    {
                        string[] currencyDataArray = entry.Split(',');
                        string currencyName = currencyDataArray[0];
                        int currencyAmount = int.Parse(currencyDataArray[1]);
                        for (int i = 0; i < currencies.Count; i++)
                        {
                            if (currencies[i].name == currencyName)
                            {
                                currencies[i].amount = currencyAmount;
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Réinitialise toutes les devises à 250.
        /// </summary>
        public void ResetCurrencies()
        {
            for (int i = 0; i < currencies.Count; i++)
            {
                currencies[i].amount = 250;
            }
            SaveCurrencies();
        }
    }
}
