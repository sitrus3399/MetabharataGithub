using System.Collections.Generic;
using UnityEngine;

public class PageManager : MonoBehaviour
{
    [SerializeField] private List<Page> pages;

    public static PageManager Instance { get; private set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void OpenPage(PageType tmpType)
    {
        foreach (Page page in pages)
        {
            if (page.type == tmpType)
            {
                page.Show();
            }
            else
            {
                page.Hide();
            }
        }
    }
}

public enum PageType
{
    MainMenu,
    Academy,
    StoryPage,
    Login,
    FreeBattlePage,
    Almanak,
    OnlinePage,
    MainRoom,
    SelectCharacterRoom,
    OnlineRoom
}
