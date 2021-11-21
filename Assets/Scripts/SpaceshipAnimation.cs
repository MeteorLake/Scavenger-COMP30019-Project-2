using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipAnimation : MonoBehaviour
{

    public float spaceshipMovementSpeed = 0.5f;
    public float spaceshipMovementMagnitude = 1.0f;

    public float spaceshipRotSpeed = 0.25f;
    public float spaceshipRotMagnitude = 20.0f;

    //---------------------------

    Vector3 startingPos;
    Quaternion startingRot;

    float time = 0;
    
    //---------------------------

    // Start is called before the first frame update
    void Start()
    {
        startingPos = transform.localPosition;
        startingRot = transform.localRotation;
    }

    // Update is called once per frame

    void Update() {
        // Increments the time
        time += Time.deltaTime;

        transform.localPosition = startingPos + new Vector3(
            0.0f,
            (Mathf.PerlinNoise(
                time * spaceshipMovementSpeed,
                0.0f
            ) - 0.5f) * spaceshipMovementMagnitude,
            0.0f
        );

        transform.localRotation = startingRot * Quaternion.Euler(
            (Mathf.PerlinNoise(
                time * spaceshipRotSpeed,
                time * spaceshipRotSpeed / 2.0f
            ) - 0.5f) * spaceshipRotMagnitude,
            0.0f,
            (Mathf.PerlinNoise(
                -time * spaceshipRotSpeed,
                time * spaceshipRotSpeed
            ) - 0.5f) * spaceshipRotMagnitude
        );
    }
}
