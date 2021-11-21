using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LoadingScreen : MonoBehaviour
{

    public GameObject terrain;
    public GameObject loadText;
    public GameObject loadFlavour;

    public GameObject loadingClockwise;
    public GameObject loadingCounterClockwise;

    public string[] loadingText;

    //----------------------------

    ProceduralTerrain terrainScript;
    
    //----------------------------

    float textFade = 0.0f;
    float loadFade = 1.0f;

    float textFadeSpeed = 1.0f;
    float loadFadeSpeed = 0.5f;

    float rotateSpeedClockwise = 20f;
    float rotateSpeedCounterClockwise = -50f;
    
    //----------------------------

    CanvasGroup loadScreenCanvasGroup;
    CanvasGroup loadTextCanvasGroup;
    
    RectTransform loadingClockwiseTransform;
    RectTransform loadingCounterClockwiseTransform;

    //----------------------------

    // Start is called before the first frame update
    void Start()
    {
        terrainScript = terrain.GetComponent<ProceduralTerrain>();
        
        loadScreenCanvasGroup = gameObject.GetComponent<CanvasGroup>();
        loadTextCanvasGroup = loadText.GetComponent<CanvasGroup>();
        
        loadingClockwiseTransform = loadingClockwise.GetComponent<RectTransform>();
        loadingCounterClockwiseTransform = loadingCounterClockwise.GetComponent<RectTransform>();

        Text loadFlavourText = loadFlavour.GetComponent<Text>();
        loadFlavourText.text = loadingText[Random.Range(0, loadingText.Length)];
    }

    // Update is called once per frame
    void Update()
    {
        // If the terrain is loaded, fade out. Else, fade in.
        if (terrainScript.GetTerrainLoaded() && loadFade > 0.0f) {
            loadFade = loadFade - loadFadeSpeed * Time.deltaTime;
        }
        else if (!terrainScript.GetTerrainLoaded() && textFade < 1.0f) {
            textFade = textFade + textFadeSpeed * Time.deltaTime;
        }

        // From here, update the alpha of the loadscreen + loadscreen text
        loadScreenCanvasGroup.alpha = Utils.easeInOutQuint(loadFade);
        loadTextCanvasGroup.alpha = Utils.easeInOutQuint(textFade);

        // Gets the rectTransforms of the loaders, transforms them
        loadingClockwiseTransform.rotation *= Quaternion.Euler(0.0f, 0.0f, rotateSpeedClockwise * Time.deltaTime);
        loadingCounterClockwiseTransform.rotation *= Quaternion.Euler(0.0f, 0.0f, rotateSpeedCounterClockwise * Time.deltaTime);
    }
}
