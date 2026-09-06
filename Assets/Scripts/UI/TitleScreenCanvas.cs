using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;

enum TITLE_SCREEN_STATE
{
    TITLE_SCREEN,
    MAIN_MENU_SCREEN,
    POSITION_SELECT_SCREEN,
    SELECT_STATUS_SCREEN
}

public class TitleScreenCanvas : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject subtitleText;
    [SerializeField] private TitleScreenPanel titleScreenPanel;
    [SerializeField] private PositionSelectPanel positionSelectPanel;
    [SerializeField] private StatAllocationPanel statusSelectPanel;
    [SerializeField] private GameFlowManager gameFlowManager;
    // ========================================================
    // 씬 전환 시, 값을 전달하기 위해 임시로 사용하는 변수
    private POSITION selectedPosition;
    public void SetSelectedPosition(POSITION position)
    {
        selectedPosition = position;
    }
    public POSITION GetPosition() { return selectedPosition; }

    // ========================================================

    private TITLE_SCREEN_STATE currentState;

    private void Start()
    {
        currentState = TITLE_SCREEN_STATE.TITLE_SCREEN;

        titleScreenPanel.Hide();
        positionSelectPanel.Hide();
        statusSelectPanel.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TryOpenMainMenu();
    }

    private void TryOpenMainMenu()
    {
        if (currentState != TITLE_SCREEN_STATE.TITLE_SCREEN)
            return;
        currentState = TITLE_SCREEN_STATE.MAIN_MENU_SCREEN;

        subtitleText.SetActive(false);

        titleScreenPanel.Show();
        positionSelectPanel.Hide();
        statusSelectPanel.Hide();
    }

    public void TryPositionMenu()
    {
        if (currentState == TITLE_SCREEN_STATE.MAIN_MENU_SCREEN 
            || currentState == TITLE_SCREEN_STATE.SELECT_STATUS_SCREEN)
        {
            currentState = TITLE_SCREEN_STATE.POSITION_SELECT_SCREEN;

            titleScreenPanel.Hide();
            positionSelectPanel.Show();
            statusSelectPanel.Hide();
        }
    }

    public void TryStatusMenu()
    {
        if (currentState != TITLE_SCREEN_STATE.POSITION_SELECT_SCREEN)
            return;
        currentState = TITLE_SCREEN_STATE.SELECT_STATUS_SCREEN;

        titleScreenPanel.Hide();
        positionSelectPanel.Hide();
        statusSelectPanel.Show();
    }

    public void TryReturnToTitle()
    {
        if (currentState != TITLE_SCREEN_STATE.POSITION_SELECT_SCREEN)
            return;
        currentState = TITLE_SCREEN_STATE.TITLE_SCREEN;
        subtitleText.SetActive(true);
        titleScreenPanel.Hide();
        positionSelectPanel.Hide();
        statusSelectPanel.Hide();
    }

    public void TryStartNewGame()
    {
        gameFlowManager.StartNewGameSelection();
    }
    public void TryContinueGame()
    {
        gameFlowManager.ContinueCareerFromTitle();
    }
    public void TryExitGame()
    {
        gameFlowManager.QuitGameFromTitle();
    }
    public void HandleStartCareer(PlayerStats stats)
    {
        if (gameFlowManager == null || stats == null)
            return;
        Debug.Log("Starting new career with position: " + selectedPosition.ToString() + " and stats: " + stats.ToString());
        gameFlowManager.StartNewCareerFromTitle(selectedPosition, stats.Clone());
    }
}
