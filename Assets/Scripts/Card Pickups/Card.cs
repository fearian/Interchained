using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Card : MonoBehaviour,
    IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    private Canvas canvas;
    private Image imageComponent;
    public RectTransform Rect;
    float timeCount;

    //[Header("Dragging V2")]
    [HideInInspector] public Transform parentAfterDrag { get; private set; }
    [HideInInspector] public Transform homeParent { get; private set; }
    [HideInInspector] public bool sendHomeAfterDrag = true;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeedLimit = 50;
    private Vector2 offset;
    private Vector2 targetPosition;
    private Vector2 direction;
    private Vector2 velocity;
    public CardSlot homeSlot;
    public CardSlot currentSlot;

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

    void Awake()
    {
        homeParent = transform.parent;
        canvas = GetComponentInParent<Canvas>();
        imageComponent = GetComponent<Image>();
        Rect = imageComponent.rectTransform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginDragEvent.Invoke(this);
        isDragging = true;
        sendHomeAfterDrag = true;
        parentAfterDrag = transform.parent;
        transform.SetParent(canvas.transform, true);
        transform.SetAsFirstSibling();
        offset = (Vector2)Input.mousePosition - Rect.anchoredPosition;
        
        //canvas.GetComponent<GraphicRaycaster>().enabled = false;
        imageComponent.raycastTarget = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndDragEvent.Invoke(this);
        isDragging = false;
        offset = Vector2.zero;
        imageComponent.raycastTarget = true;
        
        ReturnToZero();
    }

    public void ReturnToZero()
    {
        if (sendHomeAfterDrag)
        {
            transform.SetParent(homeParent);
        }
        else transform.SetParent(parentAfterDrag);

        Rect.DOLocalMove(Vector3.zero, 0.15f).SetEase(Ease.OutBack);
    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }

    void Update()
    {
        //ClampPosition();
        
        if (isDragging)
        {
            targetPosition = (Vector2)Input.mousePosition - offset;
            direction = (targetPosition - Rect.anchoredPosition).normalized;
            velocity = direction * Mathf.Min(moveSpeedLimit, Vector2.Distance(targetPosition,Rect.anchoredPosition) / Time.deltaTime);
            Rect.anchoredPosition += (velocity * Time.deltaTime);
            //Logging();
        }

        void Logging()
        {
            timeCount += Time.deltaTime;
            if (timeCount > 0.5f)
            {
                Debug.Log($"Mouse Position:[{targetPosition}]. Velocity: [{velocity}]");
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

    public void SetParentAfterDrag(Transform transform)
    {
        parentAfterDrag = transform;
    }

    private void OnDrawGizmos()
    {
        if (isDragging)
        {
            DebugExtension.DebugPoint(canvas.worldCamera.ScreenToWorldPoint(offset), new Color(0.85f, 0.85f, 0.5f), 1f);
            DebugExtension.DebugPoint(Rect.transform.position, new Color(0.15f, 0.85f, 0.51f), .2f);
            DebugExtension.DebugArrow(Rect.transform.position, direction * 0.3f,  new Color(0.85f, 0.1f, 0.51f));
        }
    }
}
