using Cinemachine;
using UnityEngine;

public class CameraManager : Singleton<CameraManager> {
    private Camera _mainCamera;
    public static Camera MainCamera { get => GetInstance()._mainCamera; set => GetInstance()._mainCamera = value; }
    CinemachineVirtualCamera cinemachineVirtualCamera;

    protected override void Awake() {
        base.Awake();

        MainCamera = Camera.main;
        MainCamera.transparencySortMode = TransparencySortMode.CustomAxis;
        MainCamera.transparencySortAxis = Vector3.up;
        cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Canvas_PauseMenu.ToggleShowHide();
        }
    }

    public void ChangeCinemachineFollow(Transform t) {
        cinemachineVirtualCamera.Follow = t;
    }

    public static bool isVisible(Camera camera, Vector3 worldPos) {
        Vector3 worldToScreen = camera.WorldToScreenPoint(worldPos);
        return (worldPos.x > 0 && worldPos.x < Screen.width &&
                worldPos.y > 0 && worldPos.y < Screen.height);
    }
}
