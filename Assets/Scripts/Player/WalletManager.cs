using UnityEngine;

public class WalletManager : MonoBehaviour
{
    private static WalletManager instance;

    [SerializeField] private int startingMoney;

    public static WalletManager Instance => instance;
    public int CurrentMoney { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateGlobalInstance()
    {
        if (instance != null)
        {
            return;
        }

        new GameObject(nameof(WalletManager)).AddComponent<WalletManager>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentMoney = PlayerSaveManager.LoadMoney(startingMoney);
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
        PlayerSaveManager.SaveMoney(CurrentMoney);
    }

    public void ResetMoney()
    {
        SetMoney(startingMoney);
    }
}
