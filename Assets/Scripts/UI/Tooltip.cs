using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode()]
[RequireComponent(typeof(RectTransform))]
public class Tooltip : MonoBehaviour {
    public TextMeshProUGUI headerField;
    public TextMeshProUGUI contentField;
    public LayoutElement layoutElement;
    public int characterWrapLimit;
    public RectTransform rectTransform;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetText(string content, string header = "") {
        headerField.gameObject.SetActive(!string.IsNullOrEmpty(header));
        if (headerField.gameObject.activeSelf) headerField.text = header;
        contentField.text = content;

        UpdateLayoutElement();
    }

    private void Update() {
        if (Application.isEditor) UpdateLayoutElement();

        Vector2 position = Input.mousePosition;
        Vector2 pivot = new Vector2(position.x / Screen.width, position.y / Screen.height);

        rectTransform.pivot = pivot;
        transform.position = position;
    }

    private void UpdateLayoutElement() {
        int headerLength = headerField.text.Length;
        int contentLength = contentField.text.Length;

        layoutElement.enabled = (headerLength > characterWrapLimit || contentLength > characterWrapLimit) ? true : false;
    }
}
