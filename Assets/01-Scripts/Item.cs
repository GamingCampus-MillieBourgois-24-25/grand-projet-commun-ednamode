using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class Item : ScriptableObject
{
    private const string _path = "Resources/Items/";
    public string itemName;
    public Sprite icon;
    public GameObject prefab;
    public int price;
    public enum ItemType
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
    public ItemType category;

}
