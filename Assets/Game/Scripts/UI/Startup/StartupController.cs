using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartupController : MonoBehaviour
{
    [SerializeField] private Button episode01Button;
    [SerializeField] private Button episode02Button;
    [SerializeField] private UIButtonSePlayer buttonSePlayer;
    private bool isLoading;

    private void Awake()
    {
        episode01Button.onClick.AddListener(() => StartCoroutine(LoadEpisodeAfterSe("Episode01")));

        episode02Button.onClick.AddListener(() => StartCoroutine(LoadEpisodeAfterSe("Episode02")));
    }

    private void OnDestroy()
    {
        episode01Button.onClick.RemoveAllListeners();
        episode02Button.onClick.RemoveAllListeners();
    }

    private IEnumerator LoadEpisodeAfterSe(string sceneName)
    {
        if (isLoading)
            yield break;

        isLoading = true;
        episode01Button.interactable = false;
        episode02Button.interactable = false;

        yield return buttonSePlayer.PlayAndWait();

        SceneManager.LoadScene(sceneName);
    }
}
