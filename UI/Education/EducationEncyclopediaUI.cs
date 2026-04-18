using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EducationEncyclopediaUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Lista")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private EducationEncyclopediaItemUI itemPrefab;

    [Header("Detalhes")]
    [SerializeField] private Image detailIcon;
    [SerializeField] private TMP_Text detailTitle;
    [SerializeField] private TMP_Text detailDecade;
    [SerializeField] private TMP_Text detailCategory;
    [SerializeField] private TMP_Text detailHistoricalSummary;
    [SerializeField] private TMP_Text detailFullHistory;
    [SerializeField] private TMP_Text detailMeaning;
    [SerializeField] private TMP_Text detailFunFact;

    private readonly List<EducationEncyclopediaItemUI> spawnedItems = new List<EducationEncyclopediaItemUI>();

    private void Start()
    {
        if (root != null)
            root.SetActive(false);
    }

    public void Open()
    {
        if (root != null)
            root.SetActive(true);

        RefreshList();
    }

    public void Close()
    {
        if (root != null)
            root.SetActive(false);
    }

    public void RefreshList()
    {
        ClearList();

        if (EducationProgressManager.Instance == null || itemPrefab == null || contentParent == null)
            return;

        List<AfroCutInfo> allCuts = EducationProgressManager.Instance.GetAllCuts();

        foreach (var cut in allCuts)
        {
            if (cut == null)
                continue;

            bool unlocked = EducationProgressManager.Instance.IsUnlocked(cut.cutId);

            var item = Instantiate(itemPrefab, contentParent);
            item.Setup(cut, unlocked, OnSelectedItem);
            spawnedItems.Add(item);
        }
    }

    private void ClearList()
    {
        foreach (var item in spawnedItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }

        spawnedItems.Clear();
    }

    private void OnSelectedItem(AfroCutInfo cut, bool unlocked)
    {
        if (cut == null)
            return;

        if (detailIcon != null)
            detailIcon.sprite = cut.icon;

        if (detailTitle != null)
            detailTitle.text = unlocked ? cut.cutName : "???";

        if (detailDecade != null)
            detailDecade.text = unlocked ? ("Década: " + cut.decade) : "Década: ???";

        if (detailCategory != null)
            detailCategory.text = unlocked ? ("Categoria: " + cut.category) : "Categoria: ???";

        if (detailHistoricalSummary != null)
            detailHistoricalSummary.text = unlocked ? cut.historicalSummary : "Descubra este corte jogando.";

        if (detailFullHistory != null)
            detailFullHistory.text = unlocked ? cut.fullHistoricalDescription : "Conteúdo bloqueado.";

        if (detailMeaning != null)
            detailMeaning.text = unlocked ? cut.culturalMeaning : "Conteúdo bloqueado.";

        if (detailFunFact != null)
            detailFunFact.text = unlocked ? cut.funFact : "Conteúdo bloqueado.";
    }
}