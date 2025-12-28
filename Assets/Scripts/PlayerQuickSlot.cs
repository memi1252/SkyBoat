using System;
using System.Collections;
using StarterAssets;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerQuickSlot : MonoBehaviour
{
    public ItemInfo slot1;
    public ItemInfo slot2;
    public ItemInfo slot3;
    public ItemInfo slot4;
    
    public GameObject[] itemObjcts;
    public GameObject currentItem;

    public int currentSlot = -1;
    public bool equiping = false;
    public bool equipped = false;
    
    private ItemInfo currentItemInfo = null;
    private ThirdPersonController player;

    private void Start()
    {
        player = GetComponent<ThirdPersonController>();
    }

    public void SetQuickSlot(ItemInfo itemInfo)
    {
        if (slot1 == null)
        {
            slot1 = itemInfo;
        }else if (slot2 == null)
        {
            slot2 = itemInfo;
        }else if (slot3 == null)
        {
            slot3 = itemInfo;
        }else if (slot4 == null)
        {
            slot4 = itemInfo;
        }
    }

    public void SelectSlot(int slotNumber)
    {
        switch (slotNumber)
        {
            case 1:
                if (slot1 == null)
                {
                    return;
                }
                break;
            case 2:
                if (slot2 == null)
                {
                    return;
                }
                break;
            case 3:
                if (slot3 == null)
                {
                    return;
                }
                break;
            case 4:
                if (slot4 == null)
                {
                    return;
                }
                break;
        }
        currentSlot = slotNumber;
        StartCoroutine(EquipSlot(slotNumber));
    }

    IEnumerator EquipSlot(int slotNumber)
    {
        if (slotNumber == -1)
        {
            equipped = false;
            player.none = true;
            player.greatSword = false;
            StartCoroutine(itemHide(currentItem));
            yield break;
        }
        switch (slotNumber)
        {
            case 1:
                currentItemInfo = slot1;
                break;
            case 2:
                currentItemInfo = slot2;
                break;
            case 3:
                currentItemInfo = slot3;
                break;
            case 4:
                currentItemInfo = slot4;
                break;
        }
        equiping = true;
        player.none = currentItemInfo.none;
        player.greatSword = currentItemInfo.greatSword;
        if (equipped)
        {
            player._animator.SetTrigger("WeaponChange");
            yield return new WaitForSecondsRealtime(0.7f);
        }
        foreach (var item in itemObjcts)
        {
            if (item.name == currentItemInfo.itemCode)
            {
                currentItem = item;
                StartCoroutine(itemShow(item));
            }
            else
            {
                item.SetActive(false);
            }
        }
    }

    IEnumerator itemShow(GameObject item)
    {
        if (equipped)
        {
            yield return new WaitForSeconds(1f);
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }
        
        equiping = false;
        equipped = true;
        item.SetActive(true);
    }

    IEnumerator itemHide(GameObject item)
    {
        yield return new WaitForSeconds(0.6f);
        
        equiping = false;
        item.SetActive(false);
    }
    
    
    public void DropItem(int slotNumber)
    {
        switch (slotNumber)
        { 
            case 1:
                Instantiate(slot1.DropPrefab, transform.position + transform.forward + new Vector3(0,2,0), Quaternion.identity);
                slot1 = null;
                break;
            case 2:
                Instantiate(slot2.DropPrefab, transform.position + transform.forward + new Vector3(0,2,0), Quaternion.identity);
                slot2 = null;
                break;
            case 3:Instantiate(slot3.DropPrefab, transform.position + transform.forward + new Vector3(0,2,0), Quaternion.identity);
                slot3 = null;
                break;
            case 4:
                Instantiate(slot4.DropPrefab, transform.position + transform.forward + new Vector3(0,2,0), Quaternion.identity);
                slot4 = null;
                break;
        }
        foreach (var item in itemObjcts)
        {
            item.SetActive(false);
        }
        equiping = false;
        player.none =true;
        player.greatSword = false;
        currentSlot = -1;
        foreach (var item in itemObjcts)
        {
            item.SetActive(false);
        }
    }
}
