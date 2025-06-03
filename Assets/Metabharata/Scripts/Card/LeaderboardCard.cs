using TMPro;
using UnityEngine;

public class LeaderboardCard : MonoBehaviour
{
    private int number;
    private string namePlayer;
    private int rank;
    private bool isTop;

    [SerializeField] private TMP_Text numberText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private GameObject topBar;

    public void InitData(int tmpNumber, string tmpNamePlayer, int tmpRank, bool tmpIsTop)
    {
        number = tmpNumber;
        namePlayer = tmpNamePlayer;
        rank = tmpRank;
        isTop = tmpIsTop;

        numberText.text = tmpNumber.ToString();
        nameText.text = tmpNamePlayer;
        rankText.text = tmpRank.ToString();
        topBar.SetActive(tmpIsTop);
    }
}
