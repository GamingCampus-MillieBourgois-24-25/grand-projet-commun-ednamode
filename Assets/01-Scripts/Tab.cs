using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTab", menuName = "Scriptable Objects/Tab")]
public class Tab : ScriptableObject
{
    private const string _path = "Resources/Tabs/";
    public Sprite tabIcon;
    public enum TabCategory
    {
        Hat,
        Sweater,
        Top,
        Pants,
        Shoes,
        Skirt,
        Dress,
        Coat,
        Accessory
    }

    [Header("Tab Properties")]
    public string tabName; 
    public TabCategory category; 

    [Header("Items in Tab")]
    public List<Item> items = new List<Item>();
}
