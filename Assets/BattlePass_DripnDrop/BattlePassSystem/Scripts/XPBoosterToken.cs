using UnityEngine;
using UnityEngine.UI;


namespace EasyBattlePass
{
    [CreateAssetMenu(fileName = "XPBoosterToken", menuName = "EasyBattlePass/XPBoosterToken", order = 0)]
    public class XPBoosterToken : ScriptableObject
    {
        public string tokenName;
        public int xpMultiplier;
        public SimpleCurrencySystem.Currency tokenCost;
        public int boostHours;
        public int boostMinutes;
        public Sprite tokenIcon;
    }
}