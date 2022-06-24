using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FaceDirection {
    UP,
    DOWN,
    LEFT,
    RIGHT,
    UPLEFT, UPRIGHT,
    DOWNLEFT, DOWNRIGHT
}
public enum FaceAngle {
    NONE,
    STRAIGHT,
    ACUTE,
    OBTUSE,
    RIGHT,
}

[RequireComponent(typeof(Animator))]
public class Actor : MonoBehaviour, IInteractable, IAutonomous, IHighlightable {
    [HideInInspector] public Animator Animator;

    public event Action<IActorState, IActorState> OnBehaviorChange;

    public ActorData actorData;

    public bool enableAI = false;

    [Header("Editor Data")]
    public Material outlineMat_Red;
    public Material outlineMat_Green;
    public Material outlineMat_Yellow;
    private Material defaultMat;

    // Combat
    public event Action OnTurnStart;
    public event Action OnTurnEnd;
    public event Action OnDeath;

    public bool ExpendedSA;
    public bool ExpendedBA;

    // Behavior
    [HideInInspector]
    public ActorState_Idle STATE_IDLE; // Taking no actions, No priority
    public ActorState_Waiting STATE_WAITING; // Taking no actions, Has priority
    public ActorState_Seeking STATE_SEEKING; // Moving at target location
    public ActorState_Attacking STATE_ATTACKING; // Attacking target object
    public HashSet<Actor> Hostiles;
    private IActorState behaviorState;

    // Movement
    private Vector2Int _navGridPosition;
    private Vector3 _worldPosition = new Vector3(0, 0, LOCK_Z);
    private Vector3 initialPosition;
    private FaceDirection _facing = FaceDirection.DOWN;
    public Vector2Int NavGridPosition { get { return _navGridPosition; } set { if (value != _navGridPosition) { onGridPositionChanged?.Invoke(_navGridPosition, value); _navGridPosition = value; } } }
    public void SetBaseNavGridPosition(Vector2Int index) { _navGridPosition = index; }
    public Cell NavGridCell { get { return NavGrid.navigationGrid.GetCell(NavGridPosition); } }
    public Vector3 WorldPosition { get { return _worldPosition; } set { if (value != _worldPosition) _worldPosition = new Vector3(value.x, value.y, LOCK_Z); } }
    public FaceDirection Facing { get { return _facing; } set { if (value != _facing) _facing = value; } }

    private int currentPathIndex;
    private List<Vector2Int> pathIndexList;
    private NavGrid _navGrid;
    public NavGrid NavGrid { get { return _navGrid; } set { if (value != _navGrid) _navGrid = value; } } 

    private Coroutine seekCoroutine;
    private bool seekCoroutineRunning = false;
    private float elapsedTime;
    private float waitTime = 0.25f;
    [HideInInspector]
    public bool seeking = false;
    public event Action onStopSeeking;
    public event Action onStartSeeking;
    public event Action<Vector2Int, Vector2Int> onGridPositionChanged;

    [Header("Audio Data")]
    // Audio
    public AudioClip[] attackClips;
    public AudioClip[] walkClips;


    private const float LOCK_Z = -3f;
    public const float ACTION_BUFFER = 1.0f;

    private void Awake() {
        if (Animator == null) Animator = GetComponent<Animator>();
    }

    private void OnEnable() {
        onStartSeeking += () => { SetBehaviorState(STATE_SEEKING); };
        onStartSeeking += () => { NavGrid.UpdateDrawSyncMovement(); };
        onStopSeeking += () => { SetBehaviorState(STATE_WAITING); };
        onStopSeeking += () => { NavGrid.UpdateDrawSyncMovement(); };
        OnTurnStart += DoOnTurnStart;
        OnTurnEnd += DoOnTurnEnd;
        OnDeath += DoOnDeath;
    }

    private void Reset() {
        actorData = new ActorData();
    }

    private void Start() {
        // Initialize Materials
        if (outlineMat_Red == null) outlineMat_Red = Resources.Load("Materials/OutlineSprite", typeof(Material)) as Material;
        defaultMat = GetComponent<SpriteRenderer>().material;

        // Initialize Behavior
        STATE_IDLE = new ActorState_Idle(this);
        STATE_WAITING = new ActorState_Waiting(this);
        STATE_SEEKING = new ActorState_Seeking(this);
        STATE_ATTACKING = new ActorState_Attacking(this);
        behaviorState = STATE_WAITING;
        Hostiles = new HashSet<Actor>();

        onGridPositionChanged += (Vector2Int oldIndex, Vector2Int newIndex) => {
            NavGrid.navigationGrid.GetCell(oldIndex).ContainsActor = false;
            NavGrid.navigationGrid.GetCell(oldIndex).PathNode.Walkable = true;
            NavGrid.navigationGrid.GetCell(newIndex).ContainsActor = true;
            NavGrid.navigationGrid.GetCell(newIndex).PathNode.Walkable = false;
            NavGrid.UpdateDrawSyncMovement();
            if(!CombatManager.InCombat) CheckForHostiles();
        };
    }

    private void Update() {
        transform.position = WorldPosition;
        behaviorState.DoState();

        if (Facing == FaceDirection.LEFT || Facing == FaceDirection.DOWNLEFT || Facing == FaceDirection.UPLEFT) 
            GetComponent<SpriteRenderer>().flipX = true;
        else if (Facing == FaceDirection.RIGHT || Facing == FaceDirection.DOWNRIGHT || Facing == FaceDirection.UPRIGHT) 
            GetComponent<SpriteRenderer>().flipX = false;
    }

    // Behavior
    public void SetBehaviorState(IActorState state) {
        if (state == null) return;

        behaviorState.Exit();

        IActorState temp = behaviorState;
        behaviorState = state;
        if (state != temp) OnBehaviorChange?.Invoke(temp, state);

        behaviorState.Enter();
    }
    public IActorState GetBehaviorState() {
        return behaviorState;
    }
    public void AddHostile(params Actor[] actor) {
        if (Hostiles == null) Hostiles = new HashSet<Actor>(); ;

        foreach (Actor a in actor) {
            if (Hostiles.Contains(a)) continue;
            Hostiles.Add(a);
        }
    }
    public void RemoveHostile(params Actor[] actor) {
        if (Hostiles == null || Hostiles.Count <= 0) return;

        foreach (Actor a in actor) Hostiles.Remove(a);
    }

    // Combat
    public void TriggerEndTurn() {
        OnTurnEnd?.Invoke();
    }
    public void TriggerStartTurn() {
        OnTurnStart?.Invoke();
    }
    public void TriggerDeath() {
        OnDeath?.Invoke();
    }
    private void DoOnTurnStart() {
        ExpendedSA = false;
        ExpendedBA = false;
        Highlight();
        actorData.Stamina.SetBaseToMax();
        SetBehaviorState(STATE_WAITING);
        NavGrid.UpdateDrawSyncMovement();
    }

    private void DoOnTurnEnd() {
        DeHighlight();
        SetBehaviorState(STATE_IDLE);
    }

    private void DoOnDeath() {
        NavGridCell.ContainsActor = false;
        NavGridCell.PathNode.Walkable = true;
    }

    public bool TryAttack(Actor actor) {
        if (!ExpendedSA) {
            // Actor prepares to attack
            Facing = FacingFromMove(NavGridPosition, actor.NavGridPosition);

            ExpendedSA = true;

            // After a pause, make the attack
            this.Wait(0.4f, () => ConfirmAttack(actor));
        }
        return ExpendedSA;
    }

    public void ConfirmAttack(Actor onActor) {
        AudioManager.PlayAudioClip(attackClips, GetComponent<AudioSource>());

        Manager_Effects.Attack(this, onActor);

        FaceAngle angle = GetFaceAngle(this.Facing, onActor.Facing);
        float flankMod = (angle == FaceAngle.NONE) ? 2f : 1f;
        onActor.actorData.Health.IncrementBaseValue(Mathf.FloorToInt(-actorData.Strength.GetModifiedValue() * 3 * flankMod));

        SetBehaviorState(STATE_WAITING);
        ActorUtils.TryDie(onActor);
    }

    public void CheckForHostiles() {
        Actor[] actors = FindObjectsOfType<Actor>();
        bool startCombat = false;
        List<Actor> toCombat = new List<Actor>();

        // Helper function to prevent duplicate additions
        void AddToCombat(Actor a) {
            //if (toCombat.Contains(a)) return;
            toCombat.Add(a);
        }

        AddToCombat(this);

        foreach (Actor actor in actors) {
            if (actor == this) continue; // Ignore Self

            Inclination inc = this.actorData.GetInclination(actor);
            Personality pers = actor.actorData.Personality;
            //Debug.Log($"{actorData.ActorName} - {actor.actorData.ActorName} : inc({inc.ToString()}), pers({pers.ToString()})");
            // Actor will start combat if they are aggressive and on bad terms
            if (inc == Inclination.Negative && pers == Personality.Aggressive) {
                Debug.Log("found mean guy");
                startCombat = true;
                AddToCombat(actor);
                AddHostile(actor);
                actor.AddHostile(this);
            }
        }

        foreach (Actor actor in actors) {
            if (actor == this) continue; // Ignore Self

            Inclination inc = this.actorData.GetInclination(actor);
            Personality pers = actor.actorData.Personality;

            // Actor will help if they are benevolent and on good terms
            if (inc == Inclination.Positive && pers == Personality.Benevolent) {
                Debug.Log("found nice guy");
                AddToCombat(actor);
                actor.AddHostile(this.Hostiles.ToArray());
                foreach(Actor h in Hostiles) {
                    h.AddHostile(actor);
                }
            }
        }

        // If we have found a hostile enemy, begin combat
        if (startCombat) {
            CombatManager.BeginCombat(toCombat.ToList(), NavGrid);
        }
        else {
            foreach (Actor actor in toCombat) {
                actor.Hostiles.Clear();
            }
        }
    }

    // Movement
    public Vector2Int GetVectorFacing(FaceDirection faceDirection) {
        switch (faceDirection) {
            case FaceDirection.UP:
                return Vector2Int.up;
            case FaceDirection.DOWN:
                return Vector2Int.down;
            case FaceDirection.LEFT:
                return Vector2Int.left;
            case FaceDirection.RIGHT:
                return Vector2Int.right;
            case FaceDirection.UPRIGHT:
                return Vector2Int.up + Vector2Int.right;
            case FaceDirection.UPLEFT:
                return Vector2Int.up + Vector2Int.left;
            case FaceDirection.DOWNRIGHT:
                return Vector2Int.down + Vector2Int.right;
            case FaceDirection.DOWNLEFT:
                return Vector2Int.down + Vector2Int.left;
        }


        return Vector2Int.zero;
    }

    public FaceDirection FacingFromMove(Vector2Int old, Vector2Int current) {
        if (current.x < old.x && current.y < old.y) return FaceDirection.DOWNLEFT;
        if (current.x < old.x && current.y > old.y) return FaceDirection.UPLEFT;
        if (current.x < old.x && current.y == old.y) return FaceDirection.LEFT;
        if (current.x > old.x && current.y < old.y) return FaceDirection.DOWNRIGHT;
        if (current.x > old.x && current.y > old.y) return FaceDirection.UPRIGHT;
        if (current.x > old.x && current.y == old.y) return FaceDirection.RIGHT;
        if (current.x == old.x && current.y < old.y) return FaceDirection.DOWN;
        if (current.x == old.x && current.y > old.y) return FaceDirection.UP;
        return FaceDirection.DOWN;
    }

    public FaceAngle GetFaceAngle(FaceDirection current, FaceDirection target) {
        Vector2Int currentDV = GetVectorFacing(current);
        Vector2Int targetDV = GetVectorFacing(target);
        float theta = Mathf.Acos(Vector2.Dot(currentDV, targetDV) / (currentDV.magnitude * targetDV.magnitude));
        return (theta == 90) ? FaceAngle.RIGHT : (theta < 90) ? (theta == 0) ? FaceAngle.NONE : FaceAngle.ACUTE : (theta == 180) ? FaceAngle.STRAIGHT : FaceAngle.OBTUSE;
    }

    public void SetTargetCell(Vector2Int index, Action onArrive = null) {
        Cell targetCell = NavGrid.navigationGrid.GetCell(index);
        pathIndexList = Pathfinding.FindPath(NavGridPosition, targetCell.GetXY(), targetCell.parentGrid).Select(node => node.GetXY())?.ToList();

        if (pathIndexList != null && pathIndexList.Count > 0) {
            if (pathIndexList.Count > 1) pathIndexList.RemoveAt(0);
            if (seekCoroutine != null) StopCoroutine(seekCoroutine);

            elapsedTime = 0;
            currentPathIndex = 0;
            seeking = true;
            seekCoroutineRunning = true;

            initialPosition = transform.position;
            seekCoroutine = StartCoroutine(SeekTargetLocation(onArrive));

            onStartSeeking?.Invoke();
        }
    }

    IEnumerator SeekTargetLocation(Action onArrive = null) {
        while (pathIndexList != null) {
            if (seekCoroutineRunning) {
                Vector3 targetPosition = NavGrid.navigationGrid.GetWorldPosition(pathIndexList[currentPathIndex]);               
                Vector3 startingPosition = (currentPathIndex - 1 >= 0) ? NavGrid.navigationGrid.GetWorldPosition(pathIndexList[currentPathIndex - 1]) : initialPosition;
                
                for (int i = 0; i < pathIndexList.Count - 1; i++) Debug.DrawLine(NavGrid.navigationGrid.GetWorldPosition(pathIndexList[i]), NavGrid.navigationGrid.GetWorldPosition(pathIndexList[i+1]), Color.red, 3f);

                float distanceBefore = Vector2.Distance(transform.position, targetPosition);

                // Arrived at next position
                if (distanceBefore <= 0.05) {
                    Cell thisCell = NavGrid.navigationGrid.GetCell(NavGridPosition);
                    Cell nextCell = NavGrid.navigationGrid.GetCell(pathIndexList[currentPathIndex]);

                    // Deplete stamina
                    actorData.Stamina.IncrementBaseValue(-Pathfinding.CalculateDistanceCost(thisCell.PathNode, nextCell.PathNode));

                    // Face the direction
                    Facing = FacingFromMove(NavGridPosition, pathIndexList[currentPathIndex]);

                    // Assign grid index
                    NavGridPosition = pathIndexList[currentPathIndex];

                    // Iterate path list
                    elapsedTime = 0;
                    currentPathIndex++;
                    seekCoroutineRunning = false;

                    AudioManager.PlayAudioClip(walkClips, GetComponent<AudioSource>());

                    if (currentPathIndex >= pathIndexList.Count) {
                        WorldPosition = targetPosition;

                        currentPathIndex = 0;
                        pathIndexList.Clear();

                        onStopSeeking?.Invoke();
                        seeking = false;

                        onArrive?.Invoke();

                        StopCoroutine(seekCoroutine);
                    }

                    this.Wait(0f, () => { seekCoroutineRunning = true; });
                }
                else {
                    WorldPosition = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / waitTime);
                    //SetWorldPosition(Vector3.SmoothDamp(worldPosition, LockZ(targetPosition), ref moveVelocity, 0.1f));
                    elapsedTime += Time.deltaTime;
                }
            }

            yield return null;
        }
    }

    // Mouse Events
    private void OnMouseOver() {
        if (Input.GetMouseButtonDown(1)) {
            Canvas_ActorActions.ThisActor = this;
            Canvas_ActorActions.Show();
        }
    }
    private void OnMouseEnter() {
        Highlight();
    }
    private void OnMouseExit() {
        if (!this.Equals(NavGrid.activeActor)) {
            DeHighlight();
        }
    }
    private void OnMouseUpAsButton() {
        ActorUtils.TryManualAttack(NavGrid.activeActor, this);
    }

    // IInteractable implementation
    public int GetHP() {
        return actorData.Health.GetModifiedValue();
    }
    public void SetHP(int hp) {
        actorData.Health.SetBaseValue(hp);
    }
    // IAutonomous implementation
    public int GetMoveRange() {
        return (actorData.Stamina != null) ? actorData.Stamina.GetModifiedValue() : 0;
    }
    public int GetInitiative() {
        return (actorData.Dexterity != null) ? actorData.Dexterity.GetModifiedValue() : 0;
    }
    //IHighlightable implementation
    public void Highlight() {
        Material mat = (NavGrid.activeActor.Hostiles.Contains(this)) ? outlineMat_Yellow : outlineMat_Green;
        GetComponent<SpriteRenderer>().material = mat;
    }
    public void DeHighlight() {
        GetComponent<SpriteRenderer>().material = defaultMat;
    }
}
