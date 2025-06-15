using System.Collections.Generic;
using UnityEngine;

public class WidgetManager : MonoBehaviour
{
    [SerializeField] private List<Widget> widgets;

    public void OpenWidget(WidgetType tmpType)
    {
        foreach (Widget widget in widgets)
        {
            if (widget.type == tmpType)
            {
                widget.Show();
            }
            else
            {
                widget.Hide();
            }
        }
    }
}

public enum WidgetType
{
    Empty,
    Setting,
    Credit,
    DailyCheckIn,
    Pause,
    EndGame,
    ConfirmBuy,
    PinRoom,
    ExitRoom,
    ChangeAccount,
    EndGameOnline
}