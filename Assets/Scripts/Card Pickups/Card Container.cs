using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardContainer : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Card selectedCard;
    private Canvas canvas;
    private RectTransform canvasRect;

    [Header("Slots")]
    [SerializeField] private List<Transform> worldSlotSpacePositions;
    
    [Header("Cards")]
    public List<Card> Cards;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
        
        for (int i = 0; i < worldSlotSpacePositions.Count; i++)
        {
            var cardSlot = Instantiate(slotPrefab, canvas.transform);
            
            Vector2 position = canvas.worldCamera.WorldToViewportPoint(worldSlotSpacePositions[i].position);
            position = position * canvasRect.sizeDelta;
            var cardSlotRect = cardSlot.GetComponent<RectTransform>();
            cardSlotRect.position = position;
            cardSlot.transform.SetParent(transform, false);
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
        if (selectedCard == null)
            return;

        selectedCard.Rect.anchoredPosition = Vector2.zero;
        
        // selectedCard.transform.DOLocalMove(selectedCard.selected ? new Vector3(0,selectedCard.selectionOffset,0) : Vector3.zero, tweenCardReturn ? .15f : 0).SetEase(Ease.OutBack);

        // rect.sizeDelta += Vector2.right;
        // rect.sizeDelta -= Vector2.right;

        selectedCard = null;

    }
}