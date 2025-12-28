using UnityEngine;

public class QuickSlotUI : MonoBehaviour
{
    public QuickSlot quickSlot1;
    public QuickSlot quickSlot2;
    public QuickSlot quickSlot3;
    public QuickSlot quickSlot4;

    public PlayerQuickSlot _PlayerQuickSlot;
    
    public void Update()
    {
        quickSlot1.Set(_PlayerQuickSlot.slot1);
        quickSlot2.Set(_PlayerQuickSlot.slot2);
        quickSlot3.Set(_PlayerQuickSlot.slot3);
        quickSlot4.Set(_PlayerQuickSlot.slot4);
    }
    
    public void SetQuickSlotUI(int index)
    {
        switch (index)
        {
            case 1:
                quickSlot1.ItemSet();
                break;
            case 2:
                quickSlot2.ItemSet();
                break;
            case 3:
                quickSlot3.ItemSet();
                break;
            case 4:
                quickSlot4.ItemSet();
                break;
        }
    }
}
