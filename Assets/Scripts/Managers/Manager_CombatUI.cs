using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Manager_CombatUI : Singleton<Manager_CombatUI> {
    [SerializeField]
    private TextMeshProUGUI roundValue;
    [SerializeField]
    private Button endTurnButton;

    public bool runUpdate = true;
    private IEnumerator updateUICoroutine;
    private const float DELTA_TIME = 0.5f;

    // Start is called before the first frame update
    void Start() {
        updateUICoroutine = UpdateUI();
        StartCoroutine(updateUICoroutine);
    }

    // Update is called once per frame
    void Update() {

    }

    IEnumerator UpdateUI() {
        while (runUpdate) {
            if (Manager_Input.ActiveActor != null) {
                // End Turn Button
                if (endTurnButton != null) {
                    endTurnButton.gameObject.SetActive(!Manager_Input.ActiveActor.Behavior.isEnabled);
                    endTurnButton.interactable = (Manager_Input.ActiveActor.Behavior.CurrentState is ActorState_Waiting);
                }
            }
            if (roundValue != null) {
                roundValue.text = Manager_GridCombat.RoundCount.ToString();
            }

            yield return new WaitForSeconds(DELTA_TIME);
        }
    }
}
