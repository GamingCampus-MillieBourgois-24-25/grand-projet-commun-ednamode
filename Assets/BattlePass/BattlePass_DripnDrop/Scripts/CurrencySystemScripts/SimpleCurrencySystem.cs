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
            public string name;
            public int amount;
            public Sprite icon;
        }

        public List<Currency> currencies;

        private const string SAVE_KEY = "CURRENCY_AMOUNTS";

        private void Start()
        {
            // load your saved currencies
            LoadCurrencies();
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
            //PlayerPrefs.SetString(SAVE_KEY, currencyData);
            EncryptionManager.Save(SAVE_KEY, currencyData);
        }

        private void LoadCurrencies()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string currencyData = EncryptionManager.Load<string>(SAVE_KEY); //PlayerPrefs.GetString(SAVE_KEY);
                string[] currencyEntries = currencyData.Split(';');
                foreach (string entry in currencyEntries)
                {
                    if (entry != "")
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
    }
}