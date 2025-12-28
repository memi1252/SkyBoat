using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuickSlot : MonoBehaviour
{
    private Image iconImage;
    private TextMeshProUGUI nameText;
    public bool isSet = false;
    [SerializeField] private float defaultHeight = 0;
    [SerializeField] private float defaultWidth = 0;

    public void Awake()
    {
        iconImage = GetComponent<Image>();
        nameText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        defaultWidth = iconImage.rectTransform.sizeDelta.x;
        defaultHeight = iconImage.rectTransform.sizeDelta.y;
    }

    public void Set(ItemInfo itemInfo)
    {
        if (itemInfo != null)
        {
            iconImage.sprite = itemInfo.itemIcon;
            nameText.text = itemInfo.itemName;
        }
        else
        {
            iconImage.sprite = null;
            nameText.text = "";
        }
    }

    public void ItemSet()
    {
        if (!isSet)
        {
            iconImage.rectTransform.sizeDelta = new Vector2(defaultWidth + 10, defaultHeight + 10);
            isSet = true;
        }
        else
        {
            iconImage.rectTransform.sizeDelta = new Vector2(defaultWidth, defaultHeight);
            isSet = false;
        }
    }
}
