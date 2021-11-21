using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class VictoryScreen : MonoBehaviour
{

    public GameObject details;
    public GameObject fader;
    public GameObject mainUI;
    public GameObject tutorialUI;
    public GameObject extraCredits;
    public GameObject backObject;

    //------------------------

    public GameObject player;
    public GameObject terrainObject;
    public GameObject waterObject;

    //------------------------
        
    CanvasGroup detailsCanvasGroup;
    CanvasGroup faderCanvasGroup;
    CanvasGroup mainUICanvasGroup;
    CanvasGroup tutorialCanvasGroup;

    Button backButton;

    Text extraCreditsText;

    PlayerController playerController;
    ProceduralTerrain proceduralGeneration;
    WaterAnimator water;

    GameObject victoryCamera;

    //------------------------

    float fade = 0.0f;
    float fadeRate = 0.5f;

    float cameraRotationRate = 10.0f;

    // Checks to see if the victory sequence has started
    bool victorySequenceStarted;

    //------------------------

    // Start is called before the first frame update
    void Start()
    {
        // Gets the details and fader canvas groups
        detailsCanvasGroup = details.GetComponent<CanvasGroup>();
        faderCanvasGroup = fader.GetComponent<CanvasGroup>();
        mainUICanvasGroup = mainUI.GetComponent<CanvasGroup>();
        tutorialCanvasGroup = mainUI.GetComponent<CanvasGroup>();

        // Gets the extra credits text
        extraCreditsText = extraCredits.GetComponent<Text>();

        // Gets the player controllerl, procedural generation and water
        playerController = player.GetComponent<PlayerController>();
        proceduralGeneration = terrainObject.GetComponent<ProceduralTerrain>();
        water = waterObject.GetComponent<WaterAnimator>();

        // Sets the back button, connects it to a function
        backButton = backObject.GetComponent<Button>();
        backButton.onClick.AddListener(GoToMain);
    }

    // Update is called once per frame
    void Update()
    {
        // If the player is now victorious and the victory sequence
        // hasn't started, run it.
        if (!victorySequenceStarted && playerController.GetVictory()) {
            victorySequenceStarted = true;
            StartCoroutine(RunVictory());
        }

        // If the camera is set, rotate it
        if (victoryCamera != null) {
            victoryCamera.transform.rotation *= Quaternion.Euler(0.0f, 0.0f, cameraRotationRate * Time.deltaTime);
        }

        // Sets the fader alpha based on the fade variable
        faderCanvasGroup.alpha = Utils.easeInOutQuint(fade);
    }
    
    //------------------------

    public void GoToMain() {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    //------------------------

    IEnumerator fadeIn() {
        while (fade > 0.0f) {
            fade -= fadeRate * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator fadeOut() {
        while (fade < 1.0f) {
            fade += fadeRate * Time.deltaTime;
            yield return null;
        }
    }

    //------------------------

    // Runs the victory sequence
    IEnumerator RunVictory() {
        // Waits for the initial fade
        yield return StartCoroutine(fadeOut());
            
        //------------------------

        // Set the alpha of the details canvas
        detailsCanvasGroup.alpha = 1.0f;
        mainUICanvasGroup.alpha = 0.0f;
        tutorialCanvasGroup.alpha = 0.0f;

        // Makes the canvas group detailed
        detailsCanvasGroup.interactable = true;
        detailsCanvasGroup.blocksRaycasts = true;

        // Creates the victory camera
        victoryCamera = new GameObject("VictoryCamera");

        // Sets the victory camera transform to be whatever the
        // terrain center is
        victoryCamera.transform.position = proceduralGeneration.GetCenter()
            + new Vector3(0.0f, 10.0f, 0.0f);
        victoryCamera.transform.rotation *= Quaternion.Euler(-90.0f, 0.0f, 0.0f);

        // Sets the water animator so that its position is correct
        water.player = victoryCamera;

        // Sets the extra credits text
        extraCreditsText.text = (playerController.GetCredits() - playerController.creditTarget).ToString();

        // Adds the camera component to the victory camera
        Camera camera = victoryCamera.AddComponent<Camera>();

        // Enables the cam + unlocks the cursor
        camera.enabled = true;
        camera.farClipPlane = 10000.0f;

        // Gets the UAC data and sets post processing to true.
        UniversalAdditionalCameraData uac = victoryCamera.AddComponent<UniversalAdditionalCameraData>();
        uac.renderPostProcessing = true;

        Cursor.lockState = CursorLockMode.None;
            
        //------------------------
        
        // Fades out once the initial fade is done
        yield return StartCoroutine(fadeIn());
    }
}
