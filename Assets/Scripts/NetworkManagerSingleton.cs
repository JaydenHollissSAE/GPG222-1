using UnityEngine;

public class NetworkManagerSingleton : MonoBehaviour
{
    public static NetworkManagerSingleton instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
}
