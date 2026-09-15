using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TrainingCard : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text detailText;
    [SerializeField] private TMP_Text effectText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private Button trainingBtn;

    public void SetData(Sprite icon, string title, string detail, string effect, string time, UnityAction onClick)
    {
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
        }

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (detailText != null)
        {
            detailText.text = detail;
        }

        if (effectText != null)
        {
            effectText.text = effect;
        }

        if (timeText != null)
        {
            timeText.text = time;
        }

        if (trainingBtn != null)
        {
            trainingBtn.onClick.RemoveAllListeners();

            if (onClick != null)
            {
                trainingBtn.onClick.AddListener(onClick);
            }
        }
    }
}
