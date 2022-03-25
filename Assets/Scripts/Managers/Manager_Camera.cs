using Cinemachine;
using UnityEngine;

public class Manager_Camera : Singleton<Manager_Camera> {
    Camera mainCamera;
    CinemachineVirtualCamera cinemachineVirtualCamera;

    protected override void Awake() {
        base.Awake();

        mainCamera = Camera.main;
        cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void ChangeCinemachineFollow(Transform t) {
        cinemachineVirtualCamera.Follow = t;
    }
}
