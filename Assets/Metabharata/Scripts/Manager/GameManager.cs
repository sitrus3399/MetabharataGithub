using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Settings")]
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

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this);
        Instance = this;
    }

    #endregion

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
