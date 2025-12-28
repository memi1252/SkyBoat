using TMPro;
using UnityEngine;

public class errorLogUI : MonoBehaviour
{
    public GameObject errorLogPrefab;
    public Transform errorLogParent;
    
    public void CreateErrorLog(string message)
    {
        GameObject errorLogInstance = Instantiate(errorLogPrefab, errorLogParent);
        TextMeshProUGUI logComponent = errorLogInstance.GetComponent<TextMeshProUGUI>();
        if (logComponent != null)
        {
            logComponent.text = message;
        }
        Destroy(errorLogInstance, 5);
    }
}
