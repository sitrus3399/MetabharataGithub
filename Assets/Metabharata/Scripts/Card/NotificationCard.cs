using TMPro;
using UnityEngine;

public class NotificationCard : MonoBehaviour
{
    [SerializeField] private TMP_Text tittleText;
    [SerializeField] private TMP_Text descriptionText;

    public void InitText(string tittle, string description)
    {
        tittleText.text = tittle;
        descriptionText.text = description;
    }
}