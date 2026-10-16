using TMPro;
using UnityEngine;

public class ActiveSetEffectsPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text[] effectTexts;
    private int displayedVersion = -1;

    private void OnEnable() => Refresh();

    private void Update()
    {
        if (displayedVersion != PlayerSaveManager.StateVersion) Refresh();
    }

    private void Refresh()
    {
        var effects = EquipmentStatsService.GetActiveSetEffects();
        for (int i = 0; i < effectTexts.Length; i++)
        {
            effectTexts[i].text = i < effects.Count ? effects[i].Replace(": ", "\n")
                : i == 0 && effects.Count == 0 ? "적용 중인 세트 효과가 없습니다." : string.Empty;
        }
        displayedVersion = PlayerSaveManager.StateVersion;
    }
}
