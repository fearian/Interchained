using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class CardContainer : MonoBehaviour
{
    //[SerializeField] private CardZonesDirector director;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Card selectedCard;
    private Canvas canvas;
    private RectTransform canvasRect;
    private RectTransform containerRect;

    [Header("Zones")]
    [SerializeField] private float distanceToLeaveContainer = 100f;
    [SerializeField] private GameObject selectionPosition;
    [SerializeField] private CardSlot selectionSlot;
    [SerializeField] private HexGrid gameBoard;

    [Header("Slots")]
    [SerializeField] private List<Transform> worldSlotSpacePositions;
    
    [Header("Cards")]
    public List<Card> Cards;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
        containerRect = this.GetComponent<RectTransform>();

        Vector2 position = Vector2.zero;
        
        for (int i = 0; i < worldSlotSpacePositions.Count; i++)
        {
            GameObject cardSlot = Instantiate(slotPrefab, canvas.transform);
            
            position = canvas.worldCamera.WorldToViewportPoint(worldSlotSpacePositions[i].position);
            position = position * canvasRect.sizeDelta;
            RectTransform cardSlotRect = cardSlot.GetComponent<RectTransform>();
            cardSlotRect.position = position;
            cardSlot.transform.SetParent(transform, true);
        }
        
        position = canvas.worldCamera.WorldToViewportPoint(selectionPosition.transform.position);
        position = position * canvasRect.sizeDelta;
        selectionSlot.rect.position = position;


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

    private void Update()
    {
        
    }

    void EndDrag(Card card)
    {
        if (selectedCard == null) return;

        var currentDistance = Vector2.Distance(selectedCard.transform.position, Vector2.zero);
        if (currentDistance >= distanceToLeaveContainer) SendToSelectedSlot(selectedCard);
        else selectedCard.Rect.DOLocalMove(Vector3.zero, 0.15f).SetEase(Ease.OutBack);

        selectedCard = null;
    }

    private void SendToSelectedSlot(Card card)
    {
        
    }
}
