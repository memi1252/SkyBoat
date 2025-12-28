using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Item Info")]
public class ItemInfo : ScriptableObject
{
    public string itemCode;
    public string itemName;
    public Sprite itemIcon;
    public GameObject DropPrefab;
    public bool none;
    public bool greatSword;
    public int sellPrice = 100; // 판매 가격 추가
}
