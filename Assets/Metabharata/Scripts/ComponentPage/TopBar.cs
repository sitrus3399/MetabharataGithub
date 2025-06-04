using TMPro;
using UnityEngine;
using GooglePlayGames;

public class TopBar : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text userIDText;

    void Start()
    {
        coinText.text = PlayerManager.Manager.Coin.ToString();
        PlayerManager.Manager.changeCoinAction += UpdateCoin;

        nameText.text = "";
        userIDText.text = "";
        Invoke("GetIDData", 1f);
    }

    void GetIDData()
    {
        nameText.text = PlayGamesPlatform.Instance.GetUserDisplayName();
        userIDText.text = $"User ID: {PlayGamesPlatform.Instance.GetUserId()} " ;
        Debug.Log($"Name {PlayGamesPlatform.Instance.GetUserDisplayName()}");
        Debug.Log($"ID {PlayGamesPlatform.Instance.GetUserId()}");

        if (nameText.text == "")
        {
            nameText.text = "Guess";
        }
    }    

    public void UpdateCoin(int coin)
    {
        coinText.text = coin.ToString();
    }

    private void OnDestroy()
    {
        PlayerManager.Manager.changeCoinAction -= UpdateCoin;
    }
}
