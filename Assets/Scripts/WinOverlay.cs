using UnityEngine;
using UnityEngine.UI;

public class WinOverlay : MonoBehaviour
{
    [SerializeField] private BoardColors boardColors;
    [SerializeField] private Interchained game;

    private GameObject overlayRoot;

    private void Start()
    {
        if (game == null) game = FindObjectOfType<Interchained>();
        if (boardColors == null && game != null) boardColors = game.boardColorPalette;

        BuildOverlay();
        SetVisible(false);

        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(object sender, System.EventArgs e)
    {
        SetVisible(GameManager.Instance != null && GameManager.Instance.IsGameOver());
    }

    private void SetVisible(bool visible)
    {
        if (overlayRoot != null) overlayRoot.SetActive(visible);
    }

    private void BuildOverlay()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            gameObject.AddComponent<GraphicRaycaster>();
        }

        Color accent = boardColors != null ? boardColors.loopComplete : Color.green;

        overlayRoot = new GameObject("WinOverlayPanel");
        overlayRoot.transform.SetParent(transform, false);
        Image backdrop = overlayRoot.AddComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.85f);
        Stretch(overlayRoot.GetComponent<RectTransform>());

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject titleObj = CreateText(overlayRoot.transform, "Title", "Puzzle Complete!", 72, Color.white, font);
        Anchored(titleObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.62f), new Vector2(900, 140));

        GameObject buttonObj = new GameObject("PlayAgainButton");
        buttonObj.transform.SetParent(overlayRoot.transform, false);
        Image buttonImg = buttonObj.AddComponent<Image>();
        buttonImg.color = accent;
        Button button = buttonObj.AddComponent<Button>();
        Anchored(buttonObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.38f), new Vector2(420, 100));

        GameObject labelObj = CreateText(buttonObj.transform, "Label", "Play Again", 44, Color.black, font);
        Stretch(labelObj.GetComponent<RectTransform>());

        button.onClick.AddListener(OnPlayAgain);
    }

    private void OnPlayAgain()
    {
        if (game != null) game.ResetBoard();
        if (GameManager.Instance != null) GameManager.Instance.ResetToPlaying();
    }

    private static GameObject CreateText(Transform parent, string name, string text, int size, Color color, Font font)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Text t = obj.AddComponent<Text>();
        t.text = text;
        t.font = font;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        return obj;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void Anchored(RectTransform rt, Vector2 center, Vector2 size)
    {
        rt.anchorMin = center;
        rt.anchorMax = center;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
    }
}
