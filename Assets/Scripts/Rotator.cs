using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float rotationSpeed = 100.0f; // Adjust the rotation speed in the Inspector.

    void Update()
    {
        // Rotate the object around the Y-axis at a constant rate.
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
