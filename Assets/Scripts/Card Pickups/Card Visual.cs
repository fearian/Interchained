using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardVisual : MonoBehaviour
{
    private bool initalize = false;

    [Header("Card")]
    public Card ThisCard;

    private Canvas canvas;
    private Vector3 positionDelta;
    private Vector3 rotationDelta;

    [Header("References")]
    [SerializeField] private Transform shakeParent;
    [SerializeField] private Transform tiltParent;
    [SerializeField] private Image cardImage;
    
    [Header("Rotation Parameters")]
    [SerializeField] private float rotationAmount = 20;
    [SerializeField] private float rotationSpeed = 20;
    [SerializeField] private float autoTiltAmount = 30;
    [SerializeField] private float manualTiltAmount = 20;
    [SerializeField] private float tiltSpeed = 20;

    public void Initalize(Card target)
    {
        ThisCard = target;
        canvas = GetComponentInParent<Canvas>();
        
        ThisCard.PointerEnterEvent.AddListener(PointerEnter);
        ThisCard.PointerExitEvent.AddListener(PointerExit);
        ThisCard.BeginDragEvent.AddListener(BeginDrag);
        ThisCard.EndDragEvent.AddListener(EndDrag);
        ThisCard.PointerDownEvent.AddListener(PointerDown);
        ThisCard.PointerUpEvent.AddListener(PointerUp);
        ThisCard.SelectEvent.AddListener(Select);

        initalize = true;
    }

    void Update()
    {
        if (!initalize || ThisCard == null) return;
        
        FollowRotation();
        CardTilt();
    }

    private void FollowRotation()
    {
    }

    private void CardTilt()
    {
    }
    
    
    private void PointerEnter(Card arg0)
    {
    }

    private void PointerExit(Card arg0)
    {
    }

    private void BeginDrag(Card arg0)
    {
    }

    private void EndDrag(Card arg0)
    {
    }

    private void PointerDown(Card arg0)
    {
        
    }

    private void PointerUp(Card arg0)
    {
    }

    private void Select(Card arg0, bool arg1)
    {
    }
}
