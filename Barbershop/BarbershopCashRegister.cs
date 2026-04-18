using UnityEngine;
using UnityEngine.Events;

public class BarbershopCashRegister : MonoBehaviour
{
    public static BarbershopCashRegister Instance { get; private set; }

    [Header("Caixa")]
    [SerializeField] private int currentMoney;

    public int CurrentMoney => currentMoney;

    public UnityEvent<int> OnMoneyChanged;

    private const string SaveKey = "AFROBARBER_CASH_REGISTER_MONEY";

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

    public void AddMoney(int amount)
    {
        if (amount <= 0)
            return;

        currentMoney += amount;
        Save();

        OnMoneyChanged?.Invoke(currentMoney);

        Debug.Log($"[BarbershopCashRegister] Dinheiro adicionado: R$ {amount}. Caixa atual: R$ {currentMoney}");
    }

    public bool TrySpendMoney(int amount)
    {
        if (amount <= 0)
            return true;

        if (currentMoney < amount)
            return false;

        currentMoney -= amount;
        Save();

        OnMoneyChanged?.Invoke(currentMoney);

        return true;
    }

    public void SetMoney(int amount)
    {
        currentMoney = Mathf.Max(0, amount);
        Save();

        OnMoneyChanged?.Invoke(currentMoney);
    }

    private void Save()
    {
        PlayerPrefs.SetInt(SaveKey, currentMoney);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        currentMoney = PlayerPrefs.GetInt(SaveKey, 0);
    }
}