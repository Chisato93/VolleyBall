internal sealed class MatchEndState
{
    private bool hasAdvancedCareerCalendar;
    private bool hasShownTrainingSelection;

    public void Reset()
    {
        hasAdvancedCareerCalendar = false;
        hasShownTrainingSelection = false;
    }

    public bool TryMarkCareerCalendarAdvanced()
    {
        if (hasAdvancedCareerCalendar)
        {
            return false;
        }

        hasAdvancedCareerCalendar = true;
        return true;
    }

    public bool TryMarkTrainingSelectionShown()
    {
        if (hasShownTrainingSelection)
        {
            return false;
        }

        hasShownTrainingSelection = true;
        return true;
    }
}
