using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryCategoryGroupUI : MonoBehaviour
{
    [SerializeField] private TMP_Text categoryTitleText;
    [SerializeField] private Transform productsParent;
    [SerializeField] private ScrollRect productsScrollRect;
    [SerializeField] private NestedScrollRect nestedScrollRect;

    public Transform ProductsParent => productsParent;

    public void Setup(ProductCategory category, ScrollRect parentVerticalScrollRect)
    {
        if (categoryTitleText != null)
            categoryTitleText.text = CategoryFormatter.ToDisplayName(category);

        if (nestedScrollRect != null && productsScrollRect != null && parentVerticalScrollRect != null)
        {
            nestedScrollRect.Setup(parentVerticalScrollRect, productsScrollRect, true);
        }
    }
}