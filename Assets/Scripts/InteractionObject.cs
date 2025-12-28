using UnityEngine;

public class InteractionObject : MonoBehaviour
{
    public string interactionText;


    public virtual void Use()
    {
        Debug.Log("시용");
    }
}
