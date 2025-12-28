using UnityEngine;

public class UIManager : MonoSingleton<UIManager>
{
    public InteractionUI interactionUI;
    public errorLogUI errorLogUI;
    public QuickSlotUI quickSlotUI;
    public bool LookInteractObject = false;
    public override void Awake()
    {
        base.Awake();
    }
}
