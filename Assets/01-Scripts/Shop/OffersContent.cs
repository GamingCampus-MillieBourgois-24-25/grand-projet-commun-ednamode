using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
public class OffersContent : MonoBehaviour
{
    List<ConvertCurrencyOffer> offers = new List<ConvertCurrencyOffer>();
    [SerializeField] private GameObject offerButton;
    private void Start()
    {
        // Load all ConvertCurrencyOffer assets from the Resources folder
        ConvertCurrencyOffer[] loadedOffers = Resources.LoadAll<ConvertCurrencyOffer>("Offers");
        foreach (ConvertCurrencyOffer offer in loadedOffers)
        {
            offerButton=Instantiate(offerButton, transform);
            offerButton.GetComponent<OfferButton>().SetScriptable(offer);
        }
    }
}
