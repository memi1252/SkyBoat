using System;
using UnityEngine;
using UnityEngine.Video;

public class end : MonoBehaviour
{
    public VideoPlayer vi;

    private void Awake()
    {
        vi = GetComponent<VideoPlayer>();
        vi.loopPointReached += ViOnloopPointReached; 
    }

    private void ViOnloopPointReached(VideoPlayer source)
    {
        LoadingBar.LoadScene("Lobby");
    }
}
