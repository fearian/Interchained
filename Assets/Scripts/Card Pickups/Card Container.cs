using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class CardContainer : MonoBehaviour
{
    private Canvas canvas;

    [Header("Zones")]
    [SerializeField] private Card selectedCard;
    [SerializeField] private HexGrid gameBoard;

    [Header("Slots")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private List<Transform> worldSlotSpacePositions;
    
    [Header("Cards")]
    public List<Card> Cards;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        CreateCardSlots();
    }

    private void CreateCardSlots()
    {
        Vector2 position = Vector2.zero;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        
        for (int i = 0; i < worldSlotSpacePositions.Count; i++)
        {
            GameObject cardSlot = Instantiate(slotPrefab, canvas.transform);
            
            position = canvas.worldCamera.WorldToViewportPoint(worldSlotSpacePositions[i].position);
            position = position * canvasRect.sizeDelta;
            RectTransform cardSlotRect = cardSlot.GetComponent<RectTransform>();
            cardSlotRect.position = position;
            cardSlot.transform.SetParent(transform, true);
        }
        
        Cards = GetComponentsInChildren<Card>().ToList();
        
        int cardCount = 0;
        foreach (var card in Cards)
        {
            card.PointerEnterEvent.AddListener(CardPointerEnter);
            card.PointerExitEvent.AddListener(CardPointerExit);
            card.BeginDragEvent.AddListener(BeginDrag);
            card.EndDragEvent.AddListener(EndDrag);
            card.name = cardCount.ToString();
            cardCount++;
        }
    }

    private void CardPointerEnter(Card card)
    {
        
    }

    private void CardPointerExit(Card arg0)
    {
        
    }

    private void BeginDrag(Card card)
    {
        selectedCard = card;
    }

    void EndDrag(Card card)
    {
        selectedCard = null;
    }
}
