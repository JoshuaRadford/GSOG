using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_GridCombat : Singleton<Manager_GridCombat> {
    // Internal data
    private List<Actor> _initiativeOrder;
    public static List<Actor> InitiativeOrder { get => GetInstance()._initiativeOrder; set => GetInstance()._initiativeOrder = value; }
    private uint _currentIndex;
    public static uint CurrentIndex { get => GetInstance()._currentIndex; set => GetInstance()._currentIndex = value; }
    private uint _roundCount;
    public static uint RoundCount { get => GetInstance()._roundCount; set => GetInstance()._roundCount = value; }

    private void Start() {
        if (InitiativeOrder == null) InitiativeOrder = new List<Actor>();
        CurrentIndex = 0;
        RoundCount = 0;
    }

    public static void RestartInitiative(Vector2Int origin, Vector2Int gridRange) {
        List<Actor> actors = FindObjectsOfType<Actor>().ToList();

        if(actors.Count <= 0) return;

        if (InitiativeOrder == null) InitiativeOrder = new List<Actor>();
        else InitiativeOrder.Clear();

        RoundCount = 0;
        
        actors = actors.OrderBy(i => i.Stats.GetInitiative()).ToList();

        foreach (Actor a in actors) {
            Vector2Int actorXY = a.Movement.CellPosition.GetXY;

            if (Mathf.Abs(origin.x - actorXY.x) <= gridRange.x && Mathf.Abs(origin.y - actorXY.y) <= gridRange.y) {
                InitiativeOrder.Add(a);
                a.OnDeath += () => { RemoveActorFromInitiative(a); };
            }
        }

        GetCurrentCharacter().TriggerStartTurn();

        Manager_Input.ActiveActor = GetCurrentCharacter();
        Manager_Camera.GetInstance().ChangeCinemachineFollow(Manager_Input.ActiveActor.Movement.transform);
    }

    public static void StepInitiative() {
        if (InitiativeOrder.Count <= 1) {
            Manager_Scenes.LoadStartMenu();
        }
        else {
            GetCurrentCharacter().TriggerEndTurn();

            CurrentIndex = (CurrentIndex + 1 >= InitiativeOrder.Count) ? 0 : CurrentIndex + 1;

            if (CurrentIndex <= 0) {
                RoundCount++;
            }

            GetCurrentCharacter().TriggerStartTurn();

            Manager_Input.ActiveActor = GetCurrentCharacter();
            Manager_Camera.GetInstance().ChangeCinemachineFollow(Manager_Input.ActiveActor.Movement.transform);
        }
    }

    public static Actor GetCurrentCharacter() {
        return InitiativeOrder[(int)CurrentIndex];
    }

    public static bool RemoveActorFromInitiative(Actor c) {
        return InitiativeOrder.Remove(c);
    }
}
