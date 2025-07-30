using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyButtons : MonoBehaviour
{
    public void JoinGameMode(string modeName)
    {
        SceneManager.LoadScene(modeName);
    }

}
