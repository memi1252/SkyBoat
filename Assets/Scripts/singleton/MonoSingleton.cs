using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour  where T : MonoBehaviour
{
    public static T Instance {get; private set;}

    public virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
