using TMPro;
using UnityEngine;

public class TopBar : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText;

    void Start()
    {
        coinText.text = PlayerManager.Manager.Coin.ToString();
        PlayerManager.Manager.changeCoinAction += UpdateCoin;
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
