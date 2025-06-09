using MetabharataAudio;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Manager;
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private PlayerPanel playerPanelLeft;
    [SerializeField] private PlayerPanel playerPanelRight;
    
    [Header ("Top Gameplay Panel")]
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private Button pauseButton;

    [SerializeField] private CharacterBase characterPrefab;
    [SerializeField] private Transform characterLeftLocation;
    [SerializeField] private Transform characterRightLocation;
    [SerializeField] private List<CharacterBase> characterOnGameplay;

    [SerializeField] private int roundNumber;
    [SerializeField] private int storyNumber;

    [SerializeField] private SpriteRenderer background;

    [SerializeField] private VideoPlayer specialSkillVideoPlayer;
    [SerializeField] private bool videoOn;
    
    [Header("Winner")]
    [SerializeField] private CharacterBase winner;
    [SerializeField] private int winnerNumber;
    [SerializeField] private TMP_Text winnerNotif;
    [SerializeField] private WidgetManager widgetManager;

    [SerializeField] private int[] rewardFreeBattle;
    [SerializeField] private TMP_Text rewardText;

    [SerializeField] private bool gameEnd;

    public List<int> playerWin;

    public UnityAction<CharacterBase> joinRoom;

    public Canvas MainCanvas { get { return mainCanvas; } }
    
    public PlayerPanel PlayerPanelLeft { get { return playerPanelLeft; } }
    public PlayerPanel PlayerPanelRight { get { return playerPanelRight; } }
    public Transform CharacterLeftLocation { get { return characterLeftLocation; } }
    public Transform CharacterRightLocation { get { return characterRightLocation; } }

    public List<CharacterBase> CharacterOnGameplay { get { return characterOnGameplay; } }
    public int StoryNumber { get { return storyNumber; } }
    public bool VideoOn { get { return videoOn; } }
    public bool GameEnd { get { return gameEnd; } }

    private void Awake()
    {
        if (Manager != null && Manager != this)
        {
            Destroy(this);
        }
        else if (Manager == null)
        {
            Manager = this;
        }
    }

    private void Start()
    {
        roundText.text = $"Round {roundNumber}";

        AudioManager.Manager.PlaySFX("AudioBattle");

        CharacterBase newCharacter1;
        CharacterController newController1;
        CharacterBase newCharacter2;
        CharacterController newController2;

        CharacterCollect newCharacterCollect1 = new CharacterCollect();
        CharacterCollect newCharacterCollect2 = new CharacterCollect();

        switch (GameManager.Instance.stageType)
        {
            case StageType.Story:
                roundNumber = 1;
                storyNumber = 1;

                newCharacter1 = Instantiate(characterPrefab);
                newController1 = newCharacter1.GetComponent<CharacterController>();

                //GameManager.Instance.storyDataSelected.characterLevel
                newCharacterCollect1.punchLevel = GameManager.Instance.storyDataSelected.characterLevel;
                newCharacterCollect1.kickLevel = GameManager.Instance.storyDataSelected.characterLevel;
                newCharacterCollect1.weaponLevel = GameManager.Instance.storyDataSelected.characterLevel;
                newCharacterCollect1.specialSkillLevel = GameManager.Instance.storyDataSelected.characterLevel;
                newCharacterCollect1.defendLevel = GameManager.Instance.storyDataSelected.characterLevel;

                newCharacter1.InitPlayerPanel(playerPanelLeft);
                newCharacter1.InitData(GameManager.Instance.storyDataSelected.characterData, newCharacterCollect1);
                newCharacter1.InitSpawnLocation(characterLeftLocation.position);
                newCharacter1.InitOwned(true, false);

                newController1.InitController();
                characterOnGameplay.Add(newCharacter1);

                newCharacter2 = Instantiate(characterPrefab);
                newController2 = newCharacter2.GetComponent<CharacterController>();

                //GameManager.Instance.storyDataSelected.stageList[0].level
                newCharacterCollect2.punchLevel = GameManager.Instance.storyDataSelected.stageList[storyNumber - 1].level;
                newCharacterCollect2.kickLevel = GameManager.Instance.storyDataSelected.stageList[storyNumber - 1].level;
                newCharacterCollect2.weaponLevel = GameManager.Instance.storyDataSelected.stageList[storyNumber - 1].level;
                newCharacterCollect2.specialSkillLevel = GameManager.Instance.storyDataSelected.stageList[storyNumber - 1].level;
                newCharacterCollect2.defendLevel = GameManager.Instance.storyDataSelected.stageList[storyNumber - 1].level;

                newCharacter2.InitPlayerPanel(playerPanelRight);
                newCharacter2.InitData(GameManager.Instance.storyDataSelected.stageList[storyNumber - 1].enemy, newCharacterCollect2);
                newCharacter2.InitSpawnLocation(characterRightLocation.position);
                newCharacter2.InitOwned(false, true);

                newController2.InitController();
                characterOnGameplay.Add(newCharacter2);

                if (characterOnGameplay.Count == 2)
                {
                    characterOnGameplay[0].SetCharacterAttackData(characterOnGameplay[1]);
                    characterOnGameplay[1].SetCharacterAttackData(characterOnGameplay[0]);
                }

                background.sprite = GameManager.Instance.storyDataSelected.backgroundSprite;
                break;
            case StageType.FreeBattle:
                roundNumber = 1;

                newCharacter1 = Instantiate(characterPrefab);
                newController1 = newCharacter1.GetComponent<CharacterController>();

                newCharacterCollect1.punchLevel = 1;
                newCharacterCollect1.kickLevel = 1;
                newCharacterCollect1.weaponLevel = 1;
                newCharacterCollect1.specialSkillLevel = 1;
                newCharacterCollect1.defendLevel = 1;

                newCharacter1.InitPlayerPanel(playerPanelLeft);
                newCharacter1.InitData(GameManager.Instance.characterDataFreeBattle1, newCharacterCollect1);
                newCharacter1.InitSpawnLocation(characterLeftLocation.position);
                newCharacter1.InitOwned(true, false);

                newController1.InitController();
                characterOnGameplay.Add(newCharacter1);

                newCharacter2 = Instantiate(characterPrefab);
                newController2 = newCharacter2.GetComponent<CharacterController>();

                newCharacterCollect2.punchLevel = 1;
                newCharacterCollect2.kickLevel = 1;
                newCharacterCollect2.weaponLevel = 1;
                newCharacterCollect2.specialSkillLevel = 1;
                newCharacterCollect2.defendLevel = 1;

                newCharacter2.InitPlayerPanel(playerPanelRight);
                newCharacter2.InitData(GameManager.Instance.characterDataFreeBattle2, newCharacterCollect2);
                newCharacter2.InitSpawnLocation(characterRightLocation.position);
                newCharacter2.InitOwned(false, true);

                newController2.InitController();
                characterOnGameplay.Add(newCharacter2);

                if (characterOnGameplay.Count == 2)
                {
                    characterOnGameplay[0].SetCharacterAttackData(characterOnGameplay[1]);
                    characterOnGameplay[1].SetCharacterAttackData(characterOnGameplay[0]);
                }

                background.sprite = GameManager.Instance.backgroundFreeBattle;
                break;
            case StageType.Online:
                roundNumber = 1;
                background.sprite = GameManager.Instance.backgroundFreeBattle;

                //newCharacter1 = Instantiate(characterPrefab);
                //newController1 = newCharacter1.GetComponent<CharacterController>();
                //newCharacter1.transform.position = characterLeftLocation.position;
                //newCharacter1.isOwned = true;
                //newCharacter1.isAI = false;

                //newCharacterCollect1.punchLevel = 1;
                //newCharacterCollect1.kickLevel = 1;
                //newCharacterCollect1.weaponLevel = 1;
                //newCharacterCollect1.specialSkillLevel = 1;
                //newCharacterCollect1.defendLevel = 1;

                //newCharacter1.InitPlayerPanel(playerPanelLeft);
                //newCharacter1.InitData(GameManager.Instance.characterDataFreeBattle1, newCharacterCollect1);

                //newController1.InitController();
                //characterOnGameplay.Add(newCharacter1);

                //newCharacter2 = Instantiate(characterPrefab);
                //newController2 = newCharacter2.GetComponent<CharacterController>();
                //newCharacter2.transform.position = characterRightLocation.position;
                //newCharacter2.isOwned = false;
                //newCharacter2.isAI = true;

                //newCharacterCollect2.punchLevel = 1;
                //newCharacterCollect2.kickLevel = 1;
                //newCharacterCollect2.weaponLevel = 1;
                //newCharacterCollect2.specialSkillLevel = 1;
                //newCharacterCollect2.defendLevel = 1;

                //newCharacter2.InitPlayerPanel(playerPanelRight);
                //newCharacter2.InitData(GameManager.Instance.characterDataFreeBattle2, newCharacterCollect2);

                //newController2.InitController();
                //characterOnGameplay.Add(newCharacter2);

                //if (characterOnGameplay.Count == 2)
                //{
                //    characterOnGameplay[0].SetCharacterAttackData(characterOnGameplay[1]);
                //    characterOnGameplay[1].SetCharacterAttackData(characterOnGameplay[0]);
                //}
                break;
            default:
                break;
        }

        pauseButton.onClick.AddListener(() => { PauseFunction(); });
    }

    void PauseFunction()
    {
        widgetManager.OpenWidget(WidgetType.Pause);
        Time.timeScale = 0;
    }

    public PlayerPanel GetPlayerPanel(CharacterBase tmpCharacter)
    {
        switch (GameManager.Instance.stageType)
        {
            case StageType.Story:
                if (tmpCharacter.isOwned)
                {
                    return playerPanelLeft;
                }
                else
                {
                    return playerPanelRight;
                }
                break;
            case StageType.FreeBattle:
                break;
            case StageType.Online:
                break;
            default:
                break;
        }

        return null;
    }

    public void InitWinner(CharacterBase tmpWinner, int tmpWinnerNumber)
    {
        gameEnd = true;
        winner = tmpWinner;
        winnerNumber = tmpWinnerNumber;

        if (tmpWinnerNumber == 1)
        {
            playerWin[roundNumber - 1] = 1;
        }
        else if(tmpWinnerNumber == 2)
        {
            playerWin[roundNumber - 1] = 2;
        }

        playerPanelLeft.SetScoreImage(playerWin, roundNumber, true);
        playerPanelRight.SetScoreImage(playerWin, roundNumber, false);

        NextRound();
    }

    public void RestartGame()
    {
        gameEnd = false;
        roundText.text = $"Round {roundNumber}";

        AudioManager.Manager.PlaySFX("AudioBattle");

        winnerNotif.gameObject.SetActive(false);

        foreach (CharacterBase character in characterOnGameplay)
        {
            character.SetRestart();
        }

        characterOnGameplay[0].transform.position = characterLeftLocation.transform.position;
        characterOnGameplay[1].transform.position = characterRightLocation.transform.position;
    }

    public void NextRound()
    {
        AudioManager.Manager.PlaySFX("AudioBattle");

        if (roundNumber < 3)
        {
            StartCoroutine(InitNextRound());
        }
        else if (roundNumber >= 3)
        {
            EndGame();
        }
    }

    IEnumerator InitNextRound()
    {
        gameEnd = false;
        winnerNotif.gameObject.SetActive(true);
        winnerNotif.text = $"Player {winnerNumber} \n {winner.BaseData.characterName} \n WIN ";

        yield return new WaitForSeconds(5);

        roundNumber += 1;

        roundText.text = $"Round {roundNumber}";

        winnerNotif.gameObject.SetActive(false);

        foreach (CharacterBase character in characterOnGameplay)
        {
            character.SetRestart();
        }

        characterOnGameplay[0].transform.position = characterLeftLocation.transform.position;
        characterOnGameplay[1].transform.position = characterRightLocation.transform.position;
    }

    public void NextStory()
    {
        AudioManager.Manager.PlaySFX("AudioBattle");

        PlayerManager.Manager.AddCoin(GameManager.Instance.storyDataSelected.stageList[storyNumber - 1].reward);

        storyNumber += 1;

        StartCoroutine(InitNextStory());
    }

    IEnumerator InitNextStory()
    {
        gameEnd = false;
        winnerNotif.gameObject.SetActive(true);
        winnerNotif.text = $"Next Story \n Story {storyNumber} \n vs {GameManager.Instance.storyDataSelected.stageList[storyNumber - 1].enemy.characterName}";

        yield return new WaitForSeconds(5);

        roundNumber = 1;

        roundText.text = $"Round {roundNumber}";

        winnerNotif.gameObject.SetActive(false);

        foreach (CharacterBase character in characterOnGameplay)
        {
            character.SetRestart();
        }

        for (int i = 0; i < playerWin.Count; i++)
        {
            playerWin[i] = 0;
        }

        playerPanelLeft.SetScoreImage(playerWin, roundNumber, true);
        playerPanelRight.SetScoreImage(playerWin, roundNumber, false);

        characterOnGameplay[0].transform.position = characterLeftLocation.transform.position;
        characterOnGameplay[1].transform.position = characterRightLocation.transform.position;

        CharacterCollect newCharacterCollect2 = new CharacterCollect();

        newCharacterCollect2.punchLevel = GameManager.Instance.storyDataSelected.stageList[storyNumber - 1].level;
        newCharacterCollect2.kickLevel = GameManager.Instance.storyDataSelected.stageList[storyNumber - 1].level;
        newCharacterCollect2.weaponLevel = GameManager.Instance.storyDataSelected.stageList[storyNumber - 1].level;
        newCharacterCollect2.specialSkillLevel = GameManager.Instance.storyDataSelected.stageList[storyNumber - 1].level;
        newCharacterCollect2.defendLevel = GameManager.Instance.storyDataSelected.stageList[storyNumber - 1].level;

        characterOnGameplay[1].InitPlayerPanel(playerPanelRight);
        characterOnGameplay[1].InitData(GameManager.Instance.storyDataSelected.stageList[storyNumber - 1].enemy, newCharacterCollect2);
        characterOnGameplay[1].InitSpawnLocation(characterRightLocation.transform.position);
        characterOnGameplay[1].InitOwned(false, true);
        
        characterOnGameplay[1].SetCharacterAttackData(characterOnGameplay[storyNumber - 1]);
    }

    public void EndGame()
    {
        int reward = 0;

        if (GameManager.Instance.stageType == StageType.FreeBattle)
        {
            if (GameManager.Instance.stageType == StageType.FreeBattle)
            {
                switch (characterOnGameplay[1].CharacterType)
                {
                    case CharacterType.Prajurit:
                        PlayerManager.Manager.AddCoin(rewardFreeBattle[0]);
                        reward = rewardFreeBattle[0];
                        break;
                    case CharacterType.Elite:
                        PlayerManager.Manager.AddCoin(rewardFreeBattle[1]);
                        reward = rewardFreeBattle[1];
                        break;
                    case CharacterType.Epic:
                        PlayerManager.Manager.AddCoin(rewardFreeBattle[2]);
                        reward = rewardFreeBattle[2];
                        break;
                    case CharacterType.Rare:
                        PlayerManager.Manager.AddCoin(rewardFreeBattle[3]);
                        reward = rewardFreeBattle[3];
                        break;
                    case CharacterType.Legendary:
                        PlayerManager.Manager.AddCoin(rewardFreeBattle[4]);
                        reward = rewardFreeBattle[4];
                        break;
                    default:
                        break;
                }
            }
        }
        else if(GameManager.Instance.stageType == StageType.Story)
        {
            reward = GameManager.Instance.storyDataSelected.stageList[storyNumber - 1].reward;

            PlayerManager.Manager.UnlockStory(GameManager.Instance.storyDataSelected.storyNumber + 1);
        }

        rewardText.text = $"{reward}";

        widgetManager.OpenWidget(WidgetType.EndGame);
    }

    public void PlaySpecialSkillVideo(VideoClip videoClip)
    {
        specialSkillVideoPlayer.Stop();
        specialSkillVideoPlayer.clip = videoClip;
        specialSkillVideoPlayer.gameObject.SetActive(true);
        specialSkillVideoPlayer.Play();

        videoOn = true;

        StartCoroutine(StopVideo());
    }

    IEnumerator StopVideo()
    {
        yield return new WaitForSeconds(2f);
        specialSkillVideoPlayer.Stop();
        specialSkillVideoPlayer.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        videoOn = false;
    }
}