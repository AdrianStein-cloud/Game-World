using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    public static Fade Instance { get; private set; }

    int childIndex;

    private void Awake()
    {
        Instance = this;
        childIndex = transform.GetSiblingIndex();
    }

    public async void SetFade(bool value, float duration, bool fadeAll = true, Color color = default)
    {
        if (color == default) color = Color.black;
        var image = GetComponent<Image>();
        transform.SetSiblingIndex(fadeAll ? transform.parent.childCount - 2 : childIndex);

        var timeElapsed = 0f;
        var startColor = image.color;

        while (timeElapsed < duration)
        {
            image.color = Color.Lerp(startColor, color, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            await Awaitable.NextFrameAsync(destroyCancellationToken);
        }

        image.color = color;
    }
}
