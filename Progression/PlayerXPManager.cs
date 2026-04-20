using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class PlayerLevelData
{
    [Header("Configuração do nível")]
    public int levelNumber = 1;
    public string levelName = "Aprendiz da Navalha";

    [Tooltip("XP necessário para sair deste nível e ir para o próximo.")]
    public int xpToNextLevel = 100;
}

public class PlayerXPManager : MonoBehaviour
{
    public static PlayerXPManager Instance { get; private set; }

    [Header("Nível atual")]
    [SerializeField] private int currentLevel = 1;

    [Tooltip("XP acumulado dentro do nível atual.")]
    [SerializeField] private int currentXP;

    [Header("Tabela de níveis")]
    [SerializeField]
    private PlayerLevelData[] levels =
    {
        new PlayerLevelData
        {
            levelNumber = 1,
            levelName = "Aprendiz da Navalha",
            xpToNextLevel = 100
        },
        new PlayerLevelData
        {
            levelNumber = 2,
            levelName = "Barbeiro de Bairro",
            xpToNextLevel = 250
        },
        new PlayerLevelData
        {
            levelNumber = 3,
            levelName = "Profissional da Cadeira",
            xpToNextLevel = 500
        },
        new PlayerLevelData
        {
            levelNumber = 4,
            levelName = "Mestre do Degradê",
            xpToNextLevel = 900
        },
        new PlayerLevelData
        {
            levelNumber = 5,
            levelName = "Lenda AfroBarber",
            xpToNextLevel = 0
        }
    };

    [Header("Eventos")]
    public UnityEvent<int> OnXPChanged;
    public UnityEvent<int> OnLevelChanged;
    public UnityEvent<string> OnLevelNameChanged;

    private const string XPKey = "AFROBARBER_PLAYER_XP";
    private const string LevelKey = "AFROBARBER_PLAYER_LEVEL";

    public int CurrentXP => currentXP;
    public int CurrentLevel => currentLevel;

    public string CurrentLevelName
    {
        get
        {
            PlayerLevelData data = GetCurrentLevelData();

            if (data == null)
                return "Sem nível";

            return data.levelName;
        }
    }

    public int XPToNextLevel
    {
        get
        {
            PlayerLevelData data = GetCurrentLevelData();

            if (data == null)
                return 0;

            if (IsMaxLevel)
                return 0;

            return Mathf.Max(0, data.xpToNextLevel);
        }
    }

    public bool IsMaxLevel
    {
        get
        {
            return currentLevel >= levels.Length;
        }
    }

    public float XPProgressNormalized
    {
        get
        {
            if (IsMaxLevel)
                return 1f;

            if (XPToNextLevel <= 0)
                return 1f;

            return Mathf.Clamp01((float)currentXP / XPToNextLevel);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Load();
        ClampLevelValues();
    }

    public void AddXP(int amount)
    {
        if (amount <= 0)
            return;

        if (IsMaxLevel)
        {
            Debug.Log("[PlayerXPManager] Jogador já está no nível máximo.");
            return;
        }

        currentXP += amount;

        while (!IsMaxLevel && currentXP >= XPToNextLevel)
        {
            currentXP -= XPToNextLevel;
            currentLevel++;

            OnLevelChanged?.Invoke(currentLevel);
            OnLevelNameChanged?.Invoke(CurrentLevelName);

            Debug.Log($"[PlayerXPManager] Subiu para o nível {currentLevel}: {CurrentLevelName}");
        }

        if (IsMaxLevel)
            currentXP = 0;

        Save();

        OnXPChanged?.Invoke(currentXP);

        Debug.Log(
            $"[PlayerXPManager] XP ganho: {amount}. " +
            $"Nível: {currentLevel} - {CurrentLevelName}. " +
            $"XP atual: {currentXP}/{XPToNextLevel}"
        );
    }

    public PlayerLevelData GetCurrentLevelData()
    {
        if (levels == null || levels.Length == 0)
            return null;

        int index = Mathf.Clamp(currentLevel - 1, 0, levels.Length - 1);
        return levels[index];
    }

    public PlayerLevelData GetLevelData(int level)
    {
        if (levels == null || levels.Length == 0)
            return null;

        int index = Mathf.Clamp(level - 1, 0, levels.Length - 1);
        return levels[index];
    }

    public void ResetProgress()
    {
        currentLevel = 1;
        currentXP = 0;

        Save();

        OnLevelChanged?.Invoke(currentLevel);
        OnLevelNameChanged?.Invoke(CurrentLevelName);
        OnXPChanged?.Invoke(currentXP);

        Debug.Log("[PlayerXPManager] Progresso de XP resetado.");
    }

    private void ClampLevelValues()
    {
        if (levels == null || levels.Length == 0)
        {
            currentLevel = 1;
            currentXP = 0;
            return;
        }

        currentLevel = Mathf.Clamp(currentLevel, 1, levels.Length);

        if (currentXP < 0)
            currentXP = 0;

        if (IsMaxLevel)
            currentXP = 0;
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