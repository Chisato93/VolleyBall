using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    private static SceneLoadManager instance;
    public static SceneLoadManager Instance => instance;

    [SerializeField] private LoadingPanel loadingPanel;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void LoadScene(SceneEnums scene)
    {
        StartCoroutine(LoadSceneAsync((int)scene));
    }

    private IEnumerator LoadSceneAsync(int sceneName)
    {
        loadingPanel.Show();
        loadingPanel.SetProgress(0f);
        yield return null;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float minimumLoadingTime = 1.0f;
        float elapsed = 0f;

        while (elapsed < minimumLoadingTime || operation.progress < 0.9f)
        {
            elapsed += Time.unscaledDeltaTime;

            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(elapsed / minimumLoadingTime);

            float displayProgress = Mathf.Min(realProgress, timeProgress);

            loadingPanel.SetProgress(displayProgress);
            yield return null;
        }
        loadingPanel.SetProgress(1f);

        yield return new WaitForSecondsRealtime(1f);
        operation.allowSceneActivation = true;

        while(!operation.isDone)
        {
            yield return null;
        }
        loadingPanel.Hide();
    }
}
