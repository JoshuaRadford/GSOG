using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public static class WaitExtension {
    public static Coroutine Wait(this MonoBehaviour mono, float delay, UnityAction action) {
        return mono.StartCoroutine(ExecuteAction(delay, action));
    }

    private static IEnumerator ExecuteAction(float delay, UnityAction action) {
        yield return new WaitForSecondsRealtime(delay);
        action.Invoke();
        yield break;
    }
}