using UnityEngine;
using UnityEngine.UI;

public class CharacterUIController : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    public Button punchButton, kickButton, specialAttackButton, weaponAttackButton, transformLegendaryButton;
    public Joystick joystick;

    void Start()
    {
        punchButton.onClick.AddListener(characterController.Punch);
        kickButton.onClick.AddListener(characterController.Kick);
        specialAttackButton.onClick.AddListener(characterController.SpecialAttack);
        weaponAttackButton.onClick.AddListener(characterController.WeaponAttack);
        transformLegendaryButton.onClick.AddListener(characterController.TransformLegend);
    }

    public void SetPlayer(CharacterController tmpCharacterController, CharacterBase characterBase)
    {
        characterController = tmpCharacterController;
        SetNonActiveButton(characterBase.BaseData);
    } 

    public void SetNonActiveButton(CharacterData data)
    {
        if (data.characterType == CharacterType.Prajurit)
        {
            specialAttackButton.gameObject.SetActive(false);
        }

        if (data.characterType != CharacterType.Legendary)
        {
            transformLegendaryButton.gameObject.SetActive(false);
        }
    }
}