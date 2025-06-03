using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardPage : MonoBehaviour
{
    [SerializeField] private OnlinePage onlinePage;
    [SerializeField] private LeaderboardCard leaderboardCard;
    [SerializeField] private List<LeaderboardCard> leaderboardCardList;
    [SerializeField] private Transform leaderboardCardLocation;

    private void Start()
    {
        
    }

    public void CreadeListCard()
    {
        foreach (var card in leaderboardCardList)
        {
            Destroy(card.gameObject);
        }

        leaderboardCardList.Clear();

        for (int i = 0; i < 6; i++)
        {
            LeaderboardCard newCard = Instantiate(leaderboardCard, leaderboardCardLocation);

            leaderboardCardList.Add(newCard);
        }
    }
}
