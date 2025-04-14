using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Scriptable Objects/Character")]
public class Character : ScriptableObject
{
    private const string _path = "Resources/Character/";
    public string characterName;
    public GameObject bodyType;

}
