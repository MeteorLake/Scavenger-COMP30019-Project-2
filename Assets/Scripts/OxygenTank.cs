using UnityEngine;

public class OxygenTank : ActionItem {
    public float oxygenValue = 0.5f;
    public AudioClip pickupSound;

    //---------------------------

    public override string GetString() {
        return "Left-click to fill oxygen by " + (Mathf.Round(oxygenValue * 100.0f)).ToString() + "%";
    }

    public override string ActivateObject(PlayerController controller) {
        // Increases the player's credits
        controller.addToOxygen(oxygenValue);

        // Plays a sound on the player's end for the pickup
        controller.playerAudio.PlayOneShot(pickupSound, 0.2f);

        // Destroys the object.
        Destroy(gameObject);

        return "PICKUP_OXYGEN";
    }
}
