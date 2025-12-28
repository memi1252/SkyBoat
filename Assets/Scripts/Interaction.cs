using System;
using StarterAssets;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    public float range;
    public StarterAssetsInputs _input;

    private void Update()
    {
        // 화면 중앙 좌표 계산
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        
        
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * range, Color.red);
        if (Physics.Raycast(ray, out hit, range, LayerMask.GetMask("Interaction")))
        {
            InteractionObject InObject = hit.collider.GetComponent<InteractionObject>();
            if (_input.interaction)
            {
                InObject.Use();
                _input.interaction = false;
            }
            string text = "(F)키를 눌러 " + InObject.interactionText;
            
            UIManager.Instance.interactionUI.Set(text);
            UIManager.Instance.LookInteractObject = true;
        }
        else
        {
            if (UIManager.Instance.LookInteractObject)
            {
                UIManager.Instance.LookInteractObject = false;
                UIManager.Instance.interactionUI.Hide();
            }
            
        }
    }
}
