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
    [SerializeField] private bool resetFinanceHystory = true;
    [SerializeField] private bool resetMoney = true;
    [SerializeField] private bool resetReputatiom = true;

    [Header("Modo total")]
    [SerializeField] private bool deleteAllPlayerPrefs = false;

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

            Debug.Log("[AfroBarberSaveResetter] Todos os PlayerPrefs foram apagados.");
            return;
        }

        if (resetReputatiom)
        {
            PlayerPrefs.DeleteKey("AFROBARBER_REPUTATION_SUM");
            PlayerPrefs.DeleteKey("AFROBARBER_TOTAL_RATINGS");
        }

        if (resetMoney)
        {
            PlayerPrefs.DeleteKey("AFROBARBER_PLAYER_MONEY");
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

        if(resetFinanceHystory)
        {
            PlayerPrefs.DeleteKey("AFROBARBER_FINANCE_HISTORY");
        }

        if (resetInventory)
        {
            PlayerPrefs.DeleteKey("AFROBARBER_INVENTORY_ITEMS");
        }

        if (resetCashRegister)
        {
            PlayerPrefs.DeleteKey("AFROBARBER_CASH_REGISTER_MONEY");
        }

        if (resetServiceSelections)
        {
            Debug.LogWarning(
                "[AfroBarberSaveResetter] As chaves AFROBARBER_SERVICE_SELECTION_ são dinâmicas. " +
                "Para apagar todas com segurança, use DeleteAll ou crie uma lista de chaves salvas."
            );
        }

        PlayerPrefs.Save();

        Debug.Log("[AfroBarberSaveResetter] Dados selecionados foram resetados.");
    }
}