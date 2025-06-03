using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStory", menuName = "Data/StoryData", order = 1)]
public class StoryData : ScriptableObject
{
    public string storyName;
    public int storyNumber;
    public string storyDescription;
    public Sprite storyIcon;
    public CharacterData characterData;
    public int characterLevel;
    public CharacterData enemyBossData;
    public List<StageData> stageList;
    public Sprite backgroundSprite;
}

[System.Serializable]
public class StageData
{
    public CharacterData enemy;
    public int level;
    public int reward;
}