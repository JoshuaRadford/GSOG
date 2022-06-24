using UnityEngine;

public class SceneManager : Singleton<SceneManager> {
    public const string START_MENU_SCENE = "StartMenu";

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public static void LoadSceneFromIndex(int index) {
        Time.timeScale = 1.0f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneBuildIndex: index);
    }

    public static void LoadStartMenu() {
        Time.timeScale = 1.0f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(START_MENU_SCENE);
    }

    public static void QuitApplication() {
        Application.Quit();
    }
}
