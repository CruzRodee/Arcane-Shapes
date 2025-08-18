using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class LeftHandedMode : MonoBehaviour
{
    [Header("Assign your top-level Canvas (or leave blank to auto-find)")]
    [SerializeField] private Canvas targetCanvas;

    [Header("Optional: set a specific content root to mirror (e.g., SafeArea)")]
    [SerializeField] private RectTransform mirrorRoot;

    [Header("Settings")]
    [SerializeField] private bool includeInactive = true; // also fix inactive objects

    private bool isLeftHanded = false;

    void Awake()
    {
        if (!targetCanvas) targetCanvas = GetComponentInParent<Canvas>();
        EnsureMirrorRootExists();
    }

    public void ToggleLeftHandedMode() => ToggleLeftHandedMode(null);

    public void ToggleLeftHandedMode(bool? enable)
    {
        if (!targetCanvas) targetCanvas = GetComponentInParent<Canvas>();
        EnsureMirrorRootExists();

        isLeftHanded = enable ?? !isLeftHanded;

        // Mirror the wrapper, not the Canvas object
        var s = mirrorRoot.localScale;
        s.x = (isLeftHanded ? -1f : 1f) * Mathf.Abs(s.x);
        s.y = Mathf.Abs(s.y);
        s.z = Mathf.Abs(s.z);
        mirrorRoot.localScale = s;

        // Re-invert text so it stays readable & adjust alignment
        FixTextReadability(mirrorRoot);
    }

    private void EnsureMirrorRootExists()
    {
        if (!targetCanvas)
        {
            Debug.LogError("[LeftHandedMode] No Canvas found. Assign one or place this on/under a Canvas.");
            return;
        }

        if (!mirrorRoot)
        {
            if (targetCanvas.transform.childCount == 1)
            {
                var sole = targetCanvas.transform.GetChild(0) as RectTransform;
                if (sole) mirrorRoot = sole;
            }

            if (!mirrorRoot)
            {
                var go = new GameObject("LeftHanded_MirrorRoot", typeof(RectTransform));
                mirrorRoot = go.GetComponent<RectTransform>();
                mirrorRoot.SetParent(targetCanvas.transform, false);
                mirrorRoot.anchorMin = Vector2.zero;
                mirrorRoot.anchorMax = Vector2.one;
                mirrorRoot.pivot = new Vector2(0.5f, 0.5f);
                mirrorRoot.anchoredPosition = Vector2.zero;
                mirrorRoot.sizeDelta = Vector2.zero;
                mirrorRoot.localScale = Vector3.one;

                var toMove = new List<Transform>();
                for (int i = 0; i < targetCanvas.transform.childCount; i++)
                {
                    var child = targetCanvas.transform.GetChild(i);
                    if (child == mirrorRoot) continue;
                    toMove.Add(child);
                }
                foreach (var t in toMove)
                    t.SetParent(mirrorRoot, false);
            }
        }
    }

    private void FixTextReadability(Transform root)
    {
        // Legacy UGUI Text
        foreach (var t in root.GetComponentsInChildren<Text>(includeInactive))
        {
            SetXScale(t.rectTransform, isLeftHanded ? -1 : 1);
            FlipLegacyAlignment(t);
        }

        // TextMeshPro UGUI
        foreach (var tmp in root.GetComponentsInChildren<TextMeshProUGUI>(includeInactive))
        {
            SetXScale(tmp.rectTransform, isLeftHanded ? -1 : 1);
            FlipTMPAlignment(tmp);
        }
    }

    private static void SetXScale(RectTransform rt, int dir)
    {
        var ls = rt.localScale;
        ls.x = dir * Mathf.Abs(ls.x);
        rt.localScale = ls;
    }

    private void FlipLegacyAlignment(Text text)
    {
        if (text.alignment == TextAnchor.MiddleLeft)
            text.alignment = TextAnchor.MiddleRight;
        else if (text.alignment == TextAnchor.MiddleRight)
            text.alignment = TextAnchor.MiddleLeft;

        else if (text.alignment == TextAnchor.UpperLeft)
            text.alignment = TextAnchor.UpperRight;
        else if (text.alignment == TextAnchor.UpperRight)
            text.alignment = TextAnchor.UpperLeft;

        else if (text.alignment == TextAnchor.LowerLeft)
            text.alignment = TextAnchor.LowerRight;
        else if (text.alignment == TextAnchor.LowerRight)
            text.alignment = TextAnchor.LowerLeft;
        // Center anchors stay as is
    }

    private void FlipTMPAlignment(TextMeshProUGUI tmp)
    {
        var a = tmp.alignment;

        // Horizontal-only swap
        if ((a & TextAlignmentOptions.Left) != 0 && (a & TextAlignmentOptions.Center) == 0)
            tmp.alignment = (a & ~TextAlignmentOptions.Left) | TextAlignmentOptions.Right;

        else if ((a & TextAlignmentOptions.Right) != 0 && (a & TextAlignmentOptions.Center) == 0)
            tmp.alignment = (a & ~TextAlignmentOptions.Right) | TextAlignmentOptions.Left;

        // Center alignments stay untouched
    }

#if UNITY_EDITOR
    [ContextMenu("Force Left-Handed ON")]
    private void ForceOn() => ToggleLeftHandedMode(true);

    [ContextMenu("Force Left-Handed OFF")]
    private void ForceOff() => ToggleLeftHandedMode(false);
#endif
}
