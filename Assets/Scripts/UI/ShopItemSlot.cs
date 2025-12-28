using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemSlot : MonoBehaviour
{
    [Header("UI Components")]
    public Image ItemIcon;
    public TextMeshProUGUI ItemName;
    public TextMeshProUGUI ItemPrice;
    public Button SellButton;
    
    private ItemInfo itemInfo;
    private ShopUI shopUI;
    
    public void Initialize(ItemInfo item, ShopUI shop)
    {
        itemInfo = item;
        shopUI = shop;
        
        if (ItemIcon != null && item.itemIcon != null)
            ItemIcon.sprite = item.itemIcon;
            
        if (ItemName != null)
            ItemName.text = item.itemName;
            
        if (ItemPrice != null)
            ItemPrice.text = $"판매가: {item.sellPrice}G";
            
        if (SellButton != null)
        {
            SellButton.onClick.RemoveAllListeners();
            SellButton.onClick.AddListener(() => shopUI.SellItem(itemInfo));
        }
    }
}
