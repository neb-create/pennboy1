using UnityEngine;
using System.Collections.Generic;

public class NoteEditor : MonoBehaviour
{
    [Header("Selection")]
    public bool selected = false;

    [Header("Visual Settings")]
    public static Material highlightMaterial;
    public Vector3 duplicateScale = Vector3.one * 1.05f;
    public Vector3 duplicateOffset = new Vector3(0f, -0.1f, 0f);

    public Note3D note3D;

    private List<GameObject> duplicates = new List<GameObject>();
    private bool lastSelectedState = false;

    void Update()
    {
        if (selected != lastSelectedState)
        {
            lastSelectedState = selected;
            if (selected) CreateHighlight();
            else ClearHighlight();
        }
    }
    public void OnSelect()
    {
        selected = true;
        CreateHighlight();
    }

    public void OnDeselect()
    {
        selected = false;
        ClearHighlight();
    }

    void CreateHighlight()
    {
        ClearHighlight();

        if (highlightMaterial == null)
        {
            highlightMaterial = Resources.Load<Material>("HighlightMat");
        }

        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }
        foreach (Transform child in children)
        {
            GameObject dup = Instantiate(child.gameObject, child.position, child.rotation, transform);
            dup.transform.localScale = Vector3.Scale(child.localScale, duplicateScale);
            dup.transform.position += duplicateOffset;

            // apply highlight material
            var renderers = dup.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
                r.material = highlightMaterial;

            duplicates.Add(dup);
        }
    }

    void ClearHighlight()
    {
        foreach (var dup in duplicates)
        {
            if (dup != null)
                DestroyImmediate(dup);
        }
        duplicates.Clear();
    }
}
