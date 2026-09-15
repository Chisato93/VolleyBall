using UnityEngine;

public class TrainingCanvas : MonoBehaviour
{
    [SerializeField] private TrainingPanel trainingPanel;
    [SerializeField] private GameFlowManager gameFlowManager;

    public TrainingPanel TrainingPanel => trainingPanel;

    private void Awake()
    {
        if (trainingPanel == null)
        {
            trainingPanel = GetComponentInChildren<TrainingPanel>(true);
        }

        if (gameFlowManager == null)
        {
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();
        }
    }

    public void OnClickTraining()
    {
        if (gameFlowManager != null)
        {
            gameFlowManager.StartTrainingAction();
            return;
        }

        if (trainingPanel != null)
        {
            trainingPanel.Show();
        }
    }
}
