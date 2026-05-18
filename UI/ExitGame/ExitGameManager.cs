using UnityEngine;
using UnityEngine.InputSystem;

public class ExitGameManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject exitPanel;

    private bool isPanelOpen;

    private void Start()
    {
        if (exitPanel != null)
        {
            exitPanel.SetActive(false);
        }

        isPanelOpen = false;
    }

    private void Update()
    {
#if UNITY_ANDROID || UNITY_EDITOR

        // Botão voltar Android / ESC teclado
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HandleBackButton();
        }

#endif
    }

    private void HandleBackButton()
    {
        if (isPanelOpen)
        {
            CloseExitPanel();
        }
        else
        {
            OpenExitPanel();
        }
    }

    public void OpenExitPanel()
    {
        if (exitPanel == null)
            return;

        isPanelOpen = true;

        exitPanel.SetActive(true);

        // Pausa jogo
        Time.timeScale = 0f;
    }

    public void CloseExitPanel()
    {
        if (exitPanel == null)
            return;

        isPanelOpen = false;

        exitPanel.SetActive(false);

        // Retorna jogo
        Time.timeScale = 1f;
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}