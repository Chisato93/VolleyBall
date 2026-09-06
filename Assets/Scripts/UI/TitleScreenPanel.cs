using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenPanel : MonoBehaviour
{
    [SerializeField] private TitleScreenCanvas titleScreenCanvas;
    [SerializeField] private CanvasGroupView titleScreenPanelView;

    public void Hide()
    {
        titleScreenPanelView.Hide();
    }
    public void Show()
    {
        titleScreenPanelView.Show();
    }

    public void OnNewGameClicked()
    {
        titleScreenCanvas.TryStartNewGame();
    }
    public void OnContinueClicked()
    {
        titleScreenCanvas.TryContinueGame();
    }
    public void OnExitGame()
    {
        titleScreenCanvas.TryExitGame();
    }
}
