using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PinWidget : Widget
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button createPinButton;
    [SerializeField] private TMP_Text pin1;
    [SerializeField] private TMP_Text pin2;
    [SerializeField] private TMP_Text pin3;
    [SerializeField] private TMP_Text pin4;
    [SerializeField] private TMP_InputField inputPIN;
    public int PIN { get; private set; }

    private void Start()
    {
        closeButton.onClick.AddListener(() => { Hide(); });
        inputPIN.onValueChanged.AddListener(UpdateCharacterDisplays);
    }

    void UpdateCharacterDisplays(string input)
    {
        PIN = int.Parse(input);

        pin1.text = input.Length > 0 ? input[0].ToString() : "";
        pin2.text = input.Length > 1 ? input[1].ToString() : "";
        pin3.text = input.Length > 2 ? input[2].ToString() : "";
        pin4.text = input.Length > 3 ? input[3].ToString() : "";
    }

    public override void Show()
    {
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
    }
}
