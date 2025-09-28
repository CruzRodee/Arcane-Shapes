using System.Collections; // For coroutines (if needed elsewhere)
using UnityEngine;        // Unity engine base classes
using TMPro;
using UnityEngine.UI;
using System;              // TextMeshPro for advanced text rendering

/// <summary>
/// TextArchitect handles the progressive display of text (typewriter, instant, fade) for both UI and world-space TextMeshPro components.
/// It supports building, appending, and customizing text reveal effects for visual novel dialogue systems.
/// </summary>
public class TextArchitect
{
    // References to UI and world-space TextMeshPro components
    /// <summary>
    /// Reference to the UI TextMeshPro component (for canvas-based text).
    /// </summary>
    private TextMeshProUGUI tmpro_ui;
    /// <summary>
    /// Reference to the world-space TextMeshPro component (for 3D world text).
    /// </summary>
    private TextMeshPro tmpro_world;

    /// <summary>
    /// Returns the assigned TextMeshPro component (UI preferred, otherwise world-space).
    /// </summary>
    public TMP_Text tmpro => tmpro_ui != null ? tmpro_ui : tmpro_world;


    /// <summary>
    /// Gets the current text displayed by the TextMeshPro component.
    /// </summary>
    public string currentText => tmpro.text;

    /// <summary>
    /// The target text to build towards. Set privately, readable publicly.
    /// </summary>
    public string targetText { get; private set; } = "";

    /// <summary>
    /// Text that comes before the target text. Set privately, readable publicly.
    /// </summary>
    public string preText { get; private set; } = "";

    /// <summary>
    /// Stores the length of preText (used internally).
    /// </summary>
    private int preTextLength = 0;


    /// <summary>
    /// The full text to display (preText + targetText).
    /// </summary>
    private string fullTargetText => preText + targetText;

    /// <summary>
    /// Defines how text is revealed: instantly, typewriter effect, or fade-in.
    /// </summary>
    public enum BuildMethod { instant, typewriter, fade }

    /// <summary>
    /// The current build method for text reveal (default is typewriter).
    /// </summary>
    public BuildMethod buildMethod = BuildMethod.typewriter;

    /// <summary>
    /// Gets or sets the color of the text.
    /// </summary>
    public Color textColor
    {
        get { return tmpro.color; }
        set { tmpro.color = value; }
    }

    /// <summary>
    /// Gets or sets the speed multiplier for text reveal. Getting returns the effective speed (baseSpeed * speedMultiplier).
    /// </summary>
    public float speed
    {
        get { return baseSpeed * speedMultiplier; }
        set { speedMultiplier = value; }
    }

    /// <summary>
    /// The base speed for text reveal (constant).
    /// </summary>
    private const float baseSpeed = 1;

    /// <summary>
    /// The multiplier applied to baseSpeed (can be changed at runtime).
    /// </summary>
    private float speedMultiplier = 1;

    /// <summary>
    /// Determines how many characters to reveal per update cycle, based on speed.
    /// </summary>
    public int charactersPerCycle
    {
        get
        {
            // If speed <= 2, use 1x multiplier
            // If speed <= 2.5, use 2x multiplier
            // Otherwise, use 3x multiplier
            return speed <= 2f ? characterMultiplier :
                   speed <= 2.5f ? characterMultiplier * 2 :
                                    characterMultiplier * 3;
        }
    }

    /// <summary>
    /// The base multiplier for characters per cycle (can be adjusted).
    /// </summary>
    private int characterMultiplier = 1;


    /// <summary>
    /// If true, text should be revealed faster.
    /// </summary>
    public bool hurryUp = false;

    /// <summary>
    /// Constructor for UI TextMeshPro component.
    /// </summary>
    /// <param name="tmpro_ui">The UI TextMeshProUGUI component to use.</param>
    public TextArchitect(TextMeshProUGUI tmpro_ui)
    {
        this.tmpro_ui = tmpro_ui;
    }

    /// <summary>
    /// Constructor for world-space TextMeshPro component.
    /// </summary>
    /// <param name="tmpro_world">The world-space TextMeshPro component to use.</param>
    public TextArchitect(TextMeshPro tmpro_world)
    {
        this.tmpro_world = tmpro_world;
    }

    /// <summary>
    /// Starts building the given text from scratch (clears preText).
    /// </summary>
    /// <param name="text">The text to build.</param>
    /// <returns>The Coroutine handling the build process.</returns>
    public Coroutine Build(string text)
    {
        preText = "";
        targetText = text;

        Stop();

        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess;
    }

    /// <summary>
    /// Appends new text to the current text (sets preText to current text).
    /// </summary>
    /// <param name="text">The text to append.</param>
    /// <returns>The Coroutine handling the build process.</returns>
    public Coroutine Append(string text)
    {
        preText = tmpro.text;
        targetText = text;

        Stop();

        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess;
    }


    /// <summary>
    /// The currently running build Coroutine, or null if not building.
    /// </summary>
    private Coroutine buildProcess = null;

    /// <summary>
    /// Returns true if a build process is currently running.
    /// </summary>
    public bool isBuilding => buildProcess != null;

    /// <summary>
    /// Stops the current build process, if any.
    /// </summary>
    public void Stop()
    {
        if (!isBuilding) return;

        tmpro.StopCoroutine(buildProcess);
        buildProcess = null;
    }

    /// <summary>
    /// Coroutine that handles the text building process based on the selected build method.
    /// </summary>
    IEnumerator Building()
    {
        Prepare();
        switch (buildMethod)
        {
            case BuildMethod.typewriter:
                yield return Build_Typewriter();
                break;
            case BuildMethod.fade:
                yield return Build_Fade();
                break;
        }
        yield return null;
    }

    /// <summary>
    /// Called when the build process is complete. Resets buildProcess to null.
    /// </summary>
    private void OnComplete()
    {
        buildProcess = null;
    }
    /// <summary>
    /// Prepares the text and TMP component for the selected build method.
    /// </summary>
    private void Prepare()
    {
        switch (buildMethod)
        {
            case BuildMethod.instant:
                Prepare_Instant();
                break;
            case BuildMethod.typewriter:
                Prepare_Typewriter();
                break;
            case BuildMethod.fade:
                Prepare_Fade();
                break;
        }
    }

    /// <summary>
    /// Prepares the TMP component for instant text display.
    /// </summary>
    private void Prepare_Instant()
    {
        tmpro.color = tmpro.color;
        tmpro.text = fullTargetText;
        tmpro.ForceMeshUpdate();
        tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
    }
    /// <summary>
    /// Prepares the TMP component for typewriter text display.
    /// </summary>
    private void Prepare_Typewriter()
    {

    }
    /// <summary>
    /// Prepares the TMP component for fade-in text display.
    /// </summary>
    private void Prepare_Fade()
    {

    }

    /// <summary>
    /// Coroutine for typewriter-style text reveal.
    /// </summary>
    private IEnumerator Build_Typewriter()
    {
        yield return null;
    }

    /// <summary>
    /// Coroutine for fade-in text reveal.
    /// </summary>
    private IEnumerator Build_Fade()
    {
        yield return null;
    }
}
