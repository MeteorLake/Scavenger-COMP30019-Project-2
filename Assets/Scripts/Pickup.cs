using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class ActionItem : MonoBehaviour {
    public string itemName = "ActionItem";

    public abstract string ActivateObject(PlayerController controller);
    public abstract string GetString();
}

//-------------------

public class Pickup : ActionItem {
    public uint pickupValue = 10;
    public AudioClip pickupSound;

    //---------------------------

    public override string GetString() {
        return "Left-click for " + pickupValue.ToString() + " credits";
    }

    public override string ActivateObject(PlayerController controller) {
        // Increases the player's credits
        controller.AddToCredits(pickupValue);

        // Plays a sound on the player's end for the pickup
        controller.playerAudio.PlayOneShot(pickupSound, 0.3f);

        // Destroys the object.
        Destroy(gameObject);

        return "PICKUP_TREASURE";
    }
}
