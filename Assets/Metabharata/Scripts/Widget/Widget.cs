using UnityEngine;

public class Widget : MonoBehaviour
{
    public WidgetType type;
    public Vector3 pointShow;
    public Vector3 pointHide;
    public float speed = 2.0f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float lerpProgress = 0f;

    public bool isShow;

    [SerializeField] protected WidgetManager widgetManager;

    protected virtual void Update()
    {
        if ((isShow && targetPosition != pointShow) || (!isShow && targetPosition != pointHide))
        {
            startPosition = transform.localPosition;
            targetPosition = isShow ? pointShow : pointHide;
            lerpProgress = 0f;
        }

        if (lerpProgress < 1f)
        {
            lerpProgress += Time.unscaledDeltaTime * speed;
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, lerpProgress);
        }
    }

    public virtual void Show()
    {
        isShow = true;
    }

    public virtual void Hide()
    {
        isShow = false;
    }
}
