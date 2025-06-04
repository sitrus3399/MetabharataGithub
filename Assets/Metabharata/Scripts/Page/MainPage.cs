using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainPage : Page
{
    [SerializeField] private Button academyButton;
    [SerializeField] private Button almanacButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button dailyMissionButton;
    [SerializeField] private Button accountButton;

    [SerializeField] private WidgetManager widgetManager;
    [SerializeField] private List<Widget> widgets;

    [SerializeField] private TMP_Text modeGameText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] int indexStartGame;
    [SerializeField] int maxIndexGame;

    [SerializeField] private string[] modeGameName;

    [SerializeField] GameObject[] iconBar;

    protected override void Start()
    {
        base.Start();

        indexStartGame = 0;
        modeGameText.text = modeGameName[indexStartGame];

        academyButton.onClick.AddListener(() => { pageManager.OpenPage(PageType.Academy); });
        almanacButton.onClick.AddListener(() => { pageManager.OpenPage(PageType.Almanak); });
        settingsButton.onClick.AddListener(() => { widgetManager.OpenWidget(WidgetType.Setting); });
        settingsButton.onClick.AddListener(() => { widgetManager.OpenWidget(WidgetType.Setting); });
        dailyMissionButton.onClick.AddListener(() => { widgetManager.OpenWidget(WidgetType.DailyCheckIn); });
        startButton.onClick.AddListener(() => { StartFunction(); });
        nextButton.onClick.AddListener(() => { NextFunction(); });
        prevButton.onClick.AddListener(() => { PrevFunction(); });
    }

    void StartFunction()
    {
        switch (indexStartGame)
        {
            case 0:
                pageManager.OpenPage(PageType.StoryPage);
                break;
            case 1:
                pageManager.OpenPage(PageType.FreeBattlePage);
                break;
            case 2:
                pageManager.OpenPage(PageType.OnlinePage);
                break;
            case 3:
                break;
            case 4:
                break;
            default:
                break;
        }
    }

    void NextFunction()
    {
        indexStartGame++;

        if (indexStartGame > maxIndexGame - 1)
        {
            indexStartGame = 0;
        }

        foreach (GameObject tmpGameObject in iconBar)
        {
            tmpGameObject.SetActive(false);
        }

        switch (indexStartGame)
        {
            case 0:
                break;
            case 1:
                iconBar[0].SetActive(true);
                break;
            case 2:
                iconBar[1].SetActive(true);
                break;
            case 3:
                break;
            case 4:
                break;
            default:
                break;
        }

        modeGameText.text = modeGameName[indexStartGame];
    }

    void PrevFunction()
    {
        indexStartGame--;

        if (indexStartGame < 0)
        {
            indexStartGame = maxIndexGame - 1;
        }

        foreach (GameObject tmpGameObject in iconBar)
        {
            tmpGameObject.SetActive(false);
        }

        switch (indexStartGame)
        {
            case 0:
                break;
            case 1:
                iconBar[0].SetActive(true);
                break;
            case 2:
                iconBar[1].SetActive(true);
                break;
            case 3:
                break;
            case 4:
                break;
            default:
                break;
        }

        modeGameText.text = modeGameName[indexStartGame];
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void Update()
    {
        base.Update();
    }
    public override void Show()
    {
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
    }
    
    public void CloseOnline() //Debug saja bukan bagian gameplay
    {
        Array.Resize(ref modeGameName, modeGameName.Length - 1);
        maxIndexGame = modeGameName.Length;
    }    
}
