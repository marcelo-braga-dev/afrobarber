using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerXPUI : MonoBehaviour
{
    [Header("Textos")]
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private TMP_Text currentXPText;
    [SerializeField] private TMP_Text nextLevelXPText;
    [SerializeField] private TMP_Text xpProgressText;

    [Header("Barra de XP")]
    [SerializeField] private Slider xpSlider;

    [Header("Configuração")]
    [SerializeField] private bool updateOnStart = true;
    [SerializeField] private string maxLevelText = "Nível Máximo";

    private PlayerXPManager xpManager;

    private void Start()
    {
        xpManager = PlayerXPManager.Instance;

        if (xpManager == null)
        {
            Debug.LogWarning("[PlayerXPUI] PlayerXPManager.Instance não encontrado na cena.");
            return;
        }

        xpManager.OnXPChanged.AddListener(OnXPChanged);
        xpManager.OnLevelChanged.AddListener(OnLevelChanged);

        if (updateOnStart)
            UpdateUI();
    }

    private void OnDestroy()
    {
        if (xpManager == null)
            return;

        xpManager.OnXPChanged.RemoveListener(OnXPChanged);
        xpManager.OnLevelChanged.RemoveListener(OnLevelChanged);
    }

    private void OnXPChanged(int currentXP)
    {
        UpdateUI();
    }

    private void OnLevelChanged(int currentLevel)
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (xpManager == null)
            xpManager = PlayerXPManager.Instance;

        if (xpManager == null)
            return;

        string levelName = xpManager.CurrentLevelName;
        int currentXP = xpManager.CurrentXP;
        int xpToNextLevel = xpManager.XPToNextLevel;
        bool isMaxLevel = xpManager.IsMaxLevel;

        if (levelNameText != null)
        {
            levelNameText.text = $"{levelName}";
        }

        if (currentXPText != null)
        {
            currentXPText.text = isMaxLevel
                ? maxLevelText
                : $"{currentXP}";
        }

        if (nextLevelXPText != null)
        {
            nextLevelXPText.text = isMaxLevel
                ? ""
                : $"{xpToNextLevel}";
        }

        if (xpProgressText != null)
        {
            xpProgressText.text = isMaxLevel
                ? maxLevelText
                : $"{currentXP} / {xpToNextLevel}";
        }

        if (xpSlider != null)
        {
            xpSlider.minValue = 0f;
            xpSlider.maxValue = 1f;
            xpSlider.value = xpManager.XPProgressNormalized;
        }
    }
}