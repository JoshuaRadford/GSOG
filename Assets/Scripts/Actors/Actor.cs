using System;
using UnityEngine;

[RequireComponent(typeof(ActorMovement))]
public class Actor : MonoBehaviour {
    public event Action OnTurnStart;
    public event Action OnTurnEnd;
    public event Action OnDeath;

    private ActorUI _ui;
    private ActorBehavior _behavior;
    private ActorMovement _movement;
    [SerializeField]
    private ActorStats _stats;
    private Inventory _inventory;
    [SerializeField]
    private string _actorName;

    public ActorUI UI { get => _ui; set => _ui = value; }
    public ActorBehavior Behavior { get => _behavior; set => _behavior = value; }
    public ActorMovement Movement { get => _movement; set => _movement = value; }
    public Inventory Inventory { get => _inventory; set => _inventory = value; }
    public ActorStats Stats { get => _stats; set => _stats = value; }
    public string ActorName { get => _actorName; set => _actorName = value; }
    public bool ExpendedSA { get; set; }
    public bool ExpendedBA { get; set; }

    private Material outlineMat;
    private Material defaultMat;

    private void Awake() {
        if (Movement == null) Movement = GetComponent<ActorMovement>();
        if (UI == null) UI = GetComponentInChildren<ActorUI>();
        if (Behavior == null) Behavior = GetComponent<ActorBehavior>();
    }

    private void OnEnable() {
        if (Stats == null || Stats.stats.Count <= 0) Stats = new ActorStats();

        Stats.OnStatsChanged += () => {
            UI.enabled = true;
            UI.OnEnableInstructions();
        };
        OnTurnStart += () => {
            ExpendedSA = false;
            ExpendedBA = false;
            Stats.SetResourceToMax("Stamina");
        };
    }

    private void Start() {
        if (ActorName == null) ActorName = gameObject.name;

        outlineMat = Resources.Load("Materials/OutlineSprite", typeof(Material)) as Material;
        defaultMat = GetComponent<SpriteRenderer>().material;
    }

    public void TriggerEndTurn() {
        OnTurnEnd?.Invoke();
    }

    public void TriggerStartTurn() {
        OnTurnStart?.Invoke();
    }

    public void TryIsDead() {
        if (Stats.GetHealthValue() <= 0) Die();
    }

    public void Die() {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    private void OnMouseEnter() {
        GetComponent<SpriteRenderer>().material = outlineMat;
    }

    private void OnMouseExit() {
        GetComponent<SpriteRenderer>().material = defaultMat;
    }

    private void OnMouseUpAsButton() {
        if (Manager_Input.ActiveActor.Behavior.CurrentState is ActorState_Waiting &&
            !Manager_Input.ActiveActor.Behavior.isEnabled) {
            if (Movement.IsNeighbor(Manager_Input.ActiveActor, Manager_Grid.TerrainOverlay)) {
                Manager_Input.ActiveActor.Behavior.TryAttack(this);
            }
        }
    }
}
