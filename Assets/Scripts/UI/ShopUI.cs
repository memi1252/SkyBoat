using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject shopPanel;
    public Transform itemListParent;
    public GameObject itemSlotPrefab;
    public TextMeshProUGUI totalMoneyText;
    public Button closeButton;
    
    [Header("Player References")]
    public PlayerQuickSlot playerQuickSlot;
    
    [Header("Shop Settings")]
    public int playerMoney = 1000; // 플레이어 보유 금액
    
    private List<GameObject> currentItemSlots = new List<GameObject>();
    
    void Start()
    {
        // 상점 패널 초기화
        if (shopPanel != null)
            shopPanel.SetActive(false);
            
        // 닫기 버튼 이벤트 연결
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);
            
        UpdateMoneyDisplay();
    }

    void Update()
    {
        // B키를 눌러서 상점 열기/닫기
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (shopPanel != null)
            {
                if (shopPanel.activeSelf)
                    CloseShop();
                else
                    OpenShop();
            }
        }
    }
    
    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            RefreshItemList();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
    
    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    
    public void RefreshItemList()
    {
        // 기존 아이템 슬롯들 제거
        foreach (GameObject slot in currentItemSlots)
        {
            if (slot != null)
                DestroyImmediate(slot);
        }
        currentItemSlots.Clear();
        
        // 플레이어가 가진 아이템들을 상점에 표시
        List<ItemInfo> playerItems = GetPlayerItems();
        
        foreach (ItemInfo item in playerItems)
        {
            CreateItemSlot(item);
        }
    }
    
    private List<ItemInfo> GetPlayerItems()
    {
        List<ItemInfo> items = new List<ItemInfo>();
        
        if (playerQuickSlot != null)
        {
            if (playerQuickSlot.slot1 != null) items.Add(playerQuickSlot.slot1);
            if (playerQuickSlot.slot2 != null) items.Add(playerQuickSlot.slot2);
            if (playerQuickSlot.slot3 != null) items.Add(playerQuickSlot.slot3);
            if (playerQuickSlot.slot4 != null) items.Add(playerQuickSlot.slot4);
        }
        
        return items;
    }
    
    private void CreateItemSlot(ItemInfo item)
    {
        if (itemSlotPrefab == null || itemListParent == null) return;
        
        GameObject newSlot = Instantiate(itemSlotPrefab, itemListParent);
        currentItemSlots.Add(newSlot);
        
        // 아이템 정보 설정
        Image itemIcon = newSlot.transform.Find("ItemIcon")?.GetComponent<Image>();
        TextMeshProUGUI itemName = newSlot.transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI itemPrice = newSlot.transform.Find("ItemPrice")?.GetComponent<TextMeshProUGUI>();
        Button sellButton = newSlot.transform.Find("SellButton")?.GetComponent<Button>();
        
        if (itemIcon != null && item.itemIcon != null)
            itemIcon.sprite = item.itemIcon;
            
        if (itemName != null)
            itemName.text = item.itemName;
            
        if (itemPrice != null)
            itemPrice.text = $"판매가: {item.sellPrice}G";
            
        if (sellButton != null)
        {
            sellButton.onClick.AddListener(() => SellItem(item));
        }
    }
    
    public void SellItem(ItemInfo item)
    {
        if (item == null || playerQuickSlot == null) return;
        
        // 플레이어 슬롯에서 해당 아이템 제거
        bool itemRemoved = false;
        
        if (playerQuickSlot.slot1 == item && !itemRemoved)
        {
            playerQuickSlot.slot1 = null;
            itemRemoved = true;
        }
        else if (playerQuickSlot.slot2 == item && !itemRemoved)
        {
            playerQuickSlot.slot2 = null;
            itemRemoved = true;
        }
        else if (playerQuickSlot.slot3 == item && !itemRemoved)
        {
            playerQuickSlot.slot3 = null;
            itemRemoved = true;
        }
        else if (playerQuickSlot.slot4 == item && !itemRemoved)
        {
            playerQuickSlot.slot4 = null;
            itemRemoved = true;
        }
        
        if (itemRemoved)
        {
            // 돈 추가
            playerMoney += item.sellPrice;
            UpdateMoneyDisplay();
            
            // 현재 장착중인 아이템이라면 해제
            if (playerQuickSlot.currentSlot != -1)
            {
                ItemInfo currentItem = GetCurrentEquippedItem();
                if (currentItem == item)
                {
                    playerQuickSlot.SelectSlot(-1); // 아이템 해제
                }
            }
            
            // 아이템 리스트 새로고침
            RefreshItemList();
            
            Debug.Log($"{item.itemName}을(를) {item.sellPrice}G에 판매했습니다!");
        }
    }
    
    private ItemInfo GetCurrentEquippedItem()
    {
        if (playerQuickSlot == null) return null;
        
        switch (playerQuickSlot.currentSlot)
        {
            case 1: return playerQuickSlot.slot1;
            case 2: return playerQuickSlot.slot2;
            case 3: return playerQuickSlot.slot3;
            case 4: return playerQuickSlot.slot4;
            default: return null;
        }
    }
    
    private void UpdateMoneyDisplay()
    {
        if (totalMoneyText != null)
        {
            totalMoneyText.text = $"보유 금액: {playerMoney}G";
        }
    }
    
    // 외부에서 아이템 정보를 설정할 수 있는 메서드
    public void AddItemToShop(ItemInfo newItem)
    {
        if (newItem != null && playerQuickSlot != null)
        {
            playerQuickSlot.SetQuickSlot(newItem);
            if (shopPanel != null && shopPanel.activeSelf)
            {
                RefreshItemList();
            }
        }
    }
    
    // 외부에서 돈을 설정할 수 있는 메서드
    public void SetPlayerMoney(int amount)
    {
        playerMoney = amount;
        UpdateMoneyDisplay();
    }
    
    // 현재 보유 금액 반환
    public int GetPlayerMoney()
    {
        return playerMoney;
    }
}
