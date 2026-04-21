using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServicePlanningStepItemUI : MonoBehaviour
{
    [Header("Textos")]
    [SerializeField] private TMP_Text orderText;
    [SerializeField] private TMP_Text actionText;
    [SerializeField] private TMP_Text durationText;
    [SerializeField] private TMP_Text toolNameText;
    [SerializeField] private TMP_Text toolStatsText;

    [Header("Imagem da ferramenta")]
    [SerializeField] private Button toolImageButton;
    [SerializeField] private Image toolImage;
    [SerializeField] private Sprite missingToolIcon;

    [Header("Botões")]
    [SerializeField] private Button removeButton;
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;

    private int index;
    private ServiceActionPlanStep step;
    private List<ProductInventoryState> compatibleTools = new List<ProductInventoryState>();

    private Action<int, ProductInventoryState> onToolChanged;
    private Action<int> onRemove;
    private Action<int> onMoveUp;
    private Action<int> onMoveDown;

    public void Setup(
        int index,
        ServiceActionPlanStep step,
        List<ProductInventoryState> compatibleTools,
        Action<int, ProductInventoryState> toolChangedCallback,
        Action<int> removeCallback,
        Action<int> moveUpCallback,
        Action<int> moveDownCallback)
    {
        this.index = index;
        this.step = step;
        this.compatibleTools = compatibleTools ?? new List<ProductInventoryState>();

        onToolChanged = toolChangedCallback;
        onRemove = removeCallback;
        onMoveUp = moveUpCallback;
        onMoveDown = moveDownCallback;

        Refresh();
    }

    private void Refresh()
    {
        if (step == null)
            return;

        if (orderText != null)
            orderText.text = $"{index + 1}.";

        if (actionText != null)
            actionText.text = GetActionDisplayName(step.actionType);

        if (durationText != null)
            durationText.text = $"{step.estimatedMinutes:0.#} min";

        RefreshSelectedToolVisual();
        SetupButtons();
    }

    private void RefreshSelectedToolVisual()
    {
        ProductInventoryState selectedState = GetSelectedToolState();
        ProductData selectedProduct = GetProductData(selectedState);

        bool hasTool = selectedState != null && selectedProduct != null;

        if (toolImage != null)
        {
            if (hasTool && selectedProduct.icon != null)
            {
                toolImage.sprite = selectedProduct.icon;
                toolImage.enabled = true;
            }
            else
            {
                toolImage.sprite = missingToolIcon;
                toolImage.enabled = missingToolIcon != null;
            }
        }

        if (toolNameText != null)
        {
            toolNameText.text = hasTool ? selectedProduct.productName : "Sem ferramenta";
        }

        if (toolStatsText != null)
        {
            if (hasTool)
            {
                toolStatsText.text =
                    $"Precisão {selectedProduct.precisao:0} | " +
                    $"Vel. {selectedProduct.velocidade:0} | " +
                    $"Dur. {selectedProduct.durabilidade:0}";
            }
            else
            {
                toolStatsText.text = compatibleTools.Count > 0
                    ? "Clique no ícone para escolher."
                    : "Nenhum item compatível no inventário.";
            }
        }

        if (toolImageButton != null)
        {
            toolImageButton.onClick.RemoveAllListeners();
            toolImageButton.onClick.AddListener(OpenToolPopup);
        }
    }

    private ProductInventoryState GetSelectedToolState()
    {
        if (string.IsNullOrWhiteSpace(step.productUniqueId))
            return null;

        foreach (ProductInventoryState tool in compatibleTools)
        {
            if (tool != null && tool.uniqueId == step.productUniqueId)
                return tool;
        }

        if (InventoryManager.Instance != null)
            return InventoryManager.Instance.GetItemByUniqueId(step.productUniqueId);

        return null;
    }

    private ProductData GetProductData(ProductInventoryState state)
    {
        if (state == null || InventoryManager.Instance == null)
            return null;

        return InventoryManager.Instance.GetProductDataById(state.productId);
    }

    private void OpenToolPopup()
    {
        ServiceToolSelectionPopupUI popup =
            FindFirstObjectByType<ServiceToolSelectionPopupUI>(FindObjectsInactive.Include);

        if (popup == null)
        {
            Debug.LogWarning("[ServicePlanningStepItemUI] ServiceToolSelectionPopupUI não encontrado na cena.");
            return;
        }

        popup.Open(
            step.actionType,
            compatibleTools,
            step.productUniqueId,
            selectedTool =>
            {
                onToolChanged?.Invoke(index, selectedTool);
            }
        );
    }

    private void SetupButtons()
    {
        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(() => onRemove?.Invoke(index));
        }

        if (upButton != null)
        {
            upButton.onClick.RemoveAllListeners();
            upButton.onClick.AddListener(() => onMoveUp?.Invoke(index));
            upButton.interactable = index > 0;
        }

        if (downButton != null)
        {
            downButton.onClick.RemoveAllListeners();
            downButton.onClick.AddListener(() => onMoveDown?.Invoke(index));
        }
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