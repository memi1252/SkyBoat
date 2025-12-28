using System;
using Unity.VisualScripting;
using UnityEngine;

public class BoatDoor : InteractionObject
{
    public Door door;
    public string doorOpneText = "문 열기";
    public string doorCloseText = "문 닫기";


    private void Update()
    {
        if (door.isOpen)
        {
            if (interactionText != doorCloseText)
            {
                interactionText = doorCloseText;
            }
        }
        else
        {
            if (interactionText != doorOpneText)
            {
                interactionText = doorOpneText;
            }
        }
    }

    public override void Use()
    {
        base.Use();
        door.isOpen= !door.isOpen;
        door.Set();
    }
}
