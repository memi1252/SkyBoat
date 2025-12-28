using System.Collections;
using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public GameObject interactionTextGameObject;
    public TextMeshProUGUI text;
    public TextMeshProUGUI shadowText;

    public void Set(string newText)
    {
        interactionTextGameObject.SetActive(true);
        text.text = newText;
        shadowText.text = newText;
    }

    public void Hide()
    {
        interactionTextGameObject.SetActive(false);
    }
}
