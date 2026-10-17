using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

// Builds the prototype presentation from the named scene hierarchy at Play time.
public class UIManager : MonoBehaviour
{
    GameManager game;
    Font font;
    Text status, clock, progressLabel, efficiencyLabel;
    Image projectFill, efficiencyFill;
    Transform layer;
    [Tooltip("Disable after replacing the temporary room artwork. Zone layout is always preserved.")]
    public bool showPrototypeRoom = true;
    readonly Color mint = new Color(.36f, .87f, .66f);
    readonly Color danger = new Color(.95f, .40f, .30f);
    readonly Color panel = new Color(.12f, .19f, .20f);
    void Start()
    {
        game = FindFirstObjectByType<GameManager>();
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var canvas = gameObject.GetComponent<Canvas>();
        if (!canvas) canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = .5f;
        gameObject.AddComponent<GraphicRaycaster>();
        if (!FindFirstObjectByType<EventSystem>())
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        if (showPrototypeRoom) BuildRoom();
        var hud = transform.Find("HUD");
        Label(hud, "Title", "OLD TECH / ОБЧИСЛЮВАЛЬНИЙ ЦЕНТР", 30, 18, 900, 36, 26, mint);
        clock = Label(hud, "Deadline", "", 1010, 20, 240, 40, 23, Color.white);
        projectFill = Bar(hud.Find("ProjectProgressBar"), 30, 72, mint);
        progressLabel = Label(hud, "ProgressLabel", "", 40, 74, 540, 28, 18, Color.white);
        efficiencyFill = Bar(hud.Find("EfficiencyBar"), 670, 72, mint);
        efficiencyLabel = Label(hud, "EfficiencyLabel", "", 680, 74, 540, 28, 18, Color.white);
        status = ConfigureText(hud.Find("StatusText").gameObject, 30, 112, 1220, 34, 19, Color.white);
        Label(hud, "Help", "Натисніть червону панель, щоб усунути поломку. Час продовжує йти під час ремонту.", 30, 655, 1220, 40, 18, mint);
        layer = transform.Find("MinigameLayer");
        Place(layer.gameObject, 0, 0, 1280, 720);
        game.Minigames.minigameLayer = (RectTransform)layer;
        BuildEnd(transform.Find("WinPanel"), "ПРОЄКТ ГОТОВИЙ", "Команда завершила роботу вчасно.");
        BuildEnd(transform.Find("LosePanel"), "ЧАС ВИЙШОВ", "Проєкт не завершено. Спробуйте ремонтувати швидше.");
    }
    void BuildRoom()
    {
        var room = GameObject.Find("Room");
        var roomCanvas = room.GetComponent<Canvas>();
        if (!roomCanvas) roomCanvas = room.AddComponent<Canvas>();
        roomCanvas.renderMode = RenderMode.ScreenSpaceOverlay; roomCanvas.sortingOrder = -1;
        var scaler = room.GetComponent<CanvasScaler>();
        if (!scaler) scaler = room.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = .5f;
        if (!room.GetComponent<GraphicRaycaster>()) room.AddComponent<GraphicRaycaster>();
        var bg = room.transform.Find("Background");
        Box(bg.gameObject, 0, 0, 1280, 720, new Color(.055f, .09f, .11f));
        for (int i = 0; i < 12; i++) Box(New(bg, "WallSeam"), i * 115, 160, 2, 480, new Color(.09f,.14f,.16f));
        var computer = room.transform.Find("MainframeComputer");
        var image = computer.GetComponent<Image>();
        if (!image) image = computer.gameObject.AddComponent<Image>();
        image.color = new Color(.25f,.30f,.28f);
        Label(computer, "MachineLabel", "М-1964   /   ГОЛОВНИЙ ОБЧИСЛЮВАЧ", 25, 16, 1080, 40, 25, mint);
    }
    void Update()
    {
        if (!game || !status) return;
        projectFill.rectTransform.localScale = new Vector3(game.Project.Progress, 1, 1);
        efficiencyFill.rectTransform.localScale = new Vector3(game.Efficiency.Value, 1, 1);
        efficiencyFill.color = game.Efficiency.Value < game.balance.stopThreshold ? danger : mint;
        progressLabel.text = $"ПРОЄКТ: {game.Project.Progress:P0}";
        efficiencyLabel.text = $"ПРОДУКТИВНІСТЬ: {game.Efficiency.Value:P0} / поріг {game.balance.stopThreshold:P0}";
        clock.text = $"ДО ДЕДЛАЙНУ {Mathf.CeilToInt(game.RemainingSeconds)} с";
        status.text = game.Efficiency.Value < game.balance.stopThreshold ? "РОБОТУ ЗУПИНЕНО — відремонтуйте обладнання!" : $"Програмісти працюють. Активних поломок: {game.Breakdowns.ActiveCount}";
        transform.Find("WinPanel").gameObject.SetActive(game.Finished && game.Won);
        transform.Find("LosePanel").gameObject.SetActive(game.Finished && !game.Won);
    }
    void BuildEnd(Transform root, string title, string message)
    {
        Box(root.gameObject, 0, 0, 1280, 720, new Color(.035f,.075f,.08f,.99f));
        Label(root, "Title", title, 200, 210, 900, 70, 44, mint);
        Label(root, "Message", message, 200, 310, 900, 90, 25, Color.white);
        MakeButton(root, "Restart", "ПОЧАТИ ЗНОВУ", 440, 460, 400, 70, () => game.Restart());
        root.gameObject.SetActive(false);
    }
    GameObject New(Transform parent, string name)
    { var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false); return go; }
    void Place(GameObject go, float x, float y, float w, float h)
    {
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h);
    }
    Image Box(GameObject go, float x, float y, float w, float h, Color color)
    {
        Place(go, x, y, w, h);
        var img = go.GetComponent<Image>(); if (!img) img = go.AddComponent<Image>(); img.color = color; return img;
    }
    Text ConfigureText(GameObject go, float x, float y, float w, float h, int size, Color color)
    {
        Place(go, x, y, w, h); var text = go.AddComponent<Text>(); text.font = font; text.fontSize = size;
        text.color = color; text.alignment = TextAnchor.MiddleLeft; text.raycastTarget = false; return text;
    }
    Text Label(Transform parent, string name, string content, float x, float y, float w, float h, int size, Color color)
    { var text = ConfigureText(New(parent, name), x, y, w, h, size, color); text.text = content; return text; }
    Image Bar(Transform root, float x, float y, Color color)
    {
        Box(root.gameObject, x, y, 580, 34, panel);
        var fill = Box(New(root, "Fill"), 0, 0, 580, 34, color); fill.raycastTarget = false; return fill;
    }
    Button MakeButton(Transform parent, string name, string caption, float x, float y, float w, float h, UnityEngine.Events.UnityAction action)
    {
        var go = New(parent, name); var image = Box(go, x, y, w, h, new Color(.20f,.34f,.33f));
        var button = go.AddComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(action);
        var text = Label(go.transform, "Label", caption, 8, 4, w - 16, h - 8, 22, Color.white); text.alignment = TextAnchor.MiddleCenter;
        return button;
    }
}
