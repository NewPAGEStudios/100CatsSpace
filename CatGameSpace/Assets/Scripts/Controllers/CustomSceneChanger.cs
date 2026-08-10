using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneChanger : MonoBehaviour
{
    private static CustomSceneChanger _instance;
    public static CustomSceneChanger Instance
    {
        get { return _instance; }
    }


    private InputManager inputManager;
    
    public Transform sprMask;
    public SpriteRenderer spriteRenderer;
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            _instance = this;
        }

        SceneManager.sceneLoaded += OnSceneLoad;
        DontDestroyOnLoad(gameObject);
        OpenAnim(0.75f, 0.75f);
    }

    private void OnSceneLoad(Scene LoadedScene, LoadSceneMode arg1)
    {
        if(LoadedScene.name == "Menu")
        {
            StartCoroutine(OpenAnim(0.75f, 1.25f));
            spriteRenderer.transform.localScale = new Vector2(10, 10);
        }
        else if (LoadedScene.name == "LobbyScene")
        {
            StartCoroutine(OpenAnim(0.75f, 1.25f));
            spriteRenderer.transform.localScale = new Vector2(10, 10);
        }
        else if (LoadedScene.name == "OfflineGamePlay")
        {
            StartCoroutine(OpenAnim(1.25f, 4.8f));
            spriteRenderer.transform.localScale = new Vector2(90, 90);
        }
        else if (LoadedScene.name == "OnlineGamePlay")
        {
            StartCoroutine(OpenAnim(1.25f, 4.8f));
            spriteRenderer.transform.localScale = new Vector2(90, 90);
        }
        else if (LoadedScene.name == "TutScene")
        {
            StartCoroutine(OpenAnim(0.75f, 1.25f));
            spriteRenderer.transform.localScale = new Vector2(90, 90);
        }
    }

    public void ChangeScene(string sceneName)
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            StartCoroutine(CloseAnim(sceneName, 0.75f, 1.25f));
        }
        else if (SceneManager.GetActiveScene().name == "LobbyScene")
        {
            StartCoroutine(CloseAnim(sceneName, 0.75f, 1.25f));
        }
        else if (SceneManager.GetActiveScene().name == "OfflineGamePlay")
        {
            StartCoroutine(CloseAnim(sceneName, 1.75f, 4.8f));
        }
        else if (SceneManager.GetActiveScene().name == "OnlineGamePlay")
        {
            StartCoroutine(CloseAnim(sceneName, 1.25f, 4.8f));
        }
        else if (SceneManager.GetActiveScene().name == "TutScene")
        {
            StartCoroutine(CloseAnim(sceneName, 0.75f, 1.25f));
            spriteRenderer.transform.localScale = new Vector2(90, 90);
        }

    }
    public void ChangeSceneCNM(CustomNetworkManager manager, string sceneName)
    {
        StartCoroutine(CloseAnimCNM(manager, sceneName, .75f, .75f));
    }

    IEnumerator CloseAnim(string sceneName, float target, float speed)
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        InputManager.Instance.stopInput();
        yield return null;

        sprMask.localScale = target * Vector3.one;

        while (true)
        {
            sprMask.localScale = Vector3.MoveTowards(sprMask.localScale, Vector3.zero, speed * Time.deltaTime);

            yield return null;
            if (sprMask.localScale.x <= 0) break;
        }


        InputManager.Instance.ContInput();
        SceneManager.LoadScene(sceneName);
    }
    IEnumerator CloseAnimCNM(CustomNetworkManager manager, string sceneName, float target, float speed)
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        InputManager.Instance.stopInput();
        yield return null;

        sprMask.localScale = target * Vector3.one;

        while (true)
        {
            sprMask.localScale = Vector3.MoveTowards(sprMask.localScale, Vector3.zero, speed * Time.deltaTime);

            yield return null;
            if (sprMask.localScale.x <= 0) break;
        }

        InputManager.Instance.ContInput();
        manager.StartGame(sceneName);
    }

    IEnumerator OpenAnim(float target, float speed)
    {
        InputManager.Instance.stopInput();
        yield return null;
        sprMask.localScale = Vector3.zero;

        while (true)
        {
            sprMask.localScale = Vector3.MoveTowards(sprMask.localScale, target * Vector3.one, speed * Time.deltaTime);

            yield return null;
            if (sprMask.localScale.x >= target) break;
        }


        InputManager.Instance.ContInput();
    }

}