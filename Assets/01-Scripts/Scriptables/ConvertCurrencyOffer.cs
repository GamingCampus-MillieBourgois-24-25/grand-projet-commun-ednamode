using UnityEngine;

[CreateAssetMenu(fileName = "ConvertCurrencyOffer", menuName = "Scriptable Objects/ConvertCurrencyOffer")]
public class ConvertCurrencyOffer : ScriptableObject
{
    private const string _path = "Resources/Offers/";
    public string offerId;
    public int jewelsToSpend;
    public int coinsToObtain;
}
