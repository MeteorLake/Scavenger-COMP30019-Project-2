using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandFollow : MonoBehaviour
{
     
    public GameObject player;
    public float sandHeight = 2.0f;

    //-------------------------
    public float sandXOffset = 0.5f;
    public float sandYOffset = 0.5f;
    public float sandXDivisor = 5.0f;
    public float sandZDivisor = 5.0f;
    
    //-------------------------

    Material sandMat;
    
    //-------------------------

    void Start() {
        sandMat = gameObject.GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update() {
        // Sets the position of the sand to below the player
        transform.position = new Vector3(
            player.transform.position.x,
            sandHeight,
            player.transform.position.z
        );

        // Sets the sandMat tiling
        sandMat.SetTextureOffset("_BaseMap", new Vector2(
            -player.transform.position.x / sandXDivisor + sandXOffset,
            -player.transform.position.z / sandZDivisor + sandYOffset));
    }
}
