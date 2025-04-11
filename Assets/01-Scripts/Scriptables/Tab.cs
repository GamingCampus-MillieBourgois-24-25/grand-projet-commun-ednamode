using System.Collections.Generic;
using UnityEngine;
namespace CharacterCustomization
{ 
[CreateAssetMenu(fileName = "NewTab", menuName = "Scriptable Objects/Tab")]

public class Tab : ScriptableObject
{
    private const string _path = "Resources/Tabs/";

    [Header("Tab Properties")]
    public SlotType category; 

    [Header("Items in Tab")]
    public List<Item> items = new List<Item>();
}
}