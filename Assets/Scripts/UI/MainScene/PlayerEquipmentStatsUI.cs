using TMPro;
using UnityEngine;

public class PlayerEquipmentStatsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text contentText;
    private int displayedVersion = -1;

    private void OnEnable() => Refresh();

    private void Update()
    {
        if (displayedVersion != PlayerSaveManager.StateVersion) Refresh();
    }

    private void Refresh()
    {
        contentText.text = EquipmentStatsService.BuildStatsText(TrainingService.LoadCareerPlayerStats());
        displayedVersion = PlayerSaveManager.StateVersion;
    }
}
