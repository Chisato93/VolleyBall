using UnityEngine;
using UnityEngine.UI;

public class PositionSelectPanel : MonoBehaviour
{
    [SerializeField] private TitleScreenCanvas titleScreenCanvas;
    [SerializeField] private CanvasGroupView positionSelectPanelView;

    [SerializeField] private Button outsideHitterButton;
    [SerializeField] private Button oppositeHitterButton;
    [SerializeField] private Button middleBlockerButton;
    [SerializeField] private Button setterButton;
    [SerializeField] private Button liberoButton;
    [SerializeField] private Button defensiveSpecialistButton;

    private void Awake()
    {
        outsideHitterButton.onClick.AddListener(() => HandlePositionSelected(POSITION.OutsideHitter));
        oppositeHitterButton.onClick.AddListener(() => HandlePositionSelected(POSITION.OppositeHitter));
        middleBlockerButton.onClick.AddListener(() => HandlePositionSelected(POSITION.MiddleBlocker));
        setterButton.onClick.AddListener(() => HandlePositionSelected(POSITION.Setter));
        liberoButton.onClick.AddListener(() => HandlePositionSelected(POSITION.Libero));
        defensiveSpecialistButton.onClick.AddListener(() => HandlePositionSelected(POSITION.DefensiveSpecialist));
    }

    public void Hide()
    {
        positionSelectPanelView.Hide();
    }
    public void Show()
    {
        positionSelectPanelView.Show();
    }

    private void HandlePositionSelected(POSITION position)
    {
        titleScreenCanvas.SetSelectedPosition(position);
        titleScreenCanvas.TryStatusMenu();
    }
    public void OnBackClicked()
    {
        titleScreenCanvas.TryReturnToTitle();
    }

}

