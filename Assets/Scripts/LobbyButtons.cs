using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyButtons : MonoBehaviour
{
    public void JoinPong()
    {
        SceneManager.LoadScene("Pong");
    }
    public void JoinFreeDraw()
    {
        SceneManager.LoadScene("FreeDraw");
    }
}
