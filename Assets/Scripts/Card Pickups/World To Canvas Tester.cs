using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class WorldToCanvasTester : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform TargetRect;
    [SerializeField] private Transform targetTransform;
    private bool isRayCasting;
    
    [Header("Results")]
    public Vector2 ScreenPosition;
    public Vector3 WorldPosition;
    public Vector3 ViewportPosition;
    public Vector3 rectPosition;

    private void Update()
    {
        // Mouse Screen Position to World Position
        if (Input.GetMouseButton(0))
        {
            isRayCasting = true;

            ScreenPosition = (Vector2)Input.mousePosition;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit))
            {
                WorldPosition = raycastHit.point;
            }

        }
        // Use Object Transform for World Position
        else
        {
            isRayCasting = false;
            WorldPosition = targetTransform != null ? targetTransform.position : WorldPosition;
        }

        // World Position to Viewport and Rect Position
        ViewportPosition = mainCamera.WorldToViewportPoint(WorldPosition);
        rectPosition = (ViewportPosition - new Vector3(0.5f, 0.5f, 0f)) * canvas.GetComponent<RectTransform>().sizeDelta;

        // World Position to Screen Position
        if (isRayCasting == false)
        {
            ScreenPosition = (Vector2)Camera.main.WorldToScreenPoint(WorldPosition) / canvas.scaleFactor;
        }

        // Place target on rectPosition
        TargetRect.anchoredPosition = rectPosition;
    }
    
    private void OnDrawGizmos()
    {
        if (isRayCasting)
        {
            DebugExtension.DebugPoint(WorldPosition, new Color(0.85f, 0.85f, 0.5f), 0.45f);
        }
    }
}
