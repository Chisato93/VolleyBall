using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingSelectionUI : MonoBehaviour
{
    private static readonly string[] TrainingNames =
    {
        "파워 훈련",
        "점프 훈련",
        "리시브 훈련",
        "토스 훈련",
        "블로킹 훈련",
        "서브 훈련",
        "스피드 훈련",
        "체력 훈련",
        "멘탈 훈련",
        "종합 훈련"
    };

    [SerializeField] private GameObject panelObject;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TMP_Text [] optionTexts;

    private void Awake()
    {
        Hide();
    }

    public void ShowRandomOptions()
    {
        if (panelObject == null || optionButtons == null || optionTexts == null)
        {
            return;
        }

        List<string> options = PickRandomTrainings(3);
        int optionCount = Mathf.Min(optionButtons.Length, optionTexts.Length, options.Count);

        for (int i = 0; i < optionCount; i++)
        {
            string trainingName = options[i];
            optionTexts[i].text = trainingName;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => SelectTraining(trainingName));
        }

        panelObject.SetActive(true);
    }

    public void Hide()
    {
        if (panelObject != null)
        {
            panelObject.SetActive(false);
        }
    }

    private void SelectTraining(string trainingName)
    {
        Debug.Log($"Selected training: {trainingName}");
        Hide();
    }

    private static List<string> PickRandomTrainings(int count)
    {
        List<string> pool = new List<string>(TrainingNames);
        List<string> selected = new List<string>();

        while (selected.Count < count && pool.Count > 0)
        {
            int index = Random.Range(0, pool.Count);
            selected.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return selected;
    }
}
