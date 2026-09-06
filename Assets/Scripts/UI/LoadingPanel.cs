using UnityEngine;
using UnityEngine.UI;

public class LoadingPanel : MonoBehaviour
{
    [SerializeField] private CanvasGroupView loadingPanelView;
    [SerializeField] private Slider loadingSlider;

    public void Show()
    {
        loadingPanelView.Show();
    }
    public void Hide()
    {
        loadingPanelView.Hide();
    }
    public void SetProgress(float progress)
    {
        loadingSlider.value = progress;
    }
}
