using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSlot : MonoBehaviour
{
    public RectTransform rect;
    public bool isOccupied { get; private set; }
    public Card currentCard { get; private set; }

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        currentCard = GetComponentInChildren<Card>();
        if (currentCard != null) isOccupied = true;
    }

    public bool SetCard(Card card)
    {
        if (isOccupied) return false;

        else
        {
            currentCard = card;
            isOccupied = true;

            return true;
        }
        
    }

    public Card RemoveCard()
    {
        if (!isOccupied) return null;

        else
        {
            var removedCard = currentCard;
            isOccupied = false;
            currentCard = null;
            return removedCard;
        }
    }
    
}
