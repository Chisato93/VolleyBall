using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusStatBarUI : MonoBehaviour
{
    private enum DisplayStat { Power, Jump, Stamina, Speed, Technique, Mental }

    [SerializeField] private DisplayStat stat;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Image fillImage;
    private int displayedVersion = -1;

    private void OnEnable() => Refresh();

    private void Update()
    {
        if (displayedVersion != PlayerSaveManager.StateVersion) Refresh();
    }

    private void Refresh()
    {
        PlayerStats stats = EquipmentStatsService.GetEffectiveStats(TrainingService.LoadCareerPlayerStats());
        int value;
        switch (stat)
        {
            case DisplayStat.Jump: value = stats.Jump; break;
            case DisplayStat.Stamina: value = stats.Stamina; break;
            case DisplayStat.Speed: value = stats.Speed; break;
            case DisplayStat.Technique: value = (stats.Receive + stats.Set + stats.Block + stats.Serve) / 4; break;
            case DisplayStat.Mental: value = stats.Mental; break;
            default: value = stats.Power; break;
        }
        valueText.text = $"{value} / {PlayerStats.MaxValue}";
        fillImage.fillAmount = Mathf.Clamp01(value / (float)PlayerStats.MaxValue);
        displayedVersion = PlayerSaveManager.StateVersion;
    }
}
