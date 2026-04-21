using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServicePlanningActionButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private Image iconImage;

    private Action onClick;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(HandleClick);
    }

    public void Setup(ServiceActionType actionType, Action clickCallback)
    {
        onClick = clickCallback;

        if (labelText != null)
            labelText.text = GetActionDisplayName(actionType);

        if (iconImage != null)
            iconImage.enabled = false;
    }

    private void HandleClick()
    {
        onClick?.Invoke();
    }

    private string GetActionDisplayName(ServiceActionType actionType)
    {
        return actionType switch
        {
            ServiceActionType.Wash => "Lavar",
            ServiceActionType.Comb => "Pentear",
            ServiceActionType.Cut => "Cortar",
            ServiceActionType.Razor => "Navalha",
            ServiceActionType.Finish => "Acabamento",
            ServiceActionType.Define => "Definir",
            ServiceActionType.Beard => "Barba",
            ServiceActionType.Finalize => "Finalizar",
            _ => actionType.ToString()
        };
    }
}