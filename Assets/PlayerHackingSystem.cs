using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHackingSystem : MonoBehaviour
{
    bool enabled = true;
    bool hacking = false;
    float timebetweenHackToggle = 0.2f;
    float lastHackToggle;
    Transform mainCamera;

    [SerializeField] GameObject hackingUI;

    [SerializeField] LayerMask interactionLayer;
    [SerializeField] float interactionDistance;

    [SerializeField] public Hackable hackingTarget { get; private set; }

    [SerializeField] GameObject hackingTargetIndicator;

    private float currentHackingProgress;
    private float hackingTime = 2.5f;

    private float lastHackingSoundTime;
    private float hackingSoundTime = 0.35f;

    bool hasScanned = false;

    [SerializeField] TextMeshProUGUI hackingProgressText;
    [SerializeField] GameObject hackingProgressSlider;
    [SerializeField] GameObject hackingProgressGO;

    [SerializeField] GameObject zombieInformationGO;
    [SerializeField] TextMeshProUGUI zombieInformationText;
    [SerializeField] TextMeshProUGUI zombieAdditionalInformationText;

    [SerializeField] TextMeshProUGUI zombieSoundclipNameText;

    [SerializeField] GameObject nametagPrefab;

    List<GameObject> nametags = new List<GameObject>();
    List<Hackable> scannedZombies = new List<Hackable>();

    [SerializeField] Animator hackingOptionsAnim;

    [SerializeField] AudioClip hackingDoneClip, scanningCompleteClip, doingScanningClip;

    [SerializeField] Material hackingShader;
    [SerializeField] Animator panelAnim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && enabled && Time.time >= lastHackToggle + timebetweenHackToggle)
        {
            if (!hacking) BeginHacking();
            else StopHacking();

            lastHackToggle = Time.time;
            hacking = !hacking;
        }

        if (hacking && Input.GetKeyDown(KeyCode.Mouse1) && hasScanned && !hackingTarget.hacked)
        {
            Hack();
        }

        if (hacking)
        {
            FindHackingTarget();
        }
    }

    void FindHackingTarget()
    {
        var hasHit = Physics.Raycast(mainCamera.position, mainCamera.forward, out RaycastHit hit, interactionDistance, interactionLayer);

        // Looking at interactable
        if (hasHit && hit.transform.TryGetComponent(out Hackable interactable))
        {
            if (hackingTarget != interactable)
            {
                hackingTarget?.EndOutline();
                hackingTarget = interactable;

                hackingTarget.BeginOutline();
                zombieInformationGO.SetActive(false);
                BeginScanning();
                UpdateHackingTargetIndicator(hackingTarget.gameObject);
            }
            else
            {
                UpdateHacktimer();
            }
        }
        else
        {
            hackingOptionsAnim.SetBool("Hacking", false);

            hackingTarget?.EndOutline();

            currentHackingProgress = 0;
            hasScanned = false;
            zombieInformationGO.SetActive(false);
            hackingProgressGO.SetActive(false);
            UpdateHackingTargetIndicator(null);
            hackingTarget = null;
        }
    }

    void UpdateHacktimer()
    {
        if (currentHackingProgress >= hackingTime) ScanningComplete();
        currentHackingProgress += Time.deltaTime;
        currentHackingProgress = Mathf.Clamp(currentHackingProgress, 0, hackingTime);

        if (!hasScanned && Time.time > lastHackingSoundTime + hackingSoundTime)
        {
            lastHackingSoundTime = Time.time;
            AudioSource.PlayClipAtPoint(doingScanningClip, Camera.main.transform.position, 0.1f);
        }

        float progress = (int)((currentHackingProgress / hackingTime) * 100);
        progress = Mathf.Clamp(progress, 0, 100);
        hackingProgressSlider.transform.localScale = new Vector3(progress / 100, 1, 1);
        hackingProgressText.text = progress.ToString() + "%";
    }

    void UpdateHackingTargetIndicator(GameObject go)
    {
        if (go == null)
        {
            hackingTargetIndicator.SetActive(false);
            hackingTargetIndicator.GetComponentInChildren<Billboard>().Detach();
            return;
        }

        hackingTargetIndicator.SetActive(true);
        hackingTargetIndicator.GetComponentInChildren<Billboard>().UpdateBillboard(go);
    }

    void HighlightZombies(bool highlight)
    {
        foreach (var zombie in GameObject.FindGameObjectsWithTag("Zombie"))
        {
            if (zombie.TryGetComponent(out Hackable hackable))
            {
                if (highlight)
                {
                    hackable.BeginOutline();
                }
                else
                {
                    hackable.EndOutline();
                }
            }
        }
    }

    void ToggleNametags(bool toggle)
    {
        foreach (var tag in nametags)
        {
            tag.SetActive(toggle);
        }
    }

    void AddNameTag(string name)
    {
        var go = Instantiate(nametagPrefab);
        go.GetComponentInChildren<Billboard>().UpdateBillboard(hackingTarget.gameObject);
        go.GetComponentInChildren<TextMeshProUGUI>().text = name;
        nametags.Add(go);
    }

    void BeginHacking()
    {
        ToggleNametags(true);
        hackingUI.SetActive(true);
        PostProcessingHandler.SetChromaticAberration(0.5f, 1f);
        //HighlightZombies(true);
        panelAnim.SetBool("Hacking", true);
    }

    void StopHacking()
    {
        hackingTarget?.EndOutline();
        ToggleNametags(false);
        hackingUI.SetActive(false);
        PostProcessingHandler.SetChromaticAberration(0.5f, 0.05f);
        //HighlightZombies(false);

        UpdateHackingTargetIndicator(null);
        hackingTarget = null;
        panelAnim.SetBool("Hacking", false);
    }

    void BeginScanning()
    {
        hackingOptionsAnim.SetBool("Hacking", false);

        hasScanned = false;

        if (scannedZombies.Contains(hackingTarget))
        {
            ScanningComplete();
            return;
        }

        currentHackingProgress = 0;
        hackingProgressGO.SetActive(true);
    }

    void ScanningComplete()
    {
        if (hasScanned) return; // So its called once per hover.
        hasScanned = true;

        if (!hackingTarget.hacked) hackingOptionsAnim.SetBool("Hacking", true);

        // Not already scanned
        if (!scannedZombies.Contains(hackingTarget))
        {
            AudioSource.PlayClipAtPoint(scanningCompleteClip, Camera.main.transform.position);

            scannedZombies.Add(hackingTarget);
            hackingTarget.scanned = true;

            hackingTarget.EndOutline();
            hackingTarget.BeginOutline();

            AddNameTag(hackingTarget.hackingInfo.Name);
        }

        hackingProgressGO.SetActive(false);
        zombieInformationGO.SetActive(true);

        var hackingInfo = hackingTarget.hackingInfo;

        var zombieInfoText = "Name: " + hackingInfo.Name + "\n" + "Age: " + hackingInfo.Age + "\n" + "Blood: " + BloodTypeHelper.GetLabel(hackingInfo.Bloodtype);
        zombieInformationText.text = zombieInfoText;

        zombieSoundclipNameText.text = hackingInfo.HackedSoundName;

        var additionalInfoText = string.Empty;
        foreach (var item in hackingInfo.HackedInformation)
        {
            additionalInfoText += item + "\n";
        }
        zombieAdditionalInformationText.text = additionalInfoText;
    }

    public void Hack()
    {
        // Hack target
        if (!hackingTarget.Hack()) return;

        StartCoroutine(DoHackEffect());

        AudioSource.PlayClipAtPoint(hackingDoneClip, Camera.main.transform.position);
        AudioSource.PlayClipAtPoint(hackingTarget.hackingInfo.HackedSoundClip, Camera.main.transform.position);
        hackingOptionsAnim.SetBool("Hacking", false);

        StopHacking();
        hacking = false;
    }

    IEnumerator DoHackEffect()
    {
        hackingShader.SetFloat("_HackerMinDrawDistance", 10f);
        hackingShader.SetFloat("_HackerMaxDrawDistance", 20f);
        hackingShader.SetFloat("_HackerOverallAlpha", 1f);

        float steps = 30;
        for (float i = 0; i <= steps; i++)
        {
            hackingShader.SetFloat("_HackerMinDrawDistance", 10f - (i / steps * 10f));

            yield return new WaitForSeconds((0.075f/3f));
        }

        yield return new WaitForSeconds(1f);

        for (float i = 0; i <= steps; i++)
        {
            hackingShader.SetFloat("_HackerMaxDrawDistance", 20f - (i / steps * 20f));

            yield return new WaitForSeconds((0.125f/3f));
        }

        hackingShader.SetFloat("_HackerOverallAlpha", 0f);
    }

    public void EnableHackingAbility()
    {
        // Enable hacking ability
    }

    private void OnDestroy()
    {
        hackingShader.SetFloat("_HackerOverallAlpha", 0f);
    }

    private void OnApplicationQuit()
    {
        hackingShader.SetFloat("_HackerOverallAlpha", 0f);
    }
}
