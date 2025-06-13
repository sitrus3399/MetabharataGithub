using System.Collections.Generic;
using UnityEngine;

public class LeaderboardPage : MonoBehaviour
{
    [SerializeField] private LeaderboardCard leaderboardCard;
    [SerializeField] private List<LeaderboardCard> leaderboardCardList;
    [SerializeField] private Transform leaderboardCardLocation;

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
            newCard.gameObject.SetActive(true);
            leaderboardCardList.Add(newCard);
        }
    }
}
