using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ActorUI : MonoBehaviour {
    public Actor thisActor;

    public Slider HealthSlider;
    public Slider StaminaSlider;

    public IEnumerator updateSlidersCoroutine;
    public float waitTimeSeconds = 0.1f;
    public float lifeTime = 5f;
    private float lifeTimeLeft = 5f;

    private void Awake() {
        if (thisActor == null) thisActor = transform.GetComponentInParent<Actor>();
    }

    private void OnEnable() { }

    void Start() { }

    void Update() {
        lifeTimeLeft -= Time.deltaTime;
        if (lifeTimeLeft <= 0) {
            enabled = false;
        }
    }

    private void OnDisable() {
        HealthSlider.gameObject.SetActive(false);
        StaminaSlider.gameObject.SetActive(false);
    }

    public void OnEnableInstructions() {
        if (updateSlidersCoroutine == null) updateSlidersCoroutine = UpdateSliders();

        HealthSlider.gameObject.SetActive(true);
        StaminaSlider.gameObject.SetActive(true);

        ResetTimer();

        StartCoroutine(updateSlidersCoroutine);
    }

    IEnumerator UpdateSliders() {
        while (thisActor != null) {
            if (StaminaSlider.isActiveAndEnabled && StaminaSlider != null) {
                Resource stamina = thisActor.Stats.GetResourceByName("Stamina");
                if (stamina != null) {
                    float toFill = (float)stamina.ModifiedValue / stamina.ModifiedMaxValue;
                    SetFillAmount(Mathf.Lerp(StaminaSlider.value, toFill, waitTimeSeconds * 5f), StaminaSlider);
                }
            }
            // Health
            if (HealthSlider.isActiveAndEnabled && HealthSlider != null) {
                Resource health = thisActor.Stats.GetResourceByName("Health");
                if (health != null) {
                    float toFill = (float)health.ModifiedValue / health.ModifiedMaxValue;
                    SetFillAmount(Mathf.Lerp(HealthSlider.value, toFill, waitTimeSeconds * 5f), HealthSlider);
                }
            }

            yield return new WaitForSeconds(waitTimeSeconds);
        }
    }

    public float SetFillAmount(float value, Slider meter) {
        if (meter.value != value) meter.value = value;
        return meter.value;
    }

    public void ResetTimer() {
        lifeTimeLeft = lifeTime;
    }
}
