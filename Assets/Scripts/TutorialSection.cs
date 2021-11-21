using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TutorialSection : MonoBehaviour
{
    public GameObject player;
    public GameObject playerCam;
    public GameObject proceduralTerrainObject;
    public GameObject tipContainer;
    public GameObject controlsContainer;

    public AudioClip successSound;
    public float successSoundVolume;

    public GameObject tipIconObject;
    public GameObject tipTextObject;

    public GameObject[] controlObjects;

    public GameObject arrowObject;

    public float helpArrowLength = 0.4f;
    public float helpArrowOrigin = 0.2f;
    public float helpArrowWidth = 0.2f;
    public Vector3 arrowOffset = new Vector3(0.0f, 0.6f, 0.0f);
    
    //---------------------------

    PlayerController playerController;
    ProceduralTerrain proceduralTerrain;

    CanvasGroup tipCanvas;
    CanvasGroup controlsCanvas;
    CanvasGroup arrowCanvas;

    Image arrow;
    Image tipIcon;
    Text tipText;
    
    Dictionary<string, Image> controlImages = new Dictionary<string, Image>();
    Dictionary<string, Text> controlTexts = new Dictionary<string, Text>();

    //---------------------------

    uint sequenceNumber = 0;
    bool tutorialFinished = false;
    bool endTutorial = false;

    //---------------------------

    Vector3 arrowPos;

    bool hasMoved;
    bool hasLooked;
    bool hasSprinted;
    bool hasJumped;

    //---------------------------

    // Indicates that a tip was completed successfully
    IEnumerator ShowTipSuccess(float fadeRate) {
        // Sets the greenery value, which will be used to lerp into
        // the "green-ness" of the success sequence
        float greenery = 0;

        tipIcon.sprite = Resources.Load<Sprite>("UI/TutorialIconTick");

        while (greenery < 1.0f) {
            // Sets the greenery
            greenery += fadeRate * Time.deltaTime;

            // Sets the tip icon color + tip text color
            tipIcon.color = Color.Lerp(
                new Color(1.0f, 1.0f, 1.0f),
                new Color(0.75f, 1.0f, 0.75f),
                greenery
            );

            tipText.color = Color.Lerp(
                new Color(1.0f, 1.0f, 1.0f),
                new Color(0.75f, 1.0f, 0.75f),
                greenery
            );

            yield return null;
        }
    }

    // Indicates that controls were used successfully
    IEnumerator ShowControlsSuccess(float fadeRate, string controlID) {
        // Sets the greenery value, which will be used to lerp into
        // the "green-ness" of the success sequence
        float greenery = 0;

        while (greenery < 1.0f) {
            // Sets the greenery
            greenery += fadeRate * Time.deltaTime;

            // Sets the control icon colors + text colors
            controlImages[controlID].color = Color.Lerp(
                new Color(1.0f, 1.0f, 1.0f),
                new Color(0.75f, 1.0f, 0.75f),
                greenery
            );

            controlTexts[controlID].color = Color.Lerp(
                new Color(1.0f, 1.0f, 1.0f),
                new Color(0.75f, 1.0f, 0.75f),
                greenery
            );

            yield return null;
        }
    }

    // Resets the tip colors
    void ResetTipColors() {
        tipIcon.color = new Color(1.0f, 1.0f, 1.0f);
        tipText.color = new Color(1.0f, 1.0f, 1.0f);
    }

    //---------------------------

    // Start is called before the first frame update
    void Start() {
        // Initialises the relevant variables for the tutorial to work
        playerController = player.GetComponent<PlayerController>();
        proceduralTerrain = proceduralTerrainObject.GetComponent<ProceduralTerrain>();

        tipCanvas = tipContainer.GetComponent<CanvasGroup>();
        controlsCanvas = controlsContainer.GetComponent<CanvasGroup>();

        tipIcon = tipIconObject.GetComponent<Image>();
        tipText = tipTextObject.GetComponent<Text>();
        
        foreach (GameObject controlObject in controlObjects) {
            string parentName = controlObject.name;

            // Gets the text + image objects
            Image controlImage = controlObject.transform.Find("ControlImage").GetComponent<Image>();
            Text controlText = controlObject.transform.Find("ControlText").GetComponent<Text>();

            // Sets the control image + text for this object
            controlImages.Add(parentName, controlImage);
            controlTexts.Add(parentName, controlText);
        }

        // Adds an image to the arrow
        arrow = arrowObject.GetComponent<Image>();
        arrow.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        // If we're on hardcore, we can skip the tutorial
        string difficultyLevel = PlayerPrefs.GetString("Difficulty");

        if (difficultyLevel == "HARDCORE") {
            endTutorial = true;
        }

        // Starts the tutorial coroutine
        StartCoroutine(RunTutorial());
    }

    private void OnEnable() {
        Application.onBeforeRender += UpdatePosition;
    }
    
    private void OnDisable() {
        Application.onBeforeRender -= UpdatePosition;
    }

    void Update() {
        // Checks if the tutorial needs to be skipped
        if (Keyboard.current[Key.E].IsPressed()) {
            endTutorial = true;
        }
    }

    //---------------------------

    void UpdatePosition() {
        // Calculates the position direction
        Vector3 positionDir = new Vector3(
            (arrowPos - player.transform.position).x, 
            0.0f,
            (arrowPos - player.transform.position).z
        ).normalized;

        Quaternion overallRotation = Quaternion.FromToRotation(playerCam.transform.rotation * new Vector3(1.0f, 0.0f, 0.0f), positionDir);
        arrow.rectTransform.rotation = Quaternion.Euler(0.0f, 0.0f, -overallRotation.eulerAngles.y);
    }

    // Coroutine for running the tutorial
    IEnumerator RunTutorial()
    {
        while (!tutorialFinished) {
            // Handles checking if the player has made a certain movement
            if (sequenceNumber == 0 && playerController.HasMoved() && !hasMoved) {
                StartCoroutine(ShowControlsSuccess(8.0f, "MovePlayer"));
                hasMoved = true;
            }

            if (sequenceNumber == 0 && playerController.HasLooked() && !hasLooked) {
                StartCoroutine(ShowControlsSuccess(8.0f, "MoveCamera"));
                hasLooked = true;
            }
            
            if (sequenceNumber == 0 && playerController.HasSprinted() && !hasSprinted) {
                StartCoroutine(ShowControlsSuccess(8.0f, "Sprint"));
                hasSprinted = true;
            }
            
            if (sequenceNumber == 0 && playerController.HasJumped() && !hasJumped) {
                StartCoroutine(ShowControlsSuccess(8.0f, "Jump"));
                hasJumped = true;
            }

            // Performs a check to see if the tutorial is over
            if (endTutorial) {
                // Sets the sequence number to the maximum
                sequenceNumber = 4;

                // Fades out any containers that are visible
                yield return (Utils.FadeContainer(controlsCanvas, Utils.FadeType.OUT, 2.5f));
                yield return (Utils.FadeContainer(tipCanvas, Utils.FadeType.OUT, 2.5f));

                // Disables the path renderer
                arrow.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            }

            // If the sequence number is zero and the player has just achieved "full movement",
            // move on to the next stage
            if (sequenceNumber == 0 && hasMoved && hasLooked && hasJumped && hasSprinted) {
                // Plays the success sound
                playerController.playerAudio.PlayOneShot(successSound, successSoundVolume);
                
                // Waits for a few seconds after the objective was completed
                yield return new WaitForSeconds(4.0f);

                // Fades out the controls canvas
                yield return Utils.FadeContainer(controlsCanvas, Utils.FadeType.OUT, 1.0f);

                // Increments the sequence number
                sequenceNumber++;

                // Sets the text + tip icon
                tipIcon.sprite = Resources.Load<Sprite>("UI/TutorialIconOxygen");
                tipText.text = "To refill your oxygen, head to the nearest oxygen farm!";

                // Sets the arrow position based on the procedural terrain oxygen station position
                arrowPos = proceduralTerrain.GetStartingOxygen().transform.position;
                arrow.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

                // Fades in the tips canvas
                yield return Utils.FadeContainer(tipCanvas, Utils.FadeType.IN, 1.0f);
            }
            else if (sequenceNumber == 1 && playerController.GetLastInteraction() == "USE_OXYGEN_STATION") {
                // Disables the path renderer
                arrow.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

                // Plays the success sound
                playerController.playerAudio.PlayOneShot(successSound, successSoundVolume);

                // Increments the sequence number
                sequenceNumber++;

                // Plays the success sequence
                yield return ShowTipSuccess(4.0f);

                // Waits for a few seconds after the objective was completed
                yield return new WaitForSeconds(4.0f);

                // Fades out the tips canvas
                yield return Utils.FadeContainer(tipCanvas, Utils.FadeType.OUT, 1.0f);
                
                // Sets the text + tip icon
                ResetTipColors();
                tipIcon.sprite = Resources.Load<Sprite>("UI/TutorialIconFire");
                tipText.text = "Camps also have oxygen farms just like these. Head to the nearest one!";
                
                // Sets the arrow position based on the closest campsite
                Vector3 minPosition = new Vector3(0.0f, 0.0f, 0.0f);
                bool minPositionSet = false;

                List<GameObject> campsites = proceduralTerrain.GetCampsiteOxygen();

                foreach (GameObject campsite in campsites) {
                    // Calculate the distance between the player and the current min point + this new campsite
                    float campsiteDistance = (player.transform.position - campsite.transform.position).magnitude;
                    float minDistance = (player.transform.position - minPosition).magnitude;

                    // If the minPosition isn't set or the magnitude between the player pos and this
                    // campsite is lower than the min position, set
                    if (minPositionSet == false || (campsiteDistance < minDistance)) {
                        minPositionSet = true;
                        minPosition = campsite.transform.position;
                    }
                }

                arrowPos = minPosition;
                arrow.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

                // Fades in the tips canvas
                yield return Utils.FadeContainer(tipCanvas, Utils.FadeType.IN, 1.0f);
            }
            else if (sequenceNumber == 2 && playerController.GetLastInteraction() == "USE_OXYGEN_STATION_CAMP") {
                // Disables the path renderer
                arrow.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

                // Plays the success sound
                playerController.playerAudio.PlayOneShot(successSound, successSoundVolume);

                // Increments the sequence number
                sequenceNumber++;

                // Plays the success sequence
                yield return ShowTipSuccess(4.0f);

                // Waits for a few seconds after the objective was completed
                yield return new WaitForSeconds(4.0f);

                // Fades out the tips canvas
                yield return Utils.FadeContainer(tipCanvas, Utils.FadeType.OUT, 1.0f);
                
                // Sets the text + tip icon
                ResetTipColors();
                tipIcon.sprite = Resources.Load<Sprite>("UI/TutorialIconCredits");
                tipText.text = "You can find treasure under blue holograms, let's pick some up and earn credit!";

                // Fades in the tips canvas
                yield return Utils.FadeContainer(tipCanvas, Utils.FadeType.IN, 1.0f);
            }
            else if (sequenceNumber == 3 && playerController.GetLastInteraction() == "PICKUP_TREASURE") {
                // Plays the success sound
                playerController.playerAudio.PlayOneShot(successSound, successSoundVolume);

                // Increments the sequence number
                sequenceNumber++;

                // Plays the success sequence
                yield return ShowTipSuccess(4.0f);

                // Waits for a few seconds after the objective was completed
                yield return new WaitForSeconds(4.0f);

                // Fades out the tips canvas
                yield return Utils.FadeContainer(tipCanvas, Utils.FadeType.OUT, 1.0f);
                
                // Sets the text + tip icon
                ResetTipColors();
                tipIcon.sprite = Resources.Load<Sprite>("UI/TutorialIconSpaceship");
                tipText.text = "Now, explore! Return to the spaceship once you reach your credit target.";

                // Fades in the tips canvas
                yield return Utils.FadeContainer(tipCanvas, Utils.FadeType.IN, 1.0f);

                // Waits for 20 seconds before ending the tutorial
                yield return new WaitForSeconds(20.0f);

                // Fades out the tips canvas
                yield return Utils.FadeContainer(tipCanvas, Utils.FadeType.OUT, 1.0f);

                tutorialFinished = true;
            }

            yield return null;
        }
    }
    
    //---------------------------


}
