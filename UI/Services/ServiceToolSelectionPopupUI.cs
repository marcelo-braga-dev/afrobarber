using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServiceToolSelectionPopupUI : MonoBehaviour
{
    [Header("Painel")]
    [SerializeField] private GameObject root;

    [Header("Cabeçalho")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text emptyText;

    [Header("Lista")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private ServiceToolOptionItemUI optionPrefab;

    [Header("Botões")]
    [SerializeField] private Button closeButton;

    private Action<ProductInventoryState> onToolSelected;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        Close();
    }

    public void Open(
        ServiceActionType actionType,
        List<ProductInventoryState> tools,
        string selectedUniqueId,
        Action<ProductInventoryState> selectionCallback)
    {
        onToolSelected = selectionCallback;

        if (root != null)
            root.SetActive(true);
        else
            gameObject.SetActive(true);

        if (titleText != null)
            titleText.text = $"Escolher ferramenta: {GetActionDisplayName(actionType)}";

        RebuildOptions(tools, selectedUniqueId);
    }

    private void RebuildOptions(List<ProductInventoryState> tools, string selectedUniqueId)
    {
        ClearOptions();

        bool hasTools = tools != null && tools.Count > 0;

        if (emptyText != null)
            emptyText.gameObject.SetActive(!hasTools);

        if (!hasTools)
        {
            if (emptyText != null)
                emptyText.text = "Você não possui item compatível para esta etapa.";

            return;
        }

        foreach (ProductInventoryState tool in tools)
        {
            if (tool == null)
                continue;

            ProductData product = InventoryManager.Instance != null
                ? InventoryManager.Instance.GetProductDataById(tool.productId)
                : null;

            if (product == null)
                continue;

            ServiceToolOptionItemUI item = Instantiate(optionPrefab, contentParent);

            bool isSelected = !string.IsNullOrWhiteSpace(selectedUniqueId)
                && tool.uniqueId == selectedUniqueId;

            item.Setup(tool, product, isSelected, HandleToolSelected);
        }
    }

    private void HandleToolSelected(ProductInventoryState selectedTool)
    {
        onToolSelected?.Invoke(selectedTool);
        Close();
    }

    public void Close()
    {
        if (root != null)
            root.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    private void ClearOptions()
    {
        if (contentParent == null)
            return;

        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);
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