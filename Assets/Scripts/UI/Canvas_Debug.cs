using System.Collections;
using TMPro;
using UnityEngine;

public class Canvas_Debug : MonoBehaviour {
    public float updateTime = 0.2f;

    public TextMeshProUGUI behaviorState;
    public TextMeshProUGUI faceDir;
    public TextMeshProUGUI canMove;
    public TextMeshProUGUI hostileCount;
    public TextMeshProUGUI closestHostile;

    public Actor thisActor;

    // Start is called before the first frame update
    void Start() {
        StartCoroutine(UpdateUI());
    }

    // Update is called once per frame
    void Update() {
    }

    IEnumerator UpdateUI() {
        if (thisActor == null) yield return new WaitForSeconds(updateTime);

        behaviorState.text = thisActor.GetBehaviorState().ToString();
        faceDir.text = thisActor.Facing.ToString();
        canMove.text = ActorUtils.CanMove(thisActor, thisActor.NavGrid.navigationGrid).ToString();
        hostileCount.text = thisActor.Hostiles.Count.ToString();
        closestHostile.text = ActorUtils.GetClosestHostile(thisActor, thisActor.NavGrid.navigationGrid).ToString();
        yield return new WaitForSeconds(updateTime);
    }
}
