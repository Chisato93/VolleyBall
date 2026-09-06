using TMPro;
using UnityEngine;

public class StatAllocationRow : MonoBehaviour
{
    [SerializeField] private PlayerStatType statType;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private GameObject selectedFrame;
    // [SerializeField] private GameObject rollChangeEffect;
    // [SerializeField] private ParticleSystem rollChangeParticle;

    public PlayerStatType StatType => statType;

    public void SetValueText(string text)
    {
        valueText.text = text;
    }

    public void SelectedAllocationRow(bool isSelected)
    {
        if(selectedFrame == null)
        {
            Debug.LogError("selectedFrame is not assigned in StatAllocationRow");
        }
        selectedFrame.SetActive(isSelected);
    }
}
