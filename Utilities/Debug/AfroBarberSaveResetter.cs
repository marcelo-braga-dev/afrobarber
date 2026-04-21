using UnityEngine;

public class AfroBarberSaveResetter : MonoBehaviour
{
    [Header("Tecla de reset")]
    [SerializeField] private KeyCode resetKey = KeyCode.F10;

    [Header("Resetar dados")]
    [SerializeField] private bool resetXP = true;
    [SerializeField] private bool resetEducation = true;
    [SerializeField] private bool resetInventory = true;
    [SerializeField] private bool resetCashRegister = true;
    [SerializeField] private bool resetServiceSelections = true;
    [SerializeField] private bool resetFinanceHistory = true;
    [SerializeField] private bool resetMoney = true;
    [SerializeField] private bool resetReputation = true;

    [Header("Modo total")]
    [SerializeField] private bool deleteAllPlayerPrefs = false;

    private const string LegacyPlayerMoneyKey = "AFROBARBER_PLAYER_MONEY";
    private const string LegacyCashRegisterKey = "AFROBARBER_CASH_REGISTER_MONEY";
    private const string CurrentCashKey = "AFROBARBER_FINANCE_CURRENT_CASH";
    private const string FinanceHistoryKey = "AFROBARBER_FINANCE_HISTORY";
    private const string FinanceMigrationKey = "AFROBARBER_FINANCE_MIGRATION_V2_DONE";

    private void Update()
    {
        if (Input.GetKeyDown(resetKey))
        {
            ResetSelectedData();
        }
    }

    public void ResetSelectedData()
    {
        if (deleteAllPlayerPrefs)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            if (FinanceManager.Instance != null)
            {
                FinanceManager.Instance.ClearAllData(true);
                FinanceManager.Instance.ResetCash(0);
            }

            if (ServiceSelectionMemory.Instance != null)
            {
                ServiceSelectionMemory.Instance.ClearAllSelections();
            }

            Debug.Log("[AfroBarberSaveResetter] Todos os PlayerPrefs foram apagados.");
            return;
        }

        if (resetReputation)
        {
            PlayerPrefs.DeleteKey("AFROBARBER_REPUTATION_SUM");
            PlayerPrefs.DeleteKey("AFROBARBER_TOTAL_RATINGS");
        }

        if (resetXP)
        {
            PlayerPrefs.DeleteKey("AFROBARBER_PLAYER_XP");
            PlayerPrefs.DeleteKey("AFROBARBER_PLAYER_LEVEL");
        }

        if (resetEducation)
        {
            PlayerPrefs.DeleteKey("AFROBARBER_UNLOCKED_CUTS");
        }

        if (resetInventory)
        {
            PlayerPrefs.DeleteKey("AFROBARBER_INVENTORY_ITEMS");
        }

        if (resetFinanceHistory)
        {
            if (FinanceManager.Instance != null)
                FinanceManager.Instance.ClearHistory();
            else
                PlayerPrefs.DeleteKey(FinanceHistoryKey);
        }

        if (resetMoney || resetCashRegister)
        {
            if (FinanceManager.Instance != null)
                FinanceManager.Instance.ResetCash(0);

            PlayerPrefs.DeleteKey(CurrentCashKey);
            PlayerPrefs.DeleteKey(LegacyPlayerMoneyKey);
            PlayerPrefs.DeleteKey(LegacyCashRegisterKey);
            PlayerPrefs.DeleteKey(FinanceMigrationKey);
        }

        if (resetServiceSelections)
        {
            if (ServiceSelectionMemory.Instance != null)
            {
                ServiceSelectionMemory.Instance.ClearAllSelections();
            }
            else
            {
                PlayerPrefs.DeleteKey("AFROBARBER_SERVICE_SELECTION_INDEX");
                Debug.LogWarning("[AfroBarberSaveResetter] ServiceSelectionMemory.Instance não encontrado. O índice foi apagado, mas seleções dinâmicas antigas podem permanecer.");
            }
        }

        PlayerPrefs.Save();

        Debug.Log("[AfroBarberSaveResetter] Dados selecionados foram resetados.");
    }
}