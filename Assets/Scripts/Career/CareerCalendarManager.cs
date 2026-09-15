using UnityEngine;

public class CareerCalendarManager : MonoBehaviour
{
    private static CareerCalendarManager instance;

    [SerializeField] private int startingYear = 1;
    [SerializeField] private int startingMonth = 11;

    public static CareerCalendarManager Instance => instance;
    public int CurrentYear { get; private set; }
    public int CurrentMonth { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateGlobalInstance()
    {
        if (instance != null)
        {
            return;
        }

        new GameObject(nameof(CareerCalendarManager)).AddComponent<CareerCalendarManager>();
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
        LoadCareerCalendar();
    }

    public void AdvanceAfterMatch()
    {
        if (CurrentMonth == 5)
        {
            CurrentYear++;
            CurrentMonth = 11;
        }
        else
        {
            CurrentMonth = GetNextMonth(CurrentMonth);
        }

        SaveCareerCalendar();
    }

    public void ResetCareerCalendar()
    {
        CurrentYear = Mathf.Max(1, startingYear);
        CurrentMonth = IsSeasonMonth(startingMonth) ? startingMonth : 11;
        SaveCareerCalendar();
    }

    private void LoadCareerCalendar()
    {
        CurrentYear = PlayerSaveManager.LoadCareerYear(startingYear);

        int savedMonth = PlayerSaveManager.LoadCareerMonth(startingMonth);
        CurrentMonth = IsSeasonMonth(savedMonth) ? savedMonth : 11;
    }

    private void SaveCareerCalendar()
    {
        PlayerSaveManager.SaveCareerDate(CurrentYear, CurrentMonth);
    }

    private static int GetNextMonth(int month)
    {
        if (month >= 11 && month < 12)
        {
            return month + 1;
        }

        if (month >= 1 && month < 5)
        {
            return month + 1;
        }

        return 11;
    }

    private static bool IsSeasonMonth(int month)
    {
        return month == 11 || month == 12 || (month >= 1 && month <= 5);
    }
}
