using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using GooglePlayGames;
using UnityEngine.UI;
using System.Collections;

public class TopBar : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text userIDText;

    public Image avatarImage;
    public Sprite defaultAvatarSprite;

    void Start()
    {
        coinText.text = PlayerManager.Manager.Coin.ToString();
        PlayerManager.Manager.changeCoinAction += UpdateCoin;

        nameText.text = "";
        userIDText.text = "";
        Invoke("GetIDData", 1f);

        //Avatar Image
        string imageUrl = PlayGamesPlatform.Instance.GetUserImageUrl();

        if (!string.IsNullOrEmpty(imageUrl))
        {
            StartCoroutine(DownloadAndSetAvatar(imageUrl));
            Debug.LogWarning("URL avatar ada");
        }
        else
        {
            // URL kosong, gunakan default
            avatarImage.sprite = defaultAvatarSprite;
            Debug.LogWarning("URL avatar kosong, gunakan default.");
        }
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

    IEnumerator DownloadAndSetAvatar(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (request.result != UnityWebRequest.Result.Success)
#else
        if (request.isNetworkError || request.isHttpError)
#endif
        {
            Debug.LogWarning("Gagal memuat avatar, gunakan default. Error: " + request.error);
            avatarImage.sprite = defaultAvatarSprite;
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Sprite sprite = Sprite.Create(texture, rect, pivot);
            avatarImage.sprite = sprite;
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
