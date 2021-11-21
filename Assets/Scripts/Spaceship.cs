using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spaceship : ActionItem {
    public GameObject player;
    
    //---------------------------

    PlayerController playerController;

    //---------------------------
    
    void Start() {
        // Sets the playerController
        playerController = player.GetComponent<PlayerController>();
    }

    //---------------------------

    public override string GetString() {
        if (playerController.GetCredits() < playerController.creditTarget)
            return "You need "
                + (playerController.creditTarget - playerController.GetCredits()).ToString()
                + " more credits to head back home";
        else
            return "You have enough credits to head home!";
    }

    public override string ActivateObject(PlayerController controller) {
        // If the player has enough credits, they're victorious!
        if (playerController.GetCredits() >= playerController.creditTarget) {
            playerController.SetVictory(true);
            playerController.SetOxygenDepleting(false);

            return "SPACESHIP_INTERACT_SUCCESS";
        }

        return "SPACESHIP_INTERACT_FAIL";
    }
}
