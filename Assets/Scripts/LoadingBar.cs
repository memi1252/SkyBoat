using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingBar : MonoBehaviour
{
    public static string nextScene;

    [SerializeField] Slider progressBar;
    [SerializeField] private TextMeshProUGUI lodingtect;
    
    void Start()
    {
        StartCoroutine(LoadScene());
        StartCoroutine(loding1());
        Time.timeScale = 1;
    }
    

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("Lobbing");
    }
    IEnumerator LoadScene()
    {
        yield return null;
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;
        float timer = 0.0f;
        while (!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;
            if (op.progress < 0.9f)
            {
                progressBar.value = Mathf.Lerp(progressBar.value, op.progress, timer);
                if (progressBar.value >= op.progress)
                {
                    timer = 0f;
                }
            }
            else
            {
                progressBar.value = Mathf.Lerp(progressBar.value, 1f, timer);
                if (progressBar.value == 1.0f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
    
    IEnumerator loding1()
    {
        yield return new WaitForSeconds(0.2f);
        lodingtect.text = "로딩중.";
        StartCoroutine(loding2());
    }
    
    IEnumerator loding2()
    {
        yield return new WaitForSeconds(0.2f);
        lodingtect.text = "로딩중..";
        StartCoroutine(loding3());
    }
    
    IEnumerator loding3()
    {
        yield return new WaitForSeconds(0.2f);
        lodingtect.text = "로딩중...";
        StartCoroutine(loding1());
    }
}