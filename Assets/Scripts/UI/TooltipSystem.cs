public class TooltipSystem : Singleton<TooltipSystem> {
    public Tooltip tooltip;

    protected override void Awake() {
        base.Awake();

        tooltip.gameObject.SetActive(false);
    }

    public static void Show(string content, string header = "") {
        GetInstance().tooltip.SetText(content, header);
        GetInstance().tooltip.gameObject.SetActive(true);
    }

    public static void Hide() {
        GetInstance().tooltip.gameObject.SetActive(false);
    }
}
