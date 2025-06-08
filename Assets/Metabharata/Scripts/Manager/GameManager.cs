using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public BahasaType bahasaType;
    public StageType stageType;
    public float volume;

    [Header("Story")]
    public StoryData storyDataSelected;

    [Header("FreeBattle")]
    public CharacterData characterDataFreeBattle1;
    public CharacterData characterDataFreeBattle2;
    public Sprite backgroundFreeBattle;

    [Header("Online")]
    public string joinCode;
    public string nameRoom;
    public CharacterData characterDataOnlineHost;
    public CharacterData characterDataOnlineClient;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void GantiBahasa(BahasaType tmpBahasaType)
    {
        bahasaType = tmpBahasaType;
    }

    public void SelectStoryData(StoryData tmpStoryData)
    {
        storyDataSelected = tmpStoryData;
    }
}

public enum StageType
{
    Story,
    FreeBattle,
    Online
}
