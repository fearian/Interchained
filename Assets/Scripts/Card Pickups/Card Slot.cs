using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CardSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private bool willSwapCurrentCardOnDrop = false;
    private Card currentCard;
    private bool isOccupied => currentCard != null;

    public UnityEvent<CardSlot> DroppedEvent;

    private void Awake()
    {
        Card childCard = this.GetComponentInChildren<Card>();
        if (childCard != null) currentCard = childCard;
    }

    public void OnDrop(PointerEventData eventData)
    {
        DroppedEvent.Invoke(this);

        Card prevCard = null;

        if (isOccupied)
        {
            if (willSwapCurrentCardOnDrop == false) return;
            else prevCard = currentCard;
        }

        GameObject dropped = eventData.pointerDrag;
        if (dropped.TryGetComponent(out currentCard) == false) return;

        currentCard.sendHomeAfterDrag = false;
        currentCard.SetParentAfterDrag(transform);
        currentCard.BeginDragEvent.AddListener(BeginDrag);

        if (prevCard != null)
        {
            prevCard.BeginDragEvent.RemoveListener(BeginDrag);
            prevCard.sendHomeAfterDrag = true;
            prevCard.ReturnToZero();
        }
    }

    private void BeginDrag(Card card)
    {
        currentCard.BeginDragEvent.RemoveListener(BeginDrag);
        currentCard = null;
    }
}