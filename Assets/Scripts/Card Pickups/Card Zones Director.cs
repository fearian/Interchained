using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardZonesDirector : MonoBehaviour
{
    [SerializeField] private RectTransform selectionZone;
    [SerializeField] private RectTransform cardContainer;

    public Card selectedCard;

    public bool IsInSelectionZone(Card card)
    {
        Debug.Log($"Card: {card.Rect.rect}, Zone:{selectionZone.rect}");
        return card.Rect.rect.Overlaps(selectionZone.rect);
    }
}
