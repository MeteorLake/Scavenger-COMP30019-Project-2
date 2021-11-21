using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    public GameObject player;

    public GameObject generalFader;

    public GameObject continueObject;
    public GameObject backToMainObject;
    public GameObject quitObject;

    //--------------------------

    PlayerController playerController;
    Camera playerCamera;

    CanvasGroup pauseCanvas;
    CanvasGroup generalFaderCanvas;

    Button continueButton;
    Button backToMainButton;
    Button quitButton;

    //--------------------------

    bool escReleased = true;
    bool pauseShown = false;
    bool menuTrigger = false;
    bool returnToMain = false;
    
    //--------------------------

    // Resumes the game by simulating a press of the escape button
    void ResumeGame() {
        menuTrigger = true;
    }

    // Goes back to the main menu
    void BackToMainMenu() {
        returnToMain = true;
    }

    // Quits the whole game
    void QuitGame() {
        Application.Quit();
    }

    //--------------------------

    // Start is called before the first frame update
    void Start()
    {
        // Gets the playercontroller from the player
        playerController = player.GetComponent<PlayerController>();

        // Gets the canvases
        pauseCanvas = transform.GetComponent<CanvasGroup>();
        generalFaderCanvas = generalFader.GetComponent<CanvasGroup>();

        // Starts the menu update coroutine
        StartCoroutine(UpdateMenu());
        
        //--------------------------
        
        // Gets the start/quit buttons
        continueButton = continueObject.GetComponent<Button>();
        backToMainButton = backToMainObject.GetComponent<Button>();
        quitButton = quitObject.GetComponent<Button>();

        // Connects up the listeners for each button
        continueButton.onClick.AddListener(ResumeGame);
        backToMainButton.onClick.AddListener(BackToMainMenu);
        quitButton.onClick.AddListener(QuitGame);
    }

    //--------------------------

    // Update is called once per frame
    IEnumerator UpdateMenu()
    {
        while (true) {
            if (playerController.playerLoaded && !playerController.GetDead()) {
                // If the pslayer presses the backtick key, turn no-clip on/off
                if (Keyboard.current[Key.Escape].IsPressed() || menuTrigger) {
                    if (escReleased || menuTrigger) {
                        // Sets the menuTrigger to false
                        menuTrigger = false;

                        // Sets that the backquote's being pressed
                        escReleased = false;

                        // Checks if the pause menu is shown
                        if (!pauseShown) {
                            // Shows the pause menu
                            pauseShown = true;
                            playerController.SetPaused(true);

                            // Sets the mouse mode and allows the player to interact with the menu
                            Cursor.lockState = CursorLockMode.None;
                            pauseCanvas.blocksRaycasts = true;
                            pauseCanvas.interactable = true;

                            // Fades in the container
                            yield return Utils.FadeContainer(pauseCanvas, Utils.FadeType.IN, 3.0f);

                        }
                        else {
                            // Hides the pause menu
                            pauseShown = false;
                            playerController.SetPaused(false);

                            // Sets the mouse mode and stops the player from interacting with the menu
                            Cursor.lockState = CursorLockMode.Locked;
                            pauseCanvas.blocksRaycasts = false;
                            pauseCanvas.interactable = false;

                            yield return Utils.FadeContainer(pauseCanvas, Utils.FadeType.OUT, 3.0f);
                        }
                    }
                } else {
                    escReleased = true;
                }

                // If the "return to menu" sequence is triggered, then start to return to the main menu
                if (returnToMain) {
                    // Fades in the container
                    yield return Utils.FadeContainer(generalFaderCanvas, Utils.FadeType.IN, 0.75f);

                    // Goes to the main menu
                    SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
                }
            }
            
            yield return null;
        }
    }
}
