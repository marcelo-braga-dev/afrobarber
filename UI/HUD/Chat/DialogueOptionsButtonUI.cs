using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueOptionsButtonUI : MonoBehaviour
{
    [SerializeField] private DialogueContextType currentContext = DialogueContextType.Queue;
    [SerializeField] private Button openButton;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button optionButtonPrefab;
    [SerializeField] private RectTransform optionsContainer;

    private void Awake()
    {
        if (openButton != null)
            openButton.onClick.AddListener(ToggleOptions);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    public void SetContext(DialogueContextType context)
    {
        currentContext = context;
        if (optionsPanel != null && optionsPanel.activeSelf)
            RebuildOptions();
    }

    public void ToggleOptions()
    {
        if (optionsPanel == null)
            return;

        optionsPanel.SetActive(!optionsPanel.activeSelf);

        if (optionsPanel.activeSelf)
            RebuildOptions();
    }

    private void RebuildOptions()
    {
        if (optionsContainer == null || optionButtonPrefab == null)
            return;

        for (int i = optionsContainer.childCount - 1; i >= 0; i--)
            Destroy(optionsContainer.GetChild(i).gameObject);

        List<DialogueSpeechOption> options = DialogueContextOptionsProvider.GetOptions(currentContext);

        for (int i = 0; i < options.Count; i++)
        {
            DialogueSpeechOption option = options[i];
            Button button = Instantiate(optionButtonPrefab, optionsContainer);

            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = option.label;

            button.onClick.AddListener(() =>
            {
                GlobalDialogueManager.Instance?.TriggerPlayerOption(option);
                if (optionsPanel != null)
                    optionsPanel.SetActive(false);
            });
        }
    }
}
