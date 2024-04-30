using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour,
    IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    private Canvas canvas;
    private Image imageComponent;
    private RectTransform rect;
    private Vector2 offset;
    float timeCount;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeedLimit = 50;
    private Vector2 direction;
    private Vector2 velocity;

    [Header("States")]
    [SerializeField] private bool isDragging;
    
    [Header("Events")]
    [HideInInspector] public UnityEvent<Card> PointerEnterEvent;
    [HideInInspector] public UnityEvent<Card> PointerExitEvent;
    [HideInInspector] public UnityEvent<Card> PointerUpEvent;
    [HideInInspector] public UnityEvent<Card> PointerDownEvent;
    [HideInInspector] public UnityEvent<Card> BeginDragEvent;
    [HideInInspector] public UnityEvent<Card> EndDragEvent;
    [HideInInspector] public UnityEvent<Card, bool> SelectEvent;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        imageComponent = GetComponent<Image>();
        rect = imageComponent.rectTransform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginDragEvent.Invoke(this);
        isDragging = true;
        offset = Vector2.zero;
        canvas.GetComponent<GraphicRaycaster>().enabled = false;
        imageComponent.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        offset = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndDragEvent.Invoke(this);
        isDragging = false;
        offset = Vector2.zero;
        canvas.GetComponent<GraphicRaycaster>().enabled = true;
        imageComponent.raycastTarget = true;
    }

    void Update()
    {
        //ClampPosition();
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"");
        }
        
        if (isDragging)
        {
            direction = (offset - rect.anchoredPosition).normalized;
            velocity = direction * Mathf.Min(moveSpeedLimit, Vector2.Distance(offset,rect.anchoredPosition) / Time.deltaTime);
            transform.Translate(velocity * Time.deltaTime);
            
            timeCount += Time.deltaTime;
            if (timeCount > 0.5f)
            {
                Debug.Log($"[{offset}] - [{rect.anchoredPosition}]");
                timeCount = 0.0f;
            }
        }
    }

    void ClampPosition()
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x, screenBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y, screenBounds.y);
        transform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnterEvent.Invoke(this);    
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExitEvent.Invoke(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PointerUpEvent.Invoke(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PointerDownEvent.Invoke(this);
    }

    private void OnDrawGizmos()
    {
        if (isDragging)
        {
            DebugExtension.DebugPoint(canvas.worldCamera.ScreenToWorldPoint(offset), new Color(0.85f, 0.85f, 0.5f), 3f);
            DebugExtension.DebugPoint(rect.transform.position, new Color(0.15f, 0.85f, 0.51f), 2f);
            DebugExtension.DebugArrow(rect.transform.position, direction,  new Color(0.85f, 0.1f, 0.51f));
        }
    }
}
