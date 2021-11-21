
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameOverScreen : MonoBehaviour
{
    public GameObject player;
    public GameObject retryButton;

    public float fadeRate = 1.0f;

    //--------------------

    PlayerController playerController;
    CanvasGroup gameOverScreen;
    Button retryButtonImage;

    float deathAlpha;
    
    //--------------------
    // Start is called before the first frame update
    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        gameOverScreen = transform.GetComponent<CanvasGroup>();
        retryButtonImage = retryButton.GetComponent<Button>();

        retryButtonImage.onClick.AddListener(GoToMain);
    }

    // Update is called once per frame
    void Update()
    {
        // If the player is dead and the deathAlpha is less than one, start fading it in
        if (playerController.GetDead() && deathAlpha < 1.0f) {
            // Update the deathAlpha
            deathAlpha += fadeRate * Time.deltaTime;

            // Gets an interpolated version of the deathAlpha
            float deathAlphaEased = Utils.easeInOutQuint(deathAlpha);

            // Sets the alpha + other settings
            gameOverScreen.alpha = deathAlphaEased;
            gameOverScreen.interactable = true;
            gameOverScreen.blocksRaycasts = true;
            
            // Unlocks the cursor
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void GoToMain() {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
