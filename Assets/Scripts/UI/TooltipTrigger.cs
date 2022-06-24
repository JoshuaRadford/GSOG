using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public string header;
    [Multiline()]
    public string content;

    private static Coroutine delay;

    private void Show() {
        delay = this.Wait(1f, () => {
            TooltipSystem.Show(content, header);
        });
    }

    private void Hide() {
        StopCoroutine(delay);
        TooltipSystem.Hide();
    }


    public void OnPointerEnter(PointerEventData eventData) {
        Show();
    }
    public void OnPointerExit(PointerEventData eventData) {
        Hide();
    }
    private void OnMouseEnter() {
        Show();
    }
    private void OnMouseExit() {
        Hide();
    }
}
