using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{

    public GameObject player;

    PlayerController playerController;

    //------------------------------

    public GameObject oxygenBar;
    public GameObject targetTracker;

    public GameObject itemName;
    public GameObject itemCredits;
    public GameObject itemBackground;

    public GameObject crosshair;

    public GameObject lowOxygenOverlay;
    public GameObject restoreOxygenOverlay;

    public GameObject restoreParticlesObject;
    public GameObject restoreParticlesRingObject;

    public GameObject targetReachedObject;

    public AudioClip victory;

    //------------------------------

    public float lowOxygenVolume = 0.5f;

    //------------------------------

    CanvasGroup canvasGroup;
    CanvasGroup targetReachedCanvas;
    AudioSource lowOxygenAudio;

    Image itemBackgroundImage;
    Image restoreOxygenImage;

    ParticleSystem restoreParticles;
    ParticleSystem restoreParticlesRing;
    
    //------------------------------

    float itemTextFade = 0.0f;
    float itemTextFadeSpeed = 3.0f;

    float restoreFadeIn = 2.0f;
    float restoreFadeOut = 1.5f;

    float lastOxygen = 1.0f;

    bool targetReached = false;

    //------------------------------

    IEnumerator AnimateRestore() {
        // Enables the particle systems
        restoreParticles.Play();
        restoreParticlesRing.Play();

        // While the restoreOxygenImage has an alpha below 1.0, add to it
        while (restoreOxygenImage.color.a < 1.0f) {
            restoreOxygenImage.color = new Color(
                restoreOxygenImage.color.r,
                restoreOxygenImage.color.g,
                restoreOxygenImage.color.b,
                restoreOxygenImage.color.a + restoreFadeIn * Time.deltaTime
            );

            yield return null;
        }

        // Disables the particles
        restoreParticles.Stop();
        restoreParticlesRing.Stop();

        // While the restoreOxygenImage has an alpha below 1.0, add to it
        while (restoreOxygenImage.color.a > 0.0f) {
            restoreOxygenImage.color = new Color(
                restoreOxygenImage.color.r,
                restoreOxygenImage.color.g,
                restoreOxygenImage.color.b,
                restoreOxygenImage.color.a - restoreFadeOut * Time.deltaTime
            );

            yield return null;
        }

        // Disables the particles (second just in case check)
        restoreParticles.Stop();
        restoreParticlesRing.Stop();
    }

    IEnumerator ShowTargetMessage() {
        // Shows the target reached message
        yield return Utils.FadeContainer(targetReachedCanvas, Utils.FadeType.IN, 2.0f);

        // Waits for a few seconds before hiding the message
        yield return new WaitForSeconds(6.0f);

        // Hides the target reached message
        yield return Utils.FadeContainer(targetReachedCanvas, Utils.FadeType.OUT, 2.0f);
    }

    //------------------------------

    // Start is called before the first frame update
    void Start()
    {
        // Gets + sets the player controller
        playerController = player.GetComponent<PlayerController>();

        // Gets this UI's canvas group
        canvasGroup = transform.GetComponent<CanvasGroup>();
        targetReachedCanvas = targetReachedObject.GetComponent<CanvasGroup>();

        // Gets the audio source for the low oxygen sounds.
        lowOxygenAudio = transform.GetComponent<AudioSource>();
        lowOxygenAudio.loop = true;
        lowOxygenAudio.volume = 0.0f;

        // Sets the particle systems
        restoreParticles = restoreParticlesObject.GetComponent<ParticleSystem>();
        restoreParticles.Stop();
        
        restoreParticlesRing = restoreParticlesRingObject.GetComponent<ParticleSystem>();
        restoreParticlesRing.Stop();

        // Gets the oxygen restoration image
        restoreOxygenImage = restoreOxygenOverlay.GetComponent<Image>();
        itemBackgroundImage = itemBackground.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        // Sets the credit target text to the player's credits value and the goal
        Text targetTrackerText = targetTracker.GetComponent<Text>();
        targetTrackerText.text = playerController.GetCredits().ToString() + "/" + playerController.creditTarget.ToString();

        // If the target is reached, run the sequence
        if (playerController.playerLoaded) {
            if (playerController.GetCredits() >= playerController.creditTarget && !targetReached) {
                targetReached = true;

                playerController.playerAudio.PlayOneShot(victory, 0.3f);

                StartCoroutine(ShowTargetMessage());
            }
        }

        // Gets the item text elements
        Text itemNameText = itemName.GetComponent<Text>();
        Text itemCreditsText = itemCredits.GetComponent<Text>();

        // Gets the oxygen bar and crosshair
        RectTransform oxygenBarTransform = oxygenBar.GetComponent<RectTransform>();
        Image oxygenBarImage = oxygenBar.GetComponent<Image>();

        RectTransform crosshairTransform = crosshair.GetComponent<RectTransform>();

        //---------------------------

        // If the player has a selected object, set the text + fade
        if (playerController.selectedObject != null) {
            // Sets the text based on the selected object
            itemNameText.text = playerController.selectedObject.itemName;
            itemCreditsText.text = playerController.selectedObject.GetString();

            // Add to the fade
            itemTextFade += itemTextFadeSpeed * Time.deltaTime;
        } else {
            // Removes from the fade
            itemTextFade -= itemTextFadeSpeed * Time.deltaTime;
        }

        // If the playercontroller is noclipping, then make this invisible
        if (playerController.IsNoclipping())
            canvasGroup.alpha = 0;
        else if (!playerController.GetVictory())
            canvasGroup.alpha = 1;

        //---------------------------

        // Sets the itemNameFade to be clamped between values
        itemTextFade = Mathf.Clamp(itemTextFade, 0.0f, 1.0f);

        // Applies easing to the rotation
        float itemTextTransition = Utils.easeInOutQuint(itemTextFade);

        // Sets the transparency of the item text
        itemNameText.color = new Color(itemNameText.color.r, itemNameText.color.g, itemNameText.color.b, itemTextTransition);
        itemCreditsText.color = new Color(itemCreditsText.color.r, itemCreditsText.color.g, itemCreditsText.color.b, itemTextTransition);
        itemBackgroundImage.color = new Color(itemBackgroundImage.color.r, itemBackgroundImage.color.g, itemBackgroundImage.color.b, itemTextTransition);

        // Sets the rotation of the crosshair
        crosshairTransform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f * itemTextTransition);

        //---------------------------

        // Gets the player's oxygen
        float playerOxygen = playerController.GetOxygen();

        // Sets the oxygen bar transform
        oxygenBarTransform.localScale = new Vector3(playerOxygen, 1.0f, 1.0f);

        // Sets the color of the oxygen bar
        if (playerOxygen < 0.2f && playerOxygen > 0.0f) {
            if (!lowOxygenAudio.isPlaying)
                lowOxygenAudio.Play();

            lowOxygenAudio.volume = lowOxygenVolume;

            oxygenBarImage.color = new Color(1.0f, 0.4f, 0.4f, 1.0f);
        }
        else {
            lowOxygenAudio.volume = 0.0f;
            lowOxygenAudio.Stop();

            oxygenBarImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }

        // Gets the low oxygen image, sets the transparency based on the oxygen levels
        Image lowOxygenImage = lowOxygenOverlay.GetComponent<Image>();
        lowOxygenImage.color = new Color(
            lowOxygenImage.color.r, lowOxygenImage.color.g, lowOxygenImage.color.b,
            1.0f - Mathf.Pow(playerOxygen, 0.5f)
        );
        
        // If the last oxygen value is under the player's current oxygen value, restore
        if (lastOxygen < playerOxygen) {
            StartCoroutine(AnimateRestore());
        }

        //---------------------------

        // Sets the last oxygen value
        lastOxygen = playerOxygen;
    }
}
