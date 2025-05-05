using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    [HideInInspector]
    public bool start = false;
    [HideInInspector]
    public float fadeInDamp = 0.0f; // Renamed from fadeDamp
    [HideInInspector]
    public float fadeOutDamp = 0.0f; // Added fadeOutDamp
    [HideInInspector]
    public int fadeSceneBuildIndex;
    [HideInInspector]
    public float alpha = 0.0f;
    [HideInInspector]
    public Color fadeColor;
    [HideInInspector]
    public bool isFadeIn = false;
    CanvasGroup myCanvas;
    Image bg;
    float lastTime = 0;
    bool startedLoading = false;

    //Set callback
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    //Remove callback
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    public void InitiateFader()
    {
        DontDestroyOnLoad(gameObject);

        //Getting the visual elements
        if (transform.GetComponent<CanvasGroup>())
            myCanvas = transform.GetComponent<CanvasGroup>();

        if (transform.GetComponentInChildren<Image>())
        {
            bg = transform.GetComponent<Image>();
            bg.color = fadeColor;
        }

        //Checking and starting the coroutine
        if (myCanvas && bg)
        {
            myCanvas.alpha = 0.0f;
            StartCoroutine(FadeIt());
        }
        else
            Debug.LogWarning("Something is missing please reimport the package.");
    }

    IEnumerator FadeIt()
    {
        while (!start)
        {
            //waiting to start
            yield return null;
        }
        lastTime = Time.time;
        float coDelta = lastTime;
        bool hasFadedIn = false;

        while (!hasFadedIn)
        {
            coDelta = Time.time - lastTime;
            if (!isFadeIn)
            {
                //Fade in (to alpha 1)
                alpha = newAlpha(coDelta, 1, alpha, fadeInDamp); // Pass fadeInDamp
                if (alpha == 1 && !startedLoading)
                {
                    startedLoading = true;
                    SceneManager.LoadScene(fadeSceneBuildIndex);
                }
            }
            else
            {
                //Fade out (to alpha 0)
                alpha = newAlpha(coDelta, 0, alpha, fadeOutDamp); // Pass fadeOutDamp
                if (alpha == 0)
                {
                    hasFadedIn = true;
                }
            }
            lastTime = Time.time;
            myCanvas.alpha = alpha;
            yield return null;
        }

        Initiate.DoneFading();

        Debug.Log("Your scene has been loaded , and fading in has just ended");

        Destroy(gameObject);

        yield return null;
    }

    // Updated to accept damp parameter
    float newAlpha(float delta, int to, float currAlpha, float damp)
    {
        switch (to)
        {
            case 0: // Fading out
                currAlpha -= damp * delta; // Use passed damp value
                if (currAlpha <= 0)
                    currAlpha = 0;

                break;
            case 1: // Fading in
                currAlpha += damp * delta; // Use passed damp value
                if (currAlpha >= 1)
                    currAlpha = 1;

                break;
        }

        return currAlpha;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        // We don't need to start the coroutine again here,
        // the existing one continues after LoadScene completes.
        // StartCoroutine(FadeIt()); // Remove this line
        //We can now fade out
        isFadeIn = true;
    }
}
