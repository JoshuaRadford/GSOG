using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Canvas_ActorActions : Singleton<Canvas_ActorActions>, IPointerExitHandler {
    public static Actor ThisActor;

    public RectTransform StatsPanel;
    public Slider HealthSlider;
    public Slider StaminaSlider;
    public Button AttackButton;
    public TextMeshProUGUI CurrentHP;
    public TextMeshProUGUI MaxHP;
    public TextMeshProUGUI CurrentSP;
    public TextMeshProUGUI MaxSP;

    public float waitTimeSeconds = 0.1f;
    public float lifeTime = 5f;
    private float lifeTimeLeft = 5f;

    protected override void Awake() {
        base.Awake();
    }

    private void Start() {
        Hide();
    }

    void Update() {
        lifeTimeLeft -= Time.deltaTime;
        if (lifeTimeLeft <= 0) {
            //enabled = false;
        }
        UpdateDisplay();
    }

    public void OnPointerExit(PointerEventData eventData) {
        Hide();
    }

    public static void Show() {
        GetInstance().gameObject.SetActive(true);


        // Attack Option
        if (GetInstance().AttackButton != null) {
            bool canMoveAndAttack = ActorUtils.CanMoveAndAttack(ThisActor.NavGrid.activeActor, ThisActor);
            if (canMoveAndAttack != GetInstance().AttackButton.gameObject.activeSelf) {
                GetInstance().AttackButton.gameObject.SetActive(canMoveAndAttack);
            }
        }
    }

    public static void Hide() {
        GetInstance().gameObject.SetActive(false);
    }

    private void UpdateDisplay() {
        if (ThisActor != null) {
            transform.parent.position = ThisActor.transform.localPosition + new Vector3(0, ThisActor.transform.localScale.y / 2, 0);

            // Stamina
            Resource stamina = ThisActor?.actorData.Stamina;
            if (stamina != null) {
                if (StaminaSlider != null && StaminaSlider.isActiveAndEnabled) {
                    float toFill = (float)stamina.GetModifiedValue() / stamina.GetModifiedMaxValue();
                    SetFillAmount(Mathf.Lerp(StaminaSlider.value, toFill, waitTimeSeconds * 5f), StaminaSlider);
                }
                if (CurrentSP != null && CurrentSP.isActiveAndEnabled) {
                    CurrentSP.text = stamina.GetModifiedValue().ToString();
                }
                if (MaxSP != null && MaxSP.isActiveAndEnabled) {
                    MaxSP.text = stamina.GetModifiedMaxValue().ToString();
                }
            }
            // Health
            Resource health = ThisActor?.actorData.Health;
            if (health != null) {
                if (HealthSlider != null && HealthSlider.isActiveAndEnabled) {
                    float toFill = (float)health.GetModifiedValue() / health.GetModifiedMaxValue();
                    SetFillAmount(Mathf.Lerp(HealthSlider.value, toFill, waitTimeSeconds * 5f), HealthSlider);
                }
                if (CurrentHP != null && CurrentHP.isActiveAndEnabled) {
                    CurrentHP.text = health.GetModifiedValue().ToString();
                }
                if (MaxHP != null && MaxHP.isActiveAndEnabled) {
                    MaxHP.text = health.GetModifiedMaxValue().ToString();
                }
            }
        }
    }

    public void MoveToAndAttack() {
        Actor activeActor = ThisActor.NavGrid.activeActor;
        ActorUtils.TryMoveAtActor(activeActor, ThisActor, () => { ActorUtils.TryManualAttack(activeActor, ThisActor); });
    }

    public void SetFillAmount(float value, Slider meter) {
        if (meter == null) return;
        if (meter.value != value) meter.value = value;
    }

    public void ResetTimer() {
        lifeTimeLeft = lifeTime;
    }
}
