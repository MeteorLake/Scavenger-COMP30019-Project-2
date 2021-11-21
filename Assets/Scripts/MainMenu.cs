using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Main Menu Settings")]
    public GameObject startObject;
    public GameObject quitObject;
    public GameObject fader;
    
    [Header("Difficulty Menu Settings")]
    public GameObject difficultyObject;
    public GameObject backToMainObject;

    public GameObject casualObject;
    public GameObject standardObject;
    public GameObject hardcoreObject;

    //---------------------------
    
    Button startButton;
    Button quitButton;
    Button backToMainButton;

    Button casualButton;
    Button standardButton;
    Button hardcoreButton;

    CanvasGroup faderCanvas;
    CanvasGroup mainCanvas;
    CanvasGroup difficultyCanvas;

    //---------------------------

    // Used to track the menu's state, can be "MAIN" or "DIFFICULTY"
    string menuState = "MAIN";

    // Used to track the previous state of the menu, will be used for comparisons
    string previousState = "MAIN";

    // Tracks the currently active CanvasGroup
    CanvasGroup activeCanvas;
    bool gameStarting = false;

    //---------------------------

    IEnumerator GameStart(string difficultyLevel) {
        // Activates the black fader
        yield return Utils.FadeContainer(faderCanvas, Utils.FadeType.IN, 1.25f);

        // Sets the difficulty level
        PlayerPrefs.SetString("Difficulty", difficultyLevel);
        
        // Loads in the main game
        SceneManager.LoadScene("MainGame", LoadSceneMode.Single);
    }

    void QuitGame() {
        Application.Quit();
    }

    void ShowDifficulty() {
        menuState = "DIFFICULTY";
    }

    void BackToMain() {
        menuState = "MAIN";
    }

    void StartGame(string difficultyLevel) {
        if (!gameStarting) {
            gameStarting = true;
            StartCoroutine(GameStart(difficultyLevel));
        }
    }
    
    //---------------------------

    // Start is called before the first frame update
    void Start()
    {
        // Gets the start/quit buttons
        startButton = startObject.GetComponent<Button>();
        quitButton = quitObject.GetComponent<Button>();
        backToMainButton = backToMainObject.GetComponent<Button>();

        // Gets the difficulty buttons
        casualButton = casualObject.GetComponent<Button>();
        standardButton = standardObject.GetComponent<Button>();
        hardcoreButton = hardcoreObject.GetComponent<Button>();

        // Connects up the listeners for each button
        startButton.onClick.AddListener(ShowDifficulty);
        quitButton.onClick.AddListener(QuitGame);
        backToMainButton.onClick.AddListener(BackToMain);
        
        casualButton.onClick.AddListener(() => StartGame("CASUAL"));
        standardButton.onClick.AddListener(() => StartGame("STANDARD"));
        hardcoreButton.onClick.AddListener(() => StartGame("HARDCORE"));
        
        // Gets the canvas groups
        faderCanvas = fader.GetComponent<CanvasGroup>();
        mainCanvas = transform.GetComponent<CanvasGroup>();
        difficultyCanvas = difficultyObject.GetComponent<CanvasGroup>();

        // Sets the active canvas
        activeCanvas = mainCanvas;

        StartCoroutine(Utils.FadeContainer(faderCanvas, Utils.FadeType.OUT, 1.5f));
        StartCoroutine(MenuLoop());
    }

    //---------------------------

    IEnumerator MenuLoop() {
        while (true) {
            // If the previous state and menu state aren't equal, we need to change that
            if (previousState != menuState) {
                // Activates the black fader
                yield return Utils.FadeContainer(faderCanvas, Utils.FadeType.IN, 2.0f);

                // Set the active canvas alpha to 0
                activeCanvas.alpha = 0;
                activeCanvas.interactable = false;
                activeCanvas.blocksRaycasts = false;

                // If the state is the main menu, set that to visible + to the active canvas
                if (menuState == "MAIN") {
                    mainCanvas.alpha = 1;
                    mainCanvas.interactable = true;
                    mainCanvas.blocksRaycasts = true;

                    activeCanvas = mainCanvas;
                }
                else if (menuState == "DIFFICULTY") {
                    difficultyCanvas.alpha = 1;
                    difficultyCanvas.interactable = true;
                    difficultyCanvas.blocksRaycasts = true;

                    activeCanvas = difficultyCanvas;
                }

                // Deactivates the black fader
                yield return Utils.FadeContainer(faderCanvas, Utils.FadeType.OUT, 2.0f);
            }

            // Sets the current state of the menu
            previousState = menuState;

            yield return null;
        }
    }
}
