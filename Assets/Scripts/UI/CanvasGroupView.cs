using Unity.Content;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    void Start()
    {
        Hide();    
    }

    public void Show()
    {
        SetVisible(true);
    }
    public void Hide()
    {
        SetVisible(false);
    }
    private void SetVisible(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

}
