using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    [Header("Compatibilidade")]
    [SerializeField] private bool logWarnings = true;

    public int CurrentMoney
    {
        get
        {
            if (FinanceManager.Instance == null)
            {
                if (logWarnings)
                    Debug.LogWarning("[PlayerWallet] FinanceManager.Instance não encontrado.");
                return 0;
            }

            return FinanceManager.Instance.CurrentCash;
        }
    }

    public bool HasEnough(int amount)
    {
        if (FinanceManager.Instance == null)
        {
            if (logWarnings)
                Debug.LogWarning("[PlayerWallet] FinanceManager.Instance não encontrado.");
            return false;
        }

        return FinanceManager.Instance.HasEnoughMoney(amount);
    }

    public bool Spend(int amount)
    {
        if (FinanceManager.Instance == null)
        {
            if (logWarnings)
                Debug.LogWarning("[PlayerWallet] FinanceManager.Instance não encontrado.");
            return false;
        }

        return FinanceManager.Instance.SpendMoney(amount, "Gasto via PlayerWallet");
    }

    public void AddMoney(int amount)
    {
        if (FinanceManager.Instance == null)
        {
            if (logWarnings)
                Debug.LogWarning("[PlayerWallet] FinanceManager.Instance não encontrado.");
            return;
        }

        FinanceManager.Instance.AddMoney(amount, "Entrada via PlayerWallet");
    }
}