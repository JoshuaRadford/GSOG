using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Canvas_Portrait : MonoBehaviour {
    public TextMeshProUGUI actorName;
    public Image actorPicture;
    public Slider HealthSlider;
    public Slider StaminaSlider;

    public Actor ThisActor;

    // Start is called before the first frame update
    void Start() {
    }

    private void OnEnable() {
    }

    private void Awake() {
    }

    public void Init() {
        SetPicture(ThisActor?.GetComponent<SpriteRenderer>()?.sprite);
    }

    // Update is called once per frame
    void Update() {
        if (ThisActor != null) {
            Resource stamina = ThisActor?.actorData?.Stamina;
            if (stamina != null) {
                if (StaminaSlider != null && StaminaSlider.isActiveAndEnabled) {
                    float toFill = (float)stamina.GetModifiedValue() / stamina.GetModifiedMaxValue();
                    SetFillAmount(toFill, StaminaSlider);
                }
            }

            Resource health = ThisActor?.actorData?.Health;
            if (health != null) {
                if (HealthSlider != null && HealthSlider.isActiveAndEnabled) {
                    float toFill = (float)health.GetModifiedValue() / health.GetModifiedMaxValue();
                    SetFillAmount(toFill, HealthSlider);
                }
            }

            if (actorName != null) actorName.text = ThisActor?.actorData?.ActorName;

            SetPicture(ThisActor.gameObject.GetComponent<SpriteRenderer>()?.sprite);
        }
    }

    public void SetFillAmount(float value, Slider meter) {
        if (meter == null) return;
        if (meter.value != value) meter.value = value;
    }

    public void SetPicture(Sprite s) {
        if (s == null) return;
        actorPicture.sprite = s;
    }
}
