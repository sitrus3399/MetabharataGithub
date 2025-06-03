using System.Collections.Generic;
using UnityEngine;

public class PageManager : MonoBehaviour
{
    [SerializeField] private List<Page> pages;

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
    SelectCharacterRoom
}
