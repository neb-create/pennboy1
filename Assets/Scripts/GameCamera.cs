using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour
{
    public enum CameraState { Rhythm, BulletHell, Editor }

    [Header("Camera Presets")]
    public Transform rhythmGamePreset;
    public Transform bulletHellPreset;
    public Transform editorPreset;

    [Header("Transition Settings")]
    public float transitionDuration = 0.75f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Camera Settings")]
    public float editorOrthoSize = 7f;
    public float gameplayFOV = 60f;

    private Coroutine currentTransition;

    public CameraState CurrentState { get; private set; } = CameraState.Rhythm;
    public bool IsEditorState => CurrentState == CameraState.Editor;

    private void Start()
    {
        if (rhythmGamePreset != null)
        {
            transform.position = rhythmGamePreset.position;
            transform.rotation = rhythmGamePreset.rotation;

            var cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographic = false;
                cam.fieldOfView = gameplayFOV;
            }
        }
    }

    public void TransitionToRhythmGame()
    {
        CurrentState = CameraState.Rhythm;
        StartTransition(rhythmGamePreset, false);
    }

    public void TransitionToBulletHell()
    {
        CurrentState = CameraState.BulletHell;
        StartTransition(bulletHellPreset, false);
    }

    public void TransitionToEditor()
    {
        CurrentState = CameraState.Editor;
        StartTransition(editorPreset, true);
    }

    private void StartTransition(Transform target, bool toOrtho)
    {
        if (target == null)
        {
            Debug.LogWarning("Target preset not assigned!");
            return;
        }

        if (currentTransition != null)
            StopCoroutine(currentTransition);

        currentTransition = StartCoroutine(TransitionRoutine(target, toOrtho));
    }

    private IEnumerator TransitionRoutine(Transform target, bool toOrtho)
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null) yield break;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 endPos = target.position;
        Quaternion endRot = target.rotation;

        float ProjectedDistance(Vector3 from, Vector3 to, Vector3 forward)
        {
            float d = Vector3.Dot((to - from), forward);
            return Mathf.Abs(d) > 0.0001f ? Mathf.Abs(d) : 1f;
        }

        float startForwardDist = ProjectedDistance(startPos, endPos, transform.forward);

        bool startingOrtho = cam.orthographic;
        float startFOV = cam.fieldOfView;
        float startSize = cam.orthographicSize;

        if (!toOrtho && startingOrtho)
        {
            float matchingFOV = 2f * Mathf.Atan(startSize / startForwardDist) * Mathf.Rad2Deg;
            cam.fieldOfView = matchingFOV;
            cam.orthographic = false;
            startFOV = cam.fieldOfView;
            startingOrtho = false;
        }

        float targetMatchingFOV = startFOV;

        if (toOrtho && !startingOrtho)
        {
            float startHorizontalDist = Vector3.Distance(
                new Vector3(startPos.x, 0f, startPos.z),
                new Vector3(endPos.x, 0f, endPos.z)
            );

            float matchingOrthoSize = Mathf.Tan(startFOV * 0.5f * Mathf.Deg2Rad) * startHorizontalDist;

            startSize = matchingOrthoSize;
            startingOrtho = true;
            cam.orthographic = true;
            cam.orthographicSize = matchingOrthoSize;
        }

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = transitionCurve.Evaluate(elapsed / transitionDuration);

            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            if (toOrtho)
            {
                if (!startingOrtho)
                {
                    cam.fieldOfView = Mathf.Lerp(startFOV, targetMatchingFOV, t);
                }
                else
                {
                    cam.orthographicSize = Mathf.Lerp(startSize, editorOrthoSize, t);
                }
            }
            else
            {
                cam.fieldOfView = Mathf.Lerp(startFOV, gameplayFOV, t);
            }

            yield return null;
        }

        transform.position = endPos;
        transform.rotation = endRot;

        if (toOrtho)
        {
            float finalForwardDist = ProjectedDistance(transform.position, endPos, transform.forward);

            if (!cam.orthographic)
            {
                float equivalentSize = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * finalForwardDist;
                cam.orthographic = true;
                cam.orthographicSize = equivalentSize;
            }

            cam.orthographicSize = editorOrthoSize;
        }
        else
        {
            cam.orthographic = false;
            cam.fieldOfView = gameplayFOV;
        }

        currentTransition = null;
    }
}
