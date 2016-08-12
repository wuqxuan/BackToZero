using UnityEngine;
using System.Collections;

public class Ground : MonoBehaviour
{
    /// <summary> Smooth RotateAround  </summary>
    public void RotateAroundWithDuration(Vector3 point, Vector3 axis, float rotateAmount, float rotateTime)
    {
        StartCoroutine(SmoothRotateAround(point, axis, rotateAmount, rotateTime));
    }

    // http://answers.unity3d.com/questions/29110/easing-a-rotation-of-rotate-around.html
    IEnumerator SmoothRotateAround(Vector3 point, Vector3 axis, float rotateAmount, float rotateTime)
    {
        float step = 0.0f; //non-smoothed
        float rate = 1.0f / rotateTime; //amount to increase non-smooth step by
        float smoothStep = 0.0f; //smooth step this time
        float lastStep = 0.0f; //smooth step last time
        while (step < 1.0f)
        { // until we're done
            step += Time.deltaTime * rate; //increase the step
            smoothStep = Mathf.SmoothStep(0.0f, 1.0f, step); //get the smooth step
            transform.RotateAround(point, axis, rotateAmount * (smoothStep - lastStep));
            // Debug.Log(step + " :smoothStep");
            lastStep = smoothStep; //store the smooth step
            yield return 0;
        }
    }
}
