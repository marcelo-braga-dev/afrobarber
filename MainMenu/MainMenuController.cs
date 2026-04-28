using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Optional Loader")]
    [SerializeField] private AsyncSceneLoader sceneLoader;

    [Header("Preload")]
    [SerializeField] private bool preloadGameSceneOnMenuStart = true;

    private bool isStartingGame;

    private void Start()
    {
        if (sceneLoader != null && preloadGameSceneOnMenuStart && !string.IsNullOrWhiteSpace(gameSceneName))
            sceneLoader.PreloadScene(gameSceneName);
    }

    public void StartGame()
    {
        if (isStartingGame)
            return;

        isStartingGame = true;
        LoadScene(gameSceneName);
    }

    public void MainMenu()
    {
        LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        Debug.Log("[MainMenuController] QuitGame chamado. No Editor, o jogo não será fechado automaticamente.");
#endif
    }

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("[MainMenuController] Nome da cena vazio.");
            return;
        }

        if (sceneLoader != null)
        {
            sceneLoader.LoadSceneByName(sceneName);
            return;
        }

        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }
}