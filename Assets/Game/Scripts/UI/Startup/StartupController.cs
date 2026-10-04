using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartupController : MonoBehaviour
{
    [SerializeField] private Button episode01Button;
    [SerializeField] private Button episode02Button;

    private void Awake()
    {
        episode01Button.onClick.AddListener(() => LoadEpisode("Episode01"));
        episode02Button.onClick.AddListener(() => LoadEpisode("Episode02"));
    }

    private void OnDestroy()
    {
        episode01Button.onClick.RemoveAllListeners();
        episode02Button.onClick.RemoveAllListeners();
    }

    private void LoadEpisode(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
