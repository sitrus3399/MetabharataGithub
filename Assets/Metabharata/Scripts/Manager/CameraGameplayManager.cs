using UnityEngine;

public class CameraGameplayManager : MonoBehaviour
{
    public float minSize = 5f;
    public float maxSize = 10f;

    public float zoomSpeed = 5f;
    public float followSpeed = 5f;

    public float groundOffset = 3f; // Jaga agar ground selalu kelihatan
    public float leftLimit = -20f;
    public float rightLimit = 20f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (GameplayManager.Manager.CharacterOnGameplay[0] == null || GameplayManager.Manager.CharacterOnGameplay[1].transform.position == null) return;

        // Hitung titik tengah
        Vector3 midpoint = (GameplayManager.Manager.CharacterOnGameplay[0].transform.position + GameplayManager.Manager.CharacterOnGameplay[1].transform.position) / 2f;

        // Hitung jarak antar player dan enemy
        float distance = Vector2.Distance(GameplayManager.Manager.CharacterOnGameplay[0].transform.position, GameplayManager.Manager.CharacterOnGameplay[1].transform.position);

        // Hitung ukuran kamera berdasarkan jarak (zoom)
        float targetSize = Mathf.Clamp(distance, minSize, maxSize);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);

        // Offset bawah agar ground tetap terlihat
        float cameraHeight = cam.orthographicSize;
        float cameraVerticalOffset = cameraHeight - groundOffset;

        Vector3 targetPosition = new Vector3(midpoint.x, transform.position.y, transform.position.z);
        //Vector3 targetPosition = new Vector3(midpoint.x, midpoint.y + cameraVerticalOffset, transform.position.z);

        // Batasi gerakan kamera horizontal berdasarkan batas dan zoom
        float cameraWidth = cam.orthographicSize * cam.aspect;
        targetPosition.x = Mathf.Clamp(targetPosition.x, leftLimit + cameraWidth, rightLimit - cameraWidth);

        // Gerakkan kamera ke posisi target dengan smoothing
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
    }
}
