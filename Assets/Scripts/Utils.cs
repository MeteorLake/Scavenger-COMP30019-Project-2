using System.Collections;
using System.Collections.Generic;

using UnityEngine;

class Utils {
    public enum FadeType { IN, OUT };

    // Fade counter that tracks what "fade" this is. Starts at zero, goes up
    // as more fade-ins happen
    static int fadeInNumber = 0;

    // Fades in/out a given container.
    public static IEnumerator FadeContainer(CanvasGroup canvas, FadeType fadeType, float fadeRate) {
        // Increments the fadeInNumber
        fadeInNumber++;

        // Sets the calledFadeNumber to fadeInNumber
        int calledFadeNumber = fadeInNumber;
        
        if (fadeType == FadeType.IN) {
            while (canvas.alpha < 1.0f && calledFadeNumber == fadeInNumber) {
                canvas.alpha += fadeRate * Time.deltaTime;
                Debug.Log("CANVAS ALPHA: " + canvas.alpha);
                yield return null;
            }
        }
        else if (fadeType == FadeType.OUT) {
            while (canvas.alpha > 0.0f && calledFadeNumber == fadeInNumber) {
                canvas.alpha -= fadeRate * Time.deltaTime;
                Debug.Log("CANVAS ALPHA: " + canvas.alpha);
                yield return null;
            }
        }
    }

    //------------------------------

    public static float easeInOutQuint(float x) {
        return x < 0.5 ? 16 * x * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 5) / 2;
    }
}