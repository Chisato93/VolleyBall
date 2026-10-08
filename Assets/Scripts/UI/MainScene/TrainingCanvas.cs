using UnityEngine;

public class TrainingCanvas : MonoBehaviour
{
    private enum TrainingPanelType
    {
        Training,
        Status
    }
    [SerializeField] private TrainingPanel trainingPanel;
    [SerializeField] private StatusPanel statusPanel;
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

        OnShowPanel(TrainingPanelType.Training);
    }

    public void OnClickStatus()
    {
        OnShowPanel(TrainingPanelType.Status);
    }
    private void OnShowPanel(TrainingPanelType panel)
    {
        trainingPanel.Hide();
        statusPanel.Hide();
        if(panel == TrainingPanelType.Training)
        {
            trainingPanel.Show();
        }
        else if(panel == TrainingPanelType.Status)
        {
            statusPanel.Show();
        }
    }
}
