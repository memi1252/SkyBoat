using UnityEngine;
using UnityEngine.SceneManagement;

public class gameOverUI : MonoBehaviour
{
    public void Lobby()
    {
        LoadingBar.LoadScene("Lobby");
    }

    public void ReStart()
    {
        LoadingBar.LoadScene("SampleScene");
    }
}
