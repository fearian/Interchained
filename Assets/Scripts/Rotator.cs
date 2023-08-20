using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float rotationSpeed = 100.0f; // Adjust the rotation speed in the Inspector.
    private bool _rotate = true;

    void Update()
    {
        if (!_rotate) return;
        // Rotate the object around the Y-axis at a constant rate.
        transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));
    }

    public bool IsRotating(bool rotate)
    {
        _rotate = rotate;
        return _rotate;
    }
    public bool IsRotating()
    {
        return _rotate;
    }
}
