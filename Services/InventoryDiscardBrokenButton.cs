using TMPro;
using UnityEngine;

public class InventoryDiscardBrokenButton : MonoBehaviour
{
    [SerializeField] private TMP_Text feedbackText;

    public void DiscardAllBrokenItems()
    {
        if (InventoryManager.Instance == null)
            return;

        int removed = InventoryManager.Instance.RemoveAllUnusableItems();

        if (feedbackText != null)
        {
            feedbackText.text = removed > 0
                ? $"{removed} item(ns) descartado(s)."
                : "Nenhum item danificado para descartar.";
        }

        if (InventoryUIManager.Instance != null)
            InventoryUIManager.Instance.RefreshUI();
    }
}