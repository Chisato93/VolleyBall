using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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

    private POSITION selectedPosition;
    private TITLE_SCREEN_STATE currentState;

    public void SetSelectedPosition(POSITION position)
    {
        selectedPosition = position;
    }

    public POSITION GetPosition()
    {
        return selectedPosition;
    }

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
        TryPositionMenu();
    }

    public void TryContinueGame()
    {
        LoadMainScene();
    }

    public void TryExitGame()
    {
#if UNITY_EDITOR
        Debug.Log("에디터에서는 종료 버튼이 게임을 닫지 않습니다.");
#else
        Application.Quit();
#endif
    }

    public void HandleStartCareer(PlayerStats stats)
    {
        if (stats == null)
            return;

        Debug.Log("Starting new career with position: " + selectedPosition.ToString() + " and stats: " + stats.ToString());
        GameFlowStateManager.StartNewCareer(selectedPosition, stats.Clone());
        LoadMainScene();
    }

    private void LoadMainScene()
    {
        if (SceneLoadManager.Instance != null)
        {
            SceneLoadManager.Instance.LoadScene(SceneEnums.MainScene);
            return;
        }

        SceneManager.LoadScene((int)SceneEnums.MainScene);
    }
}
