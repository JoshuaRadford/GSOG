using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CombatManager : Singleton<CombatManager> {
    public event Action OnCombatStart;
    public event Action OnCombatEnd;

    // Internal data
    [SerializeField]
    private bool _inCombat = false;
    public static bool InCombat { get { return GetInstance()._inCombat; } set { GetInstance()._inCombat = value; } }
    private List<Actor> _initiativeOrder;
    public static List<Actor> InitiativeOrder { get => GetInstance()._initiativeOrder; set => GetInstance()._initiativeOrder = value; }
    private Actor _currentActor;
    public static Actor CurrentActor { get => GetInstance()._currentActor; set => GetInstance()._currentActor = value; }
    private int _roundCount = 0;
    public static int RoundCount { get => GetInstance()._roundCount; set => GetInstance()._roundCount = value; }

    // External Data 
    private NavGrid _navGrid;
    public static NavGrid NavGrid { get => GetInstance()._navGrid; set => GetInstance()._navGrid = value; }

    private void OnEnable() {
        Canvas_Combat.Hide();
    }

    private void Start() {
        if (InitiativeOrder == null) InitiativeOrder = new List<Actor>();
    }

    public static void BeginCombat(List<Actor> actors, NavGrid navGrid) {
        if (actors.Count <= 0 || navGrid == null) return;
        Debug.Log("COMBAT BEGAN " + actors.Count);


        // Internal initialization
        InCombat = true;
        NavGrid = navGrid;
        InitiativeOrder = new List<Actor>();
        RoundCount = 0;
        actors = actors.OrderBy(a => a.GetInitiative()).ToList();

        // External initialization
        Canvas_Combat.Show();

        foreach (Actor a in actors) {
            if (!NavGrid.Equals(a.NavGrid)) continue;

            Vector2Int actorXY = a.NavGridPosition;
            InitiativeOrder.Add(a);
            a.OnDeath += () => { RemoveActorFromInitiative(a); };
            Canvas_Combat.AddPortrait(a);
        }

        CurrentActor = InitiativeOrder[0];
        NavGrid.activeActor = CurrentActor;
        CameraManager.GetInstance().ChangeCinemachineFollow(NavGrid.activeActor.transform);
        GetInstance().OnCombatStart?.Invoke();
        CurrentActor.TriggerStartTurn();
    }

    public static void StepInitiative() {
        if (TotalHostiles() <= 0) {
            // End combat
            SceneManager.LoadStartMenu();
            return;
        }

        int currentIndex = InitiativeOrder.IndexOf(CurrentActor);
        if (currentIndex >= InitiativeOrder.Count) currentIndex = -1;

        MoveInitiative(currentIndex + 1);

        if (InitiativeOrder.IndexOf(CurrentActor) <= 0) {
            RoundCount++;
        }
    }

    public static void MoveInitiative(int index) {
        if (TotalHostiles() <= 0) {
            // End combat
            EndInitiative();
        }
        // Loop the index
        index = (index >= InitiativeOrder.Count || index < 0) ? 0 : index;

        // Take a temp of the last current actor and end their turn
        Actor previousActor = CurrentActor;
        if (InitiativeOrder.Contains(previousActor)) previousActor.TriggerEndTurn();

        // Set the new current actor and start their turn
        CurrentActor = InitiativeOrder[index];
        NavGrid.activeActor = CurrentActor;
        CameraManager.GetInstance().ChangeCinemachineFollow(NavGrid.activeActor.transform);
        CurrentActor.TriggerStartTurn();
    }

    public static void EndInitiative() {
        InCombat = false;
        GetInstance().OnCombatEnd?.Invoke();
        Canvas_Combat.Hide();
    }

    public static bool RemoveActorFromInitiative(Actor c) {
        if (c == null) return false;

        bool success = InitiativeOrder.Remove(c);
        MoveInitiative(InitiativeOrder.IndexOf(c));

        if (success) {
            // Remove UI potrait from initiative tracker
            Canvas_Portrait[] portraits = FindObjectsOfType<Canvas_Portrait>();
            foreach (Canvas_Portrait p in portraits) {
                if (c.Equals(p.ThisActor)) {
                    Debug.Log("DESTROYED PORTRAIT");
                    Destroy(p.gameObject);
                    break;
                }
            }

            // Remove actor from others' hostiles
            foreach (Actor a in InitiativeOrder) {
                a.Hostiles.Remove(c);
            }
        }

        return success;
    }

    private static int TotalHostiles() {
        int numHostiles = 0;
        for (int i = 0; i < InitiativeOrder.Count; i++) {
            numHostiles += InitiativeOrder[i].Hostiles.Count;
        }
        return numHostiles;
    }
}
