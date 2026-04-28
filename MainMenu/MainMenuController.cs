using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Optional")]
    [SerializeField] private AsyncSceneLoader sceneLoader;

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
        LoadScene(gameSceneName);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void LoadScene(string sceneName)
    {
        if (sceneLoader != null)
        {
            sceneLoader.LoadSceneByName(sceneName);
            return;
        }

        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }
}