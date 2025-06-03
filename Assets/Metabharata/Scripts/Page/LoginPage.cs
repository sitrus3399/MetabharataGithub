using UnityEngine;
using UnityEngine.UI;

public class LoginPage : Page
{
    [SerializeField] private Button signInButton;

    protected override void Start()
    {
        base.Start();

        signInButton.onClick.AddListener(() => { SignIn(); });
    }

    void SignIn()
    {
        GooglePlayManager.Manager.SignIn();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        GooglePlayManager.afterSignIn += Hide;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        GooglePlayManager.afterSignIn -= Hide;
    }

    protected override void Update()
    {
        base.Update();
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
