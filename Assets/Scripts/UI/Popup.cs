using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class Popup : MonoBehaviour {
    public Button previousButton;
    public Button nextButton;
    public TextMeshProUGUI textContent;

    public List<string> contents;
    [HideInInspector]
    public int currentIndex = 0;

    void Start() {

    }

    private void OnEnable() {

    }

    void Update() {

    }

    public void ToNext() { currentIndex++; }
    public void ToPrevious() { currentIndex--; }

    public void CreatePopup(List<string> contents) {
        if (contents == null || contents.Count <= 0) return;

        this.contents = contents;
        currentIndex = 0;

        RefreshUI();
    }

    public void RefreshUI() {
        if (previousButton != null && currentIndex <= 0) previousButton.gameObject.SetActive(false);
        else if(contents != null && nextButton != null && currentIndex >= contents.Count - 1) nextButton.gameObject.SetActive(false);

        if(textContent != null) textContent.text = contents[currentIndex];
    }

    public void ExitDestroy() {
        Destroy(gameObject);
    }
}
