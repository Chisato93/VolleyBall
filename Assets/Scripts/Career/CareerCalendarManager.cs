using UnityEngine;

public class CareerCalendarManager : MonoBehaviour
{
    private const string CareerYearPrefsKey = "CareerCurrentYear";
    private const string CareerMonthPrefsKey = "CareerCurrentMonth";

    [SerializeField] private int startingYear = 1;
    [SerializeField] private int startingMonth = 11;

    public int CurrentYear { get; private set; }
    public int CurrentMonth { get; private set; }

    private void Awake()
    {
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
        CurrentYear = Mathf.Max(1, PlayerPrefs.GetInt(CareerYearPrefsKey, startingYear));

        int savedMonth = PlayerPrefs.GetInt(CareerMonthPrefsKey, startingMonth);
        CurrentMonth = IsSeasonMonth(savedMonth) ? savedMonth : 11;
    }

    private void SaveCareerCalendar()
    {
        PlayerPrefs.SetInt(CareerYearPrefsKey, CurrentYear);
        PlayerPrefs.SetInt(CareerMonthPrefsKey, CurrentMonth);
        PlayerPrefs.Save();
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
