using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Canvas_Combat : Singleton<Canvas_Combat> {
    [SerializeField]
    private RectTransform rectTransform;
    [SerializeField]
    private TextMeshProUGUI roundValue;
    [SerializeField]
    private Button endTurnButton;
    [SerializeField]
    private GameObject PORTRAIT_PREFAB;
    [SerializeField]
    private GameObject portraitContainer;

    public bool runUpdate = true;
    private IEnumerator updateUICoroutine;
    private const float DELTA_TIME = 0.5f;

    protected override void Awake() {
        base.Awake();

        endTurnButton.onClick.AddListener(CombatManager.StepInitiative);
    }

    private void OnEnable() {

    }

    // Start is called before the first frame update
    void Start() {
        updateUICoroutine = UpdateUI();
        StartCoroutine(updateUICoroutine);
    }

    // Update is called once per frame
    void Update() {

    }

    public static void Show() {
        if (GetInstance().rectTransform == null) return;

        GetInstance().rectTransform.anchoredPosition = new Vector2(0, 0);
    }

    public static void Hide() {
        if (GetInstance().rectTransform == null) return;

        GetInstance().rectTransform.anchoredPosition = new Vector2(0, 250);
    }

    public static void AddPortrait(Actor a) {
        if (GetInstance().PORTRAIT_PREFAB == null || GetInstance().portraitContainer == null || a == null) return;

        Canvas_Portrait portrait = Instantiate(GetInstance().PORTRAIT_PREFAB, GetInstance().portraitContainer.transform).GetComponent<Canvas_Portrait>();
        portrait.ThisActor = a;
        portrait.Init();
    }

    IEnumerator UpdateUI() {
        while (runUpdate) {
            if (CombatManager.CurrentActor != null) {
                // End Turn Button
                if (endTurnButton != null) {
                    endTurnButton.gameObject.SetActive(!CombatManager.CurrentActor.enableAI);
                    endTurnButton.interactable = (CombatManager.CurrentActor.GetBehaviorState() is ActorState_Waiting);
                }
            }
            if (roundValue != null) {
                roundValue.text = CombatManager.RoundCount.ToString();
            }

            yield return new WaitForSeconds(DELTA_TIME);
        }
    }
}
