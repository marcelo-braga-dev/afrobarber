using UnityEngine;
using UnityEngine.Events;

public class PlayerXPManager : MonoBehaviour
{
    public static PlayerXPManager Instance { get; private set; }

    [Header("XP")]
    [SerializeField] private int currentXP;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int xpPerLevel = 100;

    public int CurrentXP => currentXP;
    public int CurrentLevel => currentLevel;

    public UnityEvent<int> OnXPChanged;
    public UnityEvent<int> OnLevelChanged;

    private const string XPKey = "AFROBARBER_PLAYER_XP";
    private const string LevelKey = "AFROBARBER_PLAYER_LEVEL";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Load();
    }

    public void AddXP(int amount)
    {
        if (amount <= 0)
            return;

        currentXP += amount;

        while (currentXP >= xpPerLevel)
        {
            currentXP -= xpPerLevel;
            currentLevel++;
            OnLevelChanged?.Invoke(currentLevel);
        }

        Save();

        OnXPChanged?.Invoke(currentXP);

        Debug.Log($"[PlayerXPManager] XP ganho: {amount}. XP atual: {currentXP}. Level: {currentLevel}");
    }

    private void Save()
    {
        PlayerPrefs.SetInt(XPKey, currentXP);
        PlayerPrefs.SetInt(LevelKey, currentLevel);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        currentXP = PlayerPrefs.GetInt(XPKey, 0);
        currentLevel = PlayerPrefs.GetInt(LevelKey, 1);
    }
}