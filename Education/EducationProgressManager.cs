using System.Collections.Generic;
using UnityEngine;

public class EducationProgressManager : MonoBehaviour
{
    public static EducationProgressManager Instance;

    [SerializeField] private AfroCutDatabase cutDatabase;

    private readonly HashSet<string> unlockedCuts = new HashSet<string>();
    private const string SaveKey = "AFROBARBER_UNLOCKED_CUTS";

    public AfroCutDatabase CutDatabase => cutDatabase;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadProgress();
    }

    public bool IsUnlocked(string cutId)
    {
        return unlockedCuts.Contains(cutId);
    }

    public AfroCutInfo GetCutById(string cutId)
    {
        if (cutDatabase == null)
            return null;

        return cutDatabase.GetById(cutId);
    }

    public bool UnlockCut(string cutId)
    {
        if (string.IsNullOrWhiteSpace(cutId))
            return false;

        if (unlockedCuts.Contains(cutId))
            return false;

        unlockedCuts.Add(cutId);
        SaveProgress();

        AfroCutInfo cut = GetCutById(cutId);
        if (cut != null && EducationUnlockPopupUI.Instance != null)
            EducationUnlockPopupUI.Instance.ShowUnlock(cut);

        return true;
    }

    public List<AfroCutInfo> GetAllCuts()
    {
        if (cutDatabase == null)
            return new List<AfroCutInfo>();

        return cutDatabase.GetAllCuts();
    }

    private void SaveProgress()
    {
        string joined = string.Join("|", unlockedCuts);
        PlayerPrefs.SetString(SaveKey, joined);
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        unlockedCuts.Clear();

        string joined = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(joined))
            return;

        string[] ids = joined.Split('|');
        foreach (string id in ids)
        {
            if (!string.IsNullOrWhiteSpace(id))
                unlockedCuts.Add(id);
        }
    }
}