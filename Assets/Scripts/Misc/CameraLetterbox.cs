using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class Letterbox : MonoBehaviour
{
    [Tooltip("Width / Height, e.g. 2.35f for a 2.35:1 cinematic look")]
    public float targetAspect = 2.35f;

    Camera _cam;

    void Awake() => _cam = GetComponent<Camera>();

    void Update()
    {
        // current window aspect
        float windowAspect = (float)Screen.width / Screen.height;
        // scale relative to target
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1f)
        {
            // letterbox: bars top & bottom
            Rect r = new Rect(0, (1f - scaleHeight) / 2f, 1f, scaleHeight);
            _cam.rect = r;
        }
        else
        {
            // pillarbox: bars left & right (if window is too wide)
            float scaleWidth = 1f / scaleHeight;
            Rect r = new Rect((1f - scaleWidth) / 2f, 0, scaleWidth, 1f);
            _cam.rect = r;
        }
    }
}
