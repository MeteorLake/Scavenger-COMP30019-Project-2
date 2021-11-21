using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HologramAnimator : MonoBehaviour
{
    public float ROTATE_SPEED = 75.0f;
    const float GLITCH_CHANCE = 0.05f;
    Material holoMat;

    float time = 0;

    //-------------------------

    // Start is called before the first frame update
    void Start()
    {
        // Gets the water material
        holoMat = gameObject.GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        // Sets the time of the water
        holoMat.SetFloat("Time", time);

        time += Time.deltaTime;

        // Generates a random number, activates the glitch if under GLITCH_CHANCE
        float glitch = Random.Range(0.0f, 1.0f);
        if (glitch < GLITCH_CHANCE) {
            // Sets IsGlitching to a random value
            holoMat.SetFloat("IsGlitching", Random.Range(0.5f, 1.0f));
        } else {
            // Stops glitching
            holoMat.SetFloat("IsGlitching", 0.0f);
        }

        // Rotate the hologram
        transform.rotation *= Quaternion.Euler(0.0f, 0.0f, ROTATE_SPEED * Time.deltaTime);
    }
}