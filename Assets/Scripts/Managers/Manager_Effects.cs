using UnityEngine;

public class Manager_Effects : Singleton<Manager_Effects> {
    public Animator animPlayer;

    protected override void Awake() {
        base.Awake();

        if (animPlayer == null) animPlayer = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public static void Attack(Actor source, Actor target) {
        if (source == null || target == null) return;

        Vector3 sourcePos = source.WorldPosition;
        Vector3 targetPos = target.WorldPosition;
        Vector3 playPos = sourcePos + (targetPos - sourcePos) / 2;
        playPos.x += target.transform.localScale.x / 2;
        playPos.y += target.transform.localScale.y / 2;
        playPos.z = -0.5f;
        GetInstance().transform.localPosition = playPos;
        GetInstance().animPlayer?.SetTrigger("Attack");
    }
}
