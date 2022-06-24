using UnityEngine;

public class Canvas_PauseMenu : Singleton<Canvas_PauseMenu> {
    public GameObject background;
    private bool IsVisible;

    protected override void Awake() {
        base.Awake();
        GetInstance().IsVisible = false;
        GetInstance().background?.SetActive(false);
        GetInstance().gameObject.SetActive(false);
    }

    private void Start() {

    }

    private void Update() {
    }

    public static void Show() {
        GetInstance().IsVisible = true;
        GetInstance().PauseGame();
        GetInstance().background?.SetActive(true);
        GetInstance().gameObject.SetActive(true);
    }

    public static void Hide() {
        GetInstance().IsVisible = false;
        GetInstance().UnpauseGame();
        GetInstance().background?.SetActive(false);
        GetInstance().gameObject.SetActive(false);
    }

    public static void ToggleShowHide() {
        if (GetInstance().IsVisible) Hide();
        else Show();
    }

    public void PauseGame() {
        Time.timeScale = 0.0f;
    }

    public void UnpauseGame() {
        Time.timeScale = 1.0f;
    }
}
