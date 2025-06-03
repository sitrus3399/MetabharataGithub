using UnityEngine;
using UnityEngine.Events;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Manager;
    [SerializeField] private int coin;
    [SerializeField] private int storyUnlock;
    
    public int Coin {  get { return coin; } }
    public int StoryUnlock {  get { return storyUnlock; } }

    public UnityAction<int> changeCoinAction;

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

    void Start()
    {
        changeCoinAction?.Invoke(coin);
    }

    void Update()
    {
        
    }

    public void AddCoin(float addCoin)
    {
        coin += (int)addCoin;

        changeCoinAction?.Invoke(coin);
    }

    public void ReduceCoin(float reduceCoin)
    {
        coin -= (int)reduceCoin;
        changeCoinAction?.Invoke(coin);
    }

    public void UnlockStory(int storyNumber)
    {
        storyUnlock = storyNumber;
    }
}
