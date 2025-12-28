using System;
using UnityEngine;

public class boatIn : MonoBehaviour
{
    [Header("Shop System")]
    public ShopUI shopUI; // 상점 UI 참조
    
    private bool playerInBoat = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            playerInBoat = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;

            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            playerInBoat = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            // 상점이 열려있다면 닫기
            if (shopUI != null && shopUI.shopPanel != null && shopUI.shopPanel.activeSelf)
            {
                shopUI.CloseShop();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            
        }
    }
    
    private void Update()
    {
        // 보트 안에서만 상점 열기 가능
        if (playerInBoat && Input.GetKeyDown(KeyCode.G))
        {
            if (shopUI != null)
            {
                if (shopUI.shopPanel != null && shopUI.shopPanel.activeSelf)
                    shopUI.CloseShop();
                else
                    shopUI.OpenShop();
            }
        }
    }
}
