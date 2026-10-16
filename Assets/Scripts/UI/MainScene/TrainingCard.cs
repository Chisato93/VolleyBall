using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TrainingCard : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text detailText;
    [SerializeField] private TMP_Text effectText;
    [FormerlySerializedAs("timeText")]
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private Button trainingBtn;
    [SerializeField] private TrainingPanel ownerPanel;
    [SerializeField] private int optionIndex;

    public void SetData(TrainingOffer offer, bool canTrain, string unavailableReason)
    {
        titleText.text = offer.Training.Name;
        detailText.text = string.IsNullOrEmpty(unavailableReason) ? offer.Training.GetDetailText() : unavailableReason;
        effectText.text = offer.Training.GetEffectText();
        energyText.text = $"소요 에너지: {offer.EnergyCost}";
        trainingBtn.interactable = canTrain;
    }

    public void SetInteractable(bool value)
    {
        trainingBtn.interactable = value;
    }

    public void Select()
    {
        if (ownerPanel != null) ownerPanel.SelectTraining(optionIndex);
    }
}
