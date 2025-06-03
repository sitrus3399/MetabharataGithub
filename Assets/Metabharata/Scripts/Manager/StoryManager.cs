using System.Collections.Generic;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Manager;
    [SerializeField] private List<StoryList> story;
    public List<StoryList> Story { get { return story; } }

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
}

[System.Serializable]
public class StoryList
{
    public string storyName;
    public StoryData data;
    public bool isCleared;
    public bool isUnlocked;
}
