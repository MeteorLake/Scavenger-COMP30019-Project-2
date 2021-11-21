using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterAnimator : MonoBehaviour
{
    
    public GameObject player;
    public float waterHeight = 0.0f;

    Material waterMat;

    float time = 0;
    
    //-------------------------

    // Start is called before the first frame update
    void Start()
    {
        // Gets the water material
        waterMat = gameObject.GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        // Sets the time of the water
        waterMat.SetFloat("Time", time);
        
        time += Time.deltaTime;

        float lockConstant = 0.001953f * 300.0f;

        // Sets the position of the water to below the player
        transform.position = new Vector3(
            Mathf.Round(player.transform.position.x / lockConstant) * lockConstant,
            waterHeight,
            Mathf.Round(player.transform.position.z / lockConstant) * lockConstant
        );
    }

    //-------------------------

    public Material GetWaterMat() {
        return waterMat;
    }
}