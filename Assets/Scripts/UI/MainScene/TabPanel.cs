using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabPanel : MonoBehaviour
{
    [SerializeField] private List<GameObject> tabList;
    [SerializeField] private TabContentPanel contentPanel;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite DeactivateSprite;

    private void Start()
    {
        SetActiveTab(0);
    }

    public void SetActiveTab(int index)
    {
        contentPanel?.SetActiveTab(index);

        if (tabList == null)
        {
            return;
        }

        for (int i = 0; i < tabList.Count; i++)
        {
            if (tabList[i] == null)
            {
                continue;
            }

            Image tabImage = tabList[i].GetComponent<Image>();
            TMP_Text tabText = tabList[i].GetComponentInChildren<TMP_Text>();

            if (i == index)
            {
                if (tabImage != null)
                {
                    tabImage.sprite = activeSprite;
                }

                if (tabText != null)
                {
                    tabText.color = Color.black;
                }
            }
            else
            {
                if (tabImage != null)
                {
                    tabImage.sprite = DeactivateSprite;
                }

                if (tabText != null)
                {
                    tabText.color = Color.white;
                }
            }
        }
    }

}
