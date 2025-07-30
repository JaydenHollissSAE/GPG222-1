using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyButtons : MonoBehaviour
{
    private void Start()
    {
        Cursor.visible = true;
    }
    public void JoinGameMode(string modeName)
    {
        SceneManager.LoadScene(modeName);
    }

}
