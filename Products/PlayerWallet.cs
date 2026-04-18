using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    [SerializeField] private int currentMoney = 1000;

    public int CurrentMoney => currentMoney;

    public bool HasEnough(int amount)
    {
        return currentMoney >= amount;
    }

    public bool Spend(int amount)
    {
        if (currentMoney < amount)
            return false;

        currentMoney -= amount;
        return true;
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
    }
}