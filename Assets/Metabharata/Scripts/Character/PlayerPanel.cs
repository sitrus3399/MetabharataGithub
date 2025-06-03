using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPanel : MonoBehaviour
{
    [SerializeField] TMP_Text characterName;
    [SerializeField] Image characterIcon;
    [SerializeField] Image hpBar;
    [SerializeField] Image[] skillBar;
    [SerializeField] Image[] scoreImage;
    [SerializeField] Sprite scoreBase;
    [SerializeField] Sprite scoreWin;
    [SerializeField] Sprite scoreLose;

    public void InitData(CharacterData characterData)
    {
        characterName.text = characterData.characterName;
        characterIcon.sprite = characterData.characterSprite;
    }

    public void SetRestart()
    {
        hpBar.fillAmount = 1;
        foreach (Image bar in skillBar)
        {
            bar.fillAmount = 0;
        }
    }

    public void SetHPBar(float current, float max)
    {
        hpBar.fillAmount = current / max;
    }

    public void SetSkillBar(float current, float max, float point)
    {
        foreach (Image bar in skillBar)
        {
            bar.fillAmount = 0;
        }

        switch (point)
        {
            case 0:
                skillBar[0].fillAmount = current / max;
                break;
            case 1:
                skillBar[0].fillAmount = 1;
                skillBar[1].fillAmount = current / max;
                break;
            case 2:
                skillBar[0].fillAmount = 1;
                skillBar[1].fillAmount = 1;
                skillBar[2].fillAmount = current / max;
                break;
            case 3:
                skillBar[0].fillAmount = 1;
                skillBar[1].fillAmount = 1;
                skillBar[2].fillAmount = 1;
                break;
            default:
                break;
        }
    }

    public void SetScoreImage(List<int> score, int roundNumber, bool isLeft)
    {
        foreach (Image tmpImage in scoreImage)
        {
            tmpImage.sprite = scoreBase;
        }

        if (isLeft)
        {
            for (int i = 0; i < roundNumber; i++)
            {
                if (score[i] == 1)
                {
                    scoreImage[i].sprite = scoreWin;
                }
                else
                {
                    scoreImage[i].sprite = scoreLose;
                }
            }
        }
        else
        {
            for (int i = 0; i < roundNumber; i++)
            {
                if (score[i] == 2)
                {
                    scoreImage[i].sprite = scoreWin;
                }
                else
                {
                    scoreImage[i].sprite = scoreLose;
                }
            }
        }
    }
}