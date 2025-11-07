using UnityEngine;

public class GameCamera : MonoBehaviour
{
    [Header("Camera Presets")]
    public Transform rhythmGamePreset;
    public Transform bulletHellPreset;

    [Header("Transition Settings")]
    public float transitionDuration = 0.75f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine currentTransition;

    public void Start()
    {
        if (rhythmGamePreset != null)
        {
            transform.position = rhythmGamePreset.position;
            transform.rotation = rhythmGamePreset.rotation;
        }
    }

    public void TransitionToRhythmGame()
    {
        StartTransition(rhythmGamePreset);
    }

    public void TransitionToBulletHell()
    {
        StartTransition(bulletHellPreset);
    }

    private void StartTransition(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("Target preset not assigned!");
            return;
        }

        if (currentTransition != null)
            StopCoroutine(currentTransition);

        currentTransition = StartCoroutine(TransitionRoutine(target));
    }

    private System.Collections.IEnumerator TransitionRoutine(Transform target)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = transitionCurve.Evaluate(elapsed / transitionDuration);

            transform.position = Vector3.Lerp(startPos, target.position, t);
            transform.rotation = Quaternion.Slerp(startRot, target.rotation, t);

            yield return null;
        }

        transform.position = target.position;
        transform.rotation = target.rotation;
        currentTransition = null;
    }
}
