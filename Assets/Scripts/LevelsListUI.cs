using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelsListUI : MonoBehaviour
{
    [SerializeField] private Levels _levelsList;
    [SerializeField] private Interchained _interchained;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Transform contentField;
    [SerializeField] private ScrollRect _scrollRect;
    private List<Button> buttons;

    private void Awake()
    {
        buttons = new List<Button>();
        UpdateList();
        
        _interchained.onLevelSaved.AddListener(UpdateList);
    }

    private void UpdateList()
    {
        foreach (var button in buttons)
        {
            button.gameObject.SetActive(false);
        }
        foreach (var level in _levelsList.savedLevels)
        {
            string levelName = level.Name;
            if (levelName == "") levelName = "Blank Board";
            Button button = Instantiate(buttonPrefab, contentField);
            button.GetComponentInChildren<TextMeshProUGUI>().text = levelName;
            button.onClick.AddListener(() => _interchained.LoadBoardState(levelName));
            buttons.Add(button);
            
            button.gameObject.SetActive(true);
        }
        
        _scrollRect.Rebuild(CanvasUpdate.Prelayout);
    }
}
