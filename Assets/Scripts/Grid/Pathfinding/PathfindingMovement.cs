/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class PathfindingMovement : MonoBehaviour
{
    private float speed = 40f;

    private int currentPathIndex;
    private List<Vector3> pathVectorList;

    private Coroutine moveCoroutine;

    private void Start() { }
    private void Update() { }

    private void StopMoving() {
        pathVectorList = null;
        if(moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
    }

    public Vector3 GetPosition() {
        return transform.position;
    }

    public float GetSpeed()
    {
        return speed;
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    public void SetTargetPosition(Vector3 targetPosition) 
    {
        currentPathIndex = 0;
        pathVectorList = Pathfinding.Instance.FindPath(GetPosition(), targetPosition);

        if (pathVectorList != null && pathVectorList.Count > 1) {
            pathVectorList.RemoveAt(0);
        }

        if(moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(MoveToTargetPosition());
    }


    IEnumerator MoveToTargetPosition()
    {
        while (pathVectorList != null)
        {
            Vector3 targetPosition = pathVectorList[currentPathIndex];

            for (int i = 0; i < pathVectorList.Count - 1; i++) Debug.DrawLine(pathVectorList[i], pathVectorList[i + 1], Color.red, 3f);

            if (Vector3.Distance(transform.position, targetPosition) > 1f)
            {
                Vector3 moveDir = (targetPosition - transform.position).normalized;

                float distanceBefore = Vector3.Distance(transform.position, targetPosition);
                transform.position = Vector3.Lerp(transform.position, transform.position + moveDir, speed / 100f);
            }
            else
            {
                currentPathIndex++;
                if (currentPathIndex >= pathVectorList.Count)
                {
                    transform.position = targetPosition;
                    StopMoving();
                }
            }

            yield return new WaitForSeconds(1/60f);
        }
    }
}