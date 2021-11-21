using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OxygenStation : ActionItem {

    float refillTime = 5.0f;

    float currentTime = 0.0f;
    
    public AudioClip refillSound;

    public string oxygenStationLocation = "";

    //-------------------------

    // Update is called once per frame
    void Update()
    {
        currentTime = Mathf.Clamp(currentTime - Time.deltaTime, 0.0f, refillTime);
    }

    //---------------------------

    public override string GetString() {
        if (currentTime > 0.0f)
            return (100 - (Mathf.Round(currentTime * 10))).ToString() + "% ready for reuse";
        else
            return "Left-click to refill your oxygen";
    }

    public override string ActivateObject(PlayerController controller) {
        if (currentTime == 0.0f) {
            // Increases the player's credits
            controller.addToOxygen(1.0f);

            // Plays a sound on the player's end for the pickup
            controller.playerAudio.PlayOneShot(refillSound, 0.2f);

            // Sets the current time to the refill time
            currentTime = refillTime;
            
            if (oxygenStationLocation == "")
                return "USE_OXYGEN_STATION";
            else
                return "USE_OXYGEN_STATION_" + oxygenStationLocation;
        }
        
        return "OXYGEN_STATION_FAIL";
    }
}
