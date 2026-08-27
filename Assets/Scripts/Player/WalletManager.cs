using UnityEngine;

public class WalletManager : MonoBehaviour
{
    private const string MoneyPrefsKey = "PlayerOwnedMoney";

    [SerializeField] private int startingMoney;

    public int CurrentMoney { get; private set; }

    private void Awake()
    {
        CurrentMoney = Mathf.Max(0, PlayerPrefs.GetInt(MoneyPrefsKey, startingMoney));
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        long nextMoney = (long)CurrentMoney + amount;
        SetMoney(nextMoney > int.MaxValue ? int.MaxValue : (int)nextMoney);
    }

    public bool CanSpend(int amount)
    {
        return amount > 0 && CurrentMoney >= amount;
    }

    public bool TrySpendMoney(int amount)
    {
        if (!CanSpend(amount))
        {
            return false;
        }

        SetMoney(CurrentMoney - amount);
        return true;
    }

    public void SetMoney(int amount)
    {
        CurrentMoney = Mathf.Max(0, amount);
        PlayerPrefs.SetInt(MoneyPrefsKey, CurrentMoney);
        PlayerPrefs.Save();
    }

    public void ResetMoney()
    {
        SetMoney(startingMoney);
    }
}
