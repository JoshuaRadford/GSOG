using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager_Scenes : Singleton<Manager_Scenes> {
    public const string START_MENU_SCENE = "StartMenu";

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public static void LoadSceneFromIndex(int index) {
        SceneManager.LoadScene(sceneBuildIndex: index);
    }

    public static void LoadStartMenu() {
        SceneManager.LoadScene(START_MENU_SCENE);
    }

    public static void QuitApplication() {
        Application.Quit();
    }
}
