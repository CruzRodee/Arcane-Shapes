using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// TASK 3 & 5: Required for randomizing and dynamic button actions.
using System.Linq;
using UnityEngine.Events;

public class GameBehaviour : MonoBehaviour
{
    #region Constants
    private const int UNUSED = -1;

    private const float TRANSITIONTIME = 0.4f;
    private const float FILLTIMEAPROX = 1.5f;
    private const float STARTDELAY = 3.4f;
    private float ENDDELAY = 4.0f;
    private const float TRANSITIONDELAY = 0.5f;

    // OPTIMIZATION: Cached strings to avoid allocations
    private const string correctShapePropmt = "Tama na ba ang hugis na pinili?";
    private const string castBtnText1 = "Done";
    private const string castBtnText2 = "Erase";
    private const string undoBtnText1 = "Undo";
    private const string undoBtnText2 = "Cast";
    private const string wrongShapeMsg = "Ang hugis na pinili ay mali. Subukan ulit.";
    private const string invalidFormulaMsg = "Hindi wasto ang ibinigay na formula.";
    private const string mismatchedAnswerMsg = "Hindi tugma sa formula ang ibinigay na sagot.";
    private const string homeConfirmMsg = "Nais mo bang bumalik sa labas na pagpipilian?";
    private const string progressNotSavedMsg = "Hindi masa-Save ang progreso.";
    private const string correctChoiceMsg = "Tama ba ang napili:";

    //----------------------------------------------
    private const float DIALOGUESLIDETIME = 0.25f;
    private const float OCRSLIDETIME = 0.35f;

    [HideInInspector] public PhaseManagerClass phaseManager = new PhaseManagerClass();
    [HideInInspector] public FormulaSelectionManagerClass formulaSelectionManager = new FormulaSelectionManagerClass();

    #region Animation Timing Configuration
    [Header("Animation Timing")]
    [Tooltip("Extra delay after shape fills (before returning to classroom)")]
    [Range(0f, 5f)]
    public float extraFillDelay = 2.0f;

  
    #endregion

    // NEW: Phase 1 Constants
    private const int POINTS_PER_CORRECT = 100;
    private const float GUIDE_ALPHA = 0.3f;



    #endregion

    #region Enums
    public enum SHAPES
    {
        NONE,
        TRIANGLE,
        SQUARE,
        RECTANGLE,
        CIRCLE,
        SEMI_CIRCLE,
    }
    #endregion

    #region Player Statistics and Level System
    /// <summary>
    /// Tracks player performance data for analytics
    /// </summary>
    [System.Serializable]
    public class PlayerStats
    {
        public int level;
        public string levelName;
        public SHAPES shape;
        public int correctAttempts;
        public int wrongAttempts;
        public int totalAttempts;
        public float completionTime;
        public bool levelCompleted;

        public PlayerStats(int level, string levelName, SHAPES shape)
        {
            this.level = level;
            this.levelName = levelName;
            this.shape = shape;
            this.correctAttempts = 0;
            this.wrongAttempts = 0;
            this.totalAttempts = 0;
            this.completionTime = 0f;
            this.levelCompleted = false;
        }

        public void AddCorrectAttempt()
        {
            correctAttempts++;
            totalAttempts++;
        }

        public void AddWrongAttempt()
        {
            wrongAttempts++;
            totalAttempts++;
        }

        public float GetAccuracyPercentage()
        {
            if (totalAttempts == 0) return 0f;
            return (float)correctAttempts / totalAttempts * 100f;
        }
    }

    /// <summary>
    /// Shape specifications for each problem variant
    /// </summary>
    [System.Serializable]
    public class ShapeSpecs
    {
        public float primaryMeasure;   // Width, diameter, base, side, etc.
        public float secondaryMeasure; // Height, length, etc. (0 if not needed)

        public ShapeSpecs(float primary, float secondary = 0f)
        {
            this.primaryMeasure = primary;
            this.secondaryMeasure = secondary;
        }
    }

    /// <summary>
    /// Level configuration with shape focus and variants
    /// </summary>
    [System.Serializable]
    public class LevelInfo
    {
        public string levelName;
        public SHAPES focusShape;
        public ShapeSpecs[] shapeVariants;

        public LevelInfo(string name, SHAPES shape, ShapeSpecs[] variants)
        {
            this.levelName = name;
            this.focusShape = shape;
            this.shapeVariants = variants;
        }
    }
    #endregion



    #region Modular System Classes
    // Add these new classes to the #region Modular System Classes section in GameBehaviour

    [System.Serializable]
    public class PhaseManagerClass
    {
        [HideInInspector] public GameBehaviour main;

        public enum GamePhase
        {
            Measurement,
            FormulaSelection,
            OCRInput,
            Complete
        }

        // Public properties for cross-class access
        public GamePhase CurrentPhase = GamePhase.Measurement;
        public GamePhase PreviousPhase = GamePhase.Measurement;

        public void Initialize(GameBehaviour gameMain)
        {
            main = gameMain;
            CurrentPhase = GamePhase.Measurement;
            PreviousPhase = GamePhase.Measurement;
            Debug.Log("Phase Manager: Initialized to Measurement phase");
        }

        public bool IsInOCRPhase()
        {
            return CurrentPhase == GamePhase.OCRInput;
        }



        public void TransitionToPhase(GamePhase newPhase)
        {
            PreviousPhase = CurrentPhase;
            CurrentPhase = newPhase;

            Debug.Log($"Phase Manager: Transitioning from {PreviousPhase} to {CurrentPhase}");

            // STEP 1: Deactivate the UI and logic of the PREVIOUS phase.
            switch (PreviousPhase)
            {
                case GamePhase.Measurement:
                    // This phase ends when all measurements are done. The UI (guides) is
                    // already cleaned up by ProceedToFormulaSelection().
                    break;

                case GamePhase.FormulaSelection:
                    // This is the critical missing piece. Hide the formula buttons.
                    main.formulaSelectionManager.SetFormulaSelectionActive(false);
                    break;

                case GamePhase.OCRInput:
                    // If we were ever to transition *from* the OCR phase (e.g., via a back button)
                    // we would deactivate it here.
                    main.ocrManager.Deactivate();
                    break;
            }

            // STEP 2: Activate the UI and logic for the NEW phase.
            switch (CurrentPhase)
            {
                case GamePhase.Measurement:
                    main.gameStateManager.SetMeasurementPhase(true);
                    main.uiManager.UpdateUIForCurrentPhase();
                    break;

                case GamePhase.FormulaSelection:
                    main.formulaSelectionManager.SetFormulaSelectionActive(true);
                    main.uiManager.UpdateUIForCurrentPhase();
                    break;

                case GamePhase.OCRInput:
                    main.gameStateManager.SetOCRPhase(true);
                    // --- THE FIX IS HERE ---
                    // 1. Explicitly tell the UI Manager to set up the screen for the OCR phase.
                    //    This activates the OCR board and places it at its off-screen start position,
                    //    preventing the race condition caused by relying on the Update() loop.
                    main.uiManager.UpdateUIForCurrentPhase();

                    // 2. Now that the UI is correctly prepared, activate the OCR Manager to
                    //    begin the animation.
                    main.ocrManager.Activate();
                    break;

                case GamePhase.Complete:
                    // No UI to activate for the 'Complete' state, it just triggers the next level.
                    break;
            }

            Debug.Log($"Phase Manager: Phase transition complete to {CurrentPhase}");
        }

        public bool IsInMeasurementPhase()
        {
            return CurrentPhase == GamePhase.Measurement;
        }

        public bool IsInFormulaSelectionPhase()
        {
            return CurrentPhase == GamePhase.FormulaSelection;
        }

        public bool IsComplete()
        {
            return CurrentPhase == GamePhase.Complete;
        }

        public void OnMeasurementCompleted()
        {
            Debug.Log("Phase Manager: All measurements completed, transitioning to Formula Selection");
            TransitionToPhase(GamePhase.FormulaSelection);
        }

        public void OnFormulaSelectionCompleted()
        {
            TransitionToPhase(GamePhase.OCRInput);
            // This is the correct logic: Tell the LevelManager that the CURRENT PROBLEM is done.
            // The LevelManager will then decide if it's time to start the next problem or the next level.
            // main.levelManager.OnProblemCompleted();
        }

        public void OnOCRInputCompleted()
        {
            Debug.Log("Phase Manager: OCR Input complete. Advancing to the next problem.");
            TransitionToPhase(GamePhase.Complete);

            // This is now the correct place to end the current problem
            main.levelManager.OnProblemCompleted();
        }


        public void RestartPhases()
        {
            Debug.Log("Phase Manager: Restarting phases");
            TransitionToPhase(GamePhase.Measurement);
        }
    }

    [System.Serializable]
    public class FormulaSelectionManagerClass
    {
        [HideInInspector] public GameBehaviour main;

        // Public properties - NOW USING SHAPES INSTEAD OF FormulaModifier
        public bool IsFormulaSelectionActive = false;
        public SHAPES SelectedFormula = SHAPES.NONE;
        public SHAPES CorrectFormula = SHAPES.NONE;

        // NEW: Track if choices have been set up for current problem
        private bool choicesInitialized = false;

        // Button references - will be found in Initialize
        private Button choice1Button; // π
        private Button choice2Button; // π÷2
        private Button choice3Button; // ÷2
        private Button choice4Button; // wala (none)

        // TASK 3: A list for easy randomization
        private List<Button> allChoiceButtons = new List<Button>();

        public void Initialize(GameBehaviour gameMain)
        {
            main = gameMain;
            IsFormulaSelectionActive = false;
            SelectedFormula = SHAPES.NONE;
            CorrectFormula = SHAPES.NONE;
            choicesInitialized = false;

            GameObject canvas = GameObject.Find("GameLevelSceneCanvas");
            if (canvas != null)
            {
                choice1Button = FindButtonInCanvas(canvas, "Choice-1");
                choice2Button = FindButtonInCanvas(canvas, "Choice-2");
                choice3Button = FindButtonInCanvas(canvas, "Choice-3");
                choice4Button = FindButtonInCanvas(canvas, "Choice-4");

                // TASK 3: Populate the button list for randomization
                if (choice1Button != null) allChoiceButtons.Add(choice1Button);
                if (choice2Button != null) allChoiceButtons.Add(choice2Button);
                if (choice3Button != null) allChoiceButtons.Add(choice3Button);
                if (choice4Button != null) allChoiceButtons.Add(choice4Button);

                Debug.Log($"Formula Selection Manager: Found buttons - Choice-1: {choice1Button != null}, Choice-2: {choice2Button != null}, Choice-3: {choice3Button != null}, Choice-4: {choice4Button != null}");
            }
            else
            {
                Debug.LogError("Formula Selection Manager: GameLevelSceneCanvas not found!");
            }

            SetFormulaSelectionActive(false);
        }

        private Button FindButtonInCanvas(GameObject canvas, string buttonName)
        {
            Transform buttonTransform = canvas.transform.Find(buttonName);
            if (buttonTransform == null)
            {
                buttonTransform = FindChildRecursive(canvas.transform, buttonName);
            }
            return buttonTransform?.GetComponent<Button>();
        }

        private Transform FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform result = FindChildRecursive(child, name);
                if (result != null) return result;
            }
            return null;
        }

        private SHAPES GetCorrectFormulaForShape(GameBehaviour.SHAPES shape)
        {
            // The correct formula is just the shape itself!
            // Square needs S × S, Rectangle needs L × W, etc.
            return shape;
        }

        public void SetFormulaSelectionActive(bool active)
        {
            IsFormulaSelectionActive = active;

            if (active)
            {
                // Only setup choices if they haven't been initialized yet
                if (!choicesInitialized)
                {
                    CorrectFormula = GetCorrectFormulaForShape(main.currentShape);
                    Debug.Log($"Formula Selection Manager: Activated for shape {main.currentShape}, correct formula: {CorrectFormula}");
                    SetupAndShowDynamicChoices();
                    choicesInitialized = true;
                }
                else
                {
                    // Just show the buttons that were already set up
                    foreach (var btn in allChoiceButtons)
                    {
                        if (btn != null && btn.gameObject.activeSelf)
                        {
                            // Keep them visible
                        }
                    }
                }
            }
            else
            {
                foreach (var btn in allChoiceButtons)
                {
                    if (btn != null) btn.gameObject.SetActive(false);
                }
                // Reset initialization flag when hiding
                choicesInitialized = false;
                Debug.Log("Formula Selection Manager: Deactivated");
            }
        }

        /// <summary>
        /// NEW: Get all possible distractors (all shapes except the correct one)
        /// </summary>
        private List<SHAPES> GetDistractorPoolForShape(SHAPES shape)
        {
            // Distractor pool = ALL shapes EXCEPT the correct one
            List<SHAPES> allShapes = new List<SHAPES>
        {
            SHAPES.SQUARE,
            SHAPES.RECTANGLE,
            SHAPES.CIRCLE,
            SHAPES.SEMI_CIRCLE,
            SHAPES.TRIANGLE
        };

            // Remove the correct answer from the pool
            allShapes.Remove(shape);

            return allShapes; // Returns 4 wrong formulas
        }

        /// <summary>
        /// NEW: Determine number of choices based on difficulty
        /// Difficulty 1-2: 2 choices
        /// Difficulty 3: 3 choices
        /// Difficulty 4: 4 choices
        /// </summary>
        private int GetNumberOfChoicesForDifficulty()
        {
            int difficulty = main.GetDifficulty();

            if (difficulty <= 2) return 2;      // Difficulty 1-2: 2 choices
            else if (difficulty == 3) return 3; // Difficulty 3: 3 choices
            else return 4;                       // Difficulty 4: 4 choices
        }

        /// <summary>
        /// NEW: Setup and show dynamic choices based on difficulty and randomization
        /// </summary>
        private void SetupAndShowDynamicChoices()
        {
            // Step 1: Get the correct formula (which is just the current shape)
            SHAPES correctFormula = GetCorrectFormulaForShape(main.currentShape);
            CorrectFormula = correctFormula;

            // Step 2: Get all possible distractors (all other shapes)
            List<SHAPES> distractorPool = GetDistractorPoolForShape(main.currentShape);

            // Step 3: Determine how many choices based on difficulty
            int numberOfChoices = GetNumberOfChoicesForDifficulty();
            int numberOfDistractors = numberOfChoices - 1; // Leave 1 slot for correct answer

            // Step 4: Randomly select distractors
            System.Random rng = new System.Random();
            List<SHAPES> selectedDistractors = distractorPool
                .OrderBy(x => rng.Next())
                .Take(numberOfDistractors)
                .ToList();

            // Step 5: Combine correct answer + distractors
            List<SHAPES> allChoices = new List<SHAPES>(selectedDistractors);
            allChoices.Add(correctFormula);

            // Step 6: Shuffle so correct answer isn't always in same position
            List<SHAPES> shuffledChoices = allChoices
                .OrderBy(x => rng.Next())
                .ToList();

            // Step 7: Assign to buttons
            for (int i = 0; i < allChoiceButtons.Count; i++)
            {
                Button currentButton = allChoiceButtons[i];
                if (currentButton == null) continue;

                if (i < shuffledChoices.Count)
                {
                    SHAPES assignedShape = shuffledChoices[i];

                    Text buttonText = currentButton.GetComponentInChildren<Text>();
                    if (buttonText != null)
                    {
                        buttonText.text = GetFormulaDisplayText(assignedShape);
                    }

                    currentButton.onClick.RemoveAllListeners();
                    currentButton.onClick.AddListener(() => OnFormulaSelected(assignedShape));

                    var colors = currentButton.colors;
                    colors.normalColor = Color.white;
                    currentButton.colors = colors;

                    currentButton.gameObject.SetActive(true);
                }
                else
                {
                    // Hide unused buttons based on difficulty
                    currentButton.gameObject.SetActive(false);
                }
            }

            Debug.Log($"Formula Selection: Showing {shuffledChoices.Count} choices for {main.currentShape} at difficulty {main.GetDifficulty()}");
        }

        public void OnFormulaSelected(SHAPES selectedShape)
        {
            SelectedFormula = selectedShape;
            Debug.Log($"Formula Selection Manager: Player selected {selectedShape}, correct is {CorrectFormula}");

            bool isCorrect = (selectedShape == CorrectFormula);

            if (isCorrect)
            {
                Debug.Log("Formula Selection Manager: CORRECT formula selected!");
                main.gameStateManager.AddScore(200);
                OnCorrectFormulaSelected();
            }
            else
            {
                Debug.Log("Formula Selection Manager: INCORRECT formula selected!");
                main.gameStateManager.ReduceLives(1);
                OnIncorrectFormulaSelected();
            }
        }

        private void OnCorrectFormulaSelected()
        {
            main.PlayCorrectSound();
            main.ShowMeasurementFeedback("Correct formula! Well done!");
            main.phaseManager.OnFormulaSelectionCompleted();
        }

        private void OnIncorrectFormulaSelected()
        {
            main.PlayIncorrectSound();
            main.ShowMeasurementFeedback($"Incorrect! Try again. Lives: {main.gameStateManager.CurrentLives}");
        }

        /// <summary>
        /// NEW: Get textbook-style formula display text
        /// </summary>
        public string GetFormulaDisplayText(SHAPES shape)
        {
            switch (shape)
            {
                case SHAPES.SQUARE:
                    return "S × S";

                case SHAPES.RECTANGLE:
                    return "L × W";

                case SHAPES.CIRCLE:
                    return "R × R × π";

                case SHAPES.SEMI_CIRCLE:
                    return "R × R × π ÷ 2";

                case SHAPES.TRIANGLE:
                    return "B × H ÷ 2";

                default:
                    return "???";
            }
        }
    }
    [System.Serializable]
    public class UIManagerClass
    {
        [HideInInspector] public GameBehaviour main;

        public bool ShouldShowUndoButton = false;
        public bool ShouldShowEraseButton = true;
        public bool IsInMeasurementPhase = false;

        private PhaseManagerClass.GamePhase lastSetupPhase = PhaseManagerClass.GamePhase.Measurement;
        public void Initialize(GameBehaviour gameMain)
        {
            main = gameMain;
            Debug.Log("UI Manager: Initialized");

            // TASK 5: Ensure confirmation UI is hidden on start.
            if (main.textPlayerConfirmation != null) main.textPlayerConfirmation.gameObject.SetActive(false);
            if (main.btnConfirmYes != null) main.btnConfirmYes.gameObject.SetActive(false);
            if (main.btnConfirmNo != null) main.btnConfirmNo.gameObject.SetActive(false);
        }

        public void UpdateUIBasedOnPhase()
        {
            if (main.undoBtnImg != null) main.undoBtnImg.gameObject.SetActive(false);
            if (main.undoText != null) main.undoText.gameObject.SetActive(false);

            if (IsInMeasurementPhase)
            {
                ShouldShowEraseButton = false;
                if (main.btnMeasure != null) main.btnMeasure.gameObject.SetActive(false);
                if (main.textFinish != null) main.textFinish.gameObject.SetActive(false);
            }
            else
            {
                ShouldShowEraseButton = true;
            }
        }

        private void ShowOCRPhaseUI()
        {
            // 1. Ensure other phase's primary UI is off
            if (main.lineSnapper != null) main.lineSnapper.gameObject.SetActive(false);
            if (main.formulaSelectionManager != null) main.formulaSelectionManager.SetFormulaSelectionActive(false);

            // 2. Activate containers first
            if (main.pDialogue != null) main.pDialogue.SetActive(true);
            if (main.pDiaButtons != null) main.pDiaButtons.SetActive(true);

            // 3. Set button states
            if (main.btnMeasure != null) main.btnMeasure.gameObject.SetActive(false);
            if (main.backspaceButton != null) main.backspaceButton.SetActive(false); // Shown later
            if (main.textFinish != null) main.textFinish.text = "Cast";
            if (main.calcBtnObj != null) main.calcBtnObj.SetActive(true);

            // 4. CRITICAL: Position rtDiaButtons at rightButtonPos LOCALLY
            if (main.rtDiaButtons != null)
            {
                main.rtDiaButtons.anchoredPosition = GameBehaviour.rightButtonPos;
            }

            // 5. CRITICAL: Ensure rtDialogue is at the correct starting position
            if (main.rtDialogue != null)
            {
                main.rtDialogue.anchoredPosition = new Vector2(600f, -151.46f);
            }

            // 6. Position OCR board at starting position
            if (main.ocrInput != null && main.ocrStartTransform != null)
            {
                main.ocrInput.SetActive(true);
                main.ocrInput.transform.position = main.ocrStartTransform.position;
            }

            Debug.Log("========== OCR PHASE UI SETUP ==========");
            Debug.Log($"rtDialogue position: {main.rtDialogue?.anchoredPosition}");
            Debug.Log($"rtDiaButtons position: {main.rtDiaButtons?.anchoredPosition}");
            Debug.Log($"ocrInput active: {main.ocrInput?.activeSelf}, position: {main.ocrInput?.transform.position}");
            Debug.Log($"ocrStartTransform position: {main.ocrStartTransform?.position}");
            Debug.Log($"ocrEndTransform position: {main.ocrEndTransform?.position}");
            Debug.Log($"pDialogue active: {main.pDialogue?.activeSelf}");
            Debug.Log($"pDiaButtons active: {main.pDiaButtons?.activeSelf}");
            Debug.Log($"formulaDisplay active: {main.formulaDisplay?.activeSelf}");
            Debug.Log($"calcBtnObj active: {main.calcBtnObj?.activeSelf}");
            Debug.Log("======================================");
        }

        public void UpdateUIForCurrentPhase()
        {
            if (main.phaseManager != null)
            {
                // NEW: Only setup UI when phase CHANGES, not every frame
                if (main.phaseManager.CurrentPhase != lastSetupPhase)
                {
                    Debug.Log($"UI Manager: Phase changed from {lastSetupPhase} to {main.phaseManager.CurrentPhase}, setting up UI");
                    lastSetupPhase = main.phaseManager.CurrentPhase;

                    if (main.phaseManager.IsInMeasurementPhase()) ShowMeasurementPhaseUI();
                    else if (main.phaseManager.IsInFormulaSelectionPhase()) ShowFormulaSelectionPhaseUI();
                    else if (main.phaseManager.IsInOCRPhase()) ShowOCRPhaseUI();
                    else if (main.phaseManager.IsComplete()) ShowCompletionPhaseUI();
                }
            }
            else
            {
                UpdateUIBasedOnPhase(); // Fallback for old logic
            }
        }

        private void ShowMeasurementPhaseUI()
        {
            if (main.formulaSelectionManager != null) main.formulaSelectionManager.SetFormulaSelectionActive(false);
            if (main.pDiaButtons != null) main.pDiaButtons.SetActive(true);

            if (main.lineSnapper != null)
            {
                main.lineSnapper.gameObject.SetActive(true);
                main.lineSnapper.enabled = true;
            }

            if (main.undoBtnImg != null) main.undoBtnImg.gameObject.SetActive(false);
            if (main.undoText != null) main.undoText.gameObject.SetActive(false);
            if (main.btnMeasure != null) main.btnMeasure.gameObject.SetActive(false);
            if (main.textFinish != null) main.textFinish.gameObject.SetActive(false);
        }

        private void ShowFormulaSelectionPhaseUI()
        {
            if (main.lineSnapper != null) main.lineSnapper.gameObject.SetActive(false);
            if (main.pDiaButtons != null) main.pDiaButtons.SetActive(false);
            if (main.formulaSelectionManager != null) main.formulaSelectionManager.SetFormulaSelectionActive(true);
            if (main.undoBtnImg != null) main.undoBtnImg.gameObject.SetActive(false);
            if (main.undoText != null) main.undoText.gameObject.SetActive(false);
        }

        private void ShowCompletionPhaseUI()
        {
            if (main.lineSnapper != null) main.lineSnapper.gameObject.SetActive(false);
            if (main.pDiaButtons != null) main.pDiaButtons.SetActive(false);
            if (main.formulaSelectionManager != null) main.formulaSelectionManager.SetFormulaSelectionActive(false);
            if (main.undoBtnImg != null) main.undoBtnImg.gameObject.SetActive(false);
            if (main.undoText != null) main.undoText.gameObject.SetActive(false);
        }

        // TASK 5: New reusable method to show the confirmation prompt.
        /// <summary>
        /// Displays a confirmation prompt with a custom message and dynamic actions for Yes/No buttons.
        /// </summary>
        /// <param name="message">The question to ask the player.</param>
        /// <param name="onYesAction">The code to run when the "Yes" button is clicked.</param>
        /// <param name="onNoAction">The code to run when the "No" button is clicked.</param>
        public void ShowConfirmationDialog(string message, UnityAction onYesAction, UnityAction onNoAction)
        {
            // Set the confirmation text.
            if (main.textPlayerConfirmation != null)
            {
                main.textPlayerConfirmation.text = message;
                main.textPlayerConfirmation.gameObject.SetActive(true);
            }

            // Configure and show the "Yes" button.
            if (main.btnConfirmYes != null)
            {
                main.btnConfirmYes.onClick.RemoveAllListeners(); // Clear old actions.
                main.btnConfirmYes.onClick.AddListener(() => { HideConfirmationDialog(); onYesAction(); });
                main.btnConfirmYes.gameObject.SetActive(true);
            }

            // Configure and show the "No" button.
            if (main.btnConfirmNo != null)
            {
                main.btnConfirmNo.onClick.RemoveAllListeners(); // Clear old actions.
                main.btnConfirmNo.onClick.AddListener(() => { HideConfirmationDialog(); onNoAction(); });
                main.btnConfirmNo.gameObject.SetActive(true);
            }
        }

        // TASK 5: Helper method to hide the confirmation UI.
        private void HideConfirmationDialog()
        {
            if (main.textPlayerConfirmation != null) main.textPlayerConfirmation.gameObject.SetActive(false);
            if (main.btnConfirmYes != null) main.btnConfirmYes.gameObject.SetActive(false);
            if (main.btnConfirmNo != null) main.btnConfirmNo.gameObject.SetActive(false);
        }

        public void onUndo() { return; }

        public void onCast()
        {
            if (IsInMeasurementPhase || (main.phaseManager != null && main.phaseManager.IsInMeasurementPhase()))
            {
                return;
            }
            if (!main.gameStateManager.IsDoneMeasuring)
            {
                if (main.lineSnapper == null || main.lineSnapper.lineCount != main.lineSnapper.GetMaxLinesForShape()) return;
                //main.DoneMeasure();
            }
            else
            {
                main.fa?.ResetCalcDisp();
                main.fa?.ResetAnalyzer();
                main.showDiaBoxAfterMeasuring();
                main.ocrScript?.ResetColor();
                main.ocrScript?.ResetVFX();
            }
        }
    }

    [System.Serializable]
    public class LevelManagerClass
    {
        [HideInInspector] public GameBehaviour main;

        // Public properties for cross-class access
        public int CurrentLevel = 0;
       
        public int CurrentProblemIndex = 0;
        public int TotalLevels = 5;
        public int ProblemsPerLevel = 5;

        // Public getters for level info
        public LevelInfo CurrentLevelInfo
        {
            get
            {
                if (CurrentLevel >= 0 && CurrentLevel < GameBehaviour.LEVEL_MAP.Length)
                    return GameBehaviour.LEVEL_MAP[CurrentLevel];
                return null;
            }
        }

        public ShapeSpecs CurrentProblemSpecs
        {
            get
            {
                var levelInfo = CurrentLevelInfo;
                if (levelInfo != null && CurrentProblemIndex >= 0 && CurrentProblemIndex < levelInfo.shapeVariants.Length)
                    return levelInfo.shapeVariants[CurrentProblemIndex];
                return null;
            }
        }

        public bool IsLastProblemInLevel
        {
            get
            {
                var levelInfo = CurrentLevelInfo;
                return levelInfo != null && CurrentProblemIndex >= levelInfo.shapeVariants.Length - 1;
            }
        }

        public bool IsLastLevel
        {
            get
            {
                return CurrentLevel >= TotalLevels - 1;
            }
        }

        public void Initialize(GameBehaviour gameMain)
        {
            main = gameMain;
            CurrentLevel = 0;
            CurrentProblemIndex = 0;
            TotalLevels = 5; // All 5 levels
            ProblemsPerLevel = 5;
        }

        public void InitializeCurrentLevel()
        {
            if (CurrentLevel >= TotalLevels)
            {
                OnAllLevelsCompleted();
                return;
            }

            LevelInfo levelInfo = GameBehaviour.LEVEL_MAP[CurrentLevel];
            main.currentLevelStats = new PlayerStats(CurrentLevel + 1, levelInfo.levelName, levelInfo.focusShape);
            main.levelStartTime = Time.time;

            // NEW: Use ProblemToLoad instead of always starting at 0
            CurrentProblemIndex = GameBehaviour.GameStatePreserver.ProblemToLoad;

            // Safety clamp
            if (CurrentProblemIndex >= levelInfo.shapeVariants.Length)
            {
                Debug.LogWarning($"ProblemToLoad ({CurrentProblemIndex}) exceeds available problems ({levelInfo.shapeVariants.Length}), clamping to 0");
                CurrentProblemIndex = 0;
            }

            main.currentShape = levelInfo.focusShape;
            GlobalVariables.loSelectedShape = levelInfo.focusShape;

            Debug.Log($"Level Manager: Starting Level {CurrentLevel + 1}: {levelInfo.levelName} - Focus Shape: {levelInfo.focusShape}");
            Debug.Log($"Starting at problem {CurrentProblemIndex + 1}/{levelInfo.shapeVariants.Length}");

            UpdateLevelDisplay();
        }

        public void CreateLevelProblem()
        {
            if (CurrentLevel >= TotalLevels) return;

            LevelInfo levelInfo = GameBehaviour.LEVEL_MAP[CurrentLevel];

            if (CurrentProblemIndex >= levelInfo.shapeVariants.Length)
            {
                Debug.Log($"Level Manager: All problems completed for level {CurrentLevel + 1}");
                OnLevelCompleted();
                return;
            }

            ShapeSpecs specs = levelInfo.shapeVariants[CurrentProblemIndex];

            // SIMPLE: Just use the level's focus shape directly
            SHAPES shapeToUse = levelInfo.focusShape;

            Debug.Log($"Level Manager: Creating problem {CurrentProblemIndex + 1}/{levelInfo.shapeVariants.Length} for level {CurrentLevel + 1}");
            Debug.Log($"Level Manager: Shape: {shapeToUse}, Primary: {specs.primaryMeasure}, Secondary: {specs.secondaryMeasure}");

            Problem problem = new Problem(shapeToUse, main, specs.primaryMeasure, specs.secondaryMeasure);
            main.spellCastEvent = new SpellCastEvent(main, problem);
            main.ActivateSpell(main.currentShape);
        }
        // MODIFIED: This is the second part of the bug fix.
        // It now contains the correct logic to decide whether to go to the next problem or the next level.
        public void OnProblemCompleted()
        {
            Debug.Log($"Level Manager: Problem {CurrentProblemIndex + 1} completed!");

            GameStatistics.Instance.RecordProblemCompleted(
                main.GetDifficulty(),
                CurrentLevel,
                CurrentProblemIndex,
                main.currentShape.ToString(),
                main.gameStateManager.CurrentLives,
                main.gameStateManager.CurrentScore
            );

            CurrentProblemIndex++; // Advance to the next problem index
            LevelInfo levelInfo = GameBehaviour.LEVEL_MAP[CurrentLevel];

            // Check if we have finished all the problems (variants) for the current level
            if (CurrentProblemIndex >= levelInfo.shapeVariants.Length)
            {
                Debug.Log($"Level Manager: All problems for Level {CurrentLevel + 1} are complete. Advancing to the next level.");
                // If so, trigger the level completion logic
                OnLevelCompleted();
            }
            else
            {
                Debug.Log($"Level Manager: Preparing transition to the next problem in the same level.");
                // Otherwise, start the transition to the next problem within the *same* level
                main.transitionManager.BeginTransitionToNextProblem();
            }
        }

        public void StartNextProblem()
        {
            CreateLevelProblem();
            main.CompletePhase1Setup();
        }

        // In LevelManagerClass

        //Levels of all of one shape done. (aka all square level)

        public void OnLevelCompleted()
        {
            // Finalize current level stats
            main.currentLevelStats.levelCompleted = true;
            main.currentLevelStats.completionTime = Time.time - main.levelStartTime;
            main.allLevelStats.Add(main.currentLevelStats);

            LevelInfo levelInfo = GameBehaviour.LEVEL_MAP[CurrentLevel];

            GameStatistics.Instance.RecordLevelCompleted(
                main.GetDifficulty(),
                CurrentLevel,
                CurrentProblemIndex - 1, // Last problem that was completed
                levelInfo.focusShape.ToString(),
                main.gameStateManager.CurrentLives,
                main.gameStateManager.CurrentScore,
                main.currentLevelStats.completionTime
            );

            

            
            Debug.Log($"Level Manager: Level {CurrentLevel + 1} ({levelInfo.levelName}) completed!");

            // Move to next level
            CurrentLevel++;
            CurrentProblemIndex = 0; // Reset to first problem of new level

            // Check if all levels completed
            if (CurrentLevel >= TotalLevels)
            {
                OnAllLevelsCompleted();
            }
            else
            {
                main.transitionManager.BeginTransitionToNextLevel();
            }
        }

        public void StartNextLevel()
        {
            // This method is no longer used since we only cycle through squares
            Debug.Log("Level Manager: StartNextLevel called but we only cycle squares - calling StartNextProblem instead");
            StartNextProblem();
        }

        public void OnAllLevelsCompleted()
        {
            Debug.Log("Level Manager: 🎉 All levels completed! Showing final results...");
            main.ShowFinalResults();
        }

        public void UpdateLevelDisplay()
        {
            if (CurrentLevel < TotalLevels)
            {
                LevelInfo levelInfo = GameBehaviour.LEVEL_MAP[CurrentLevel];

                // Update level text (you may need to add this UI element)
                if (main.textHUD != null)
                {
                    main.textHUD.text = $"Level {CurrentLevel + 1}: {levelInfo.levelName} ({CurrentProblemIndex + 1}/{levelInfo.shapeVariants.Length})";
                }
            }
        }

        public void onRestart()
        {
            Debug.Log("Level Manager: Restart called");

            // Reset to beginning of current level
            CurrentProblemIndex = 0;

            // Reset lives
            main.gameStateManager.ResetLives();

            // Call existing restart implementation
            main.onRestartOriginal();
        }
    }

    [System.Serializable]
    public class GameStateManagerClass
    {
        [HideInInspector] public GameBehaviour main;

        // Public properties for cross-class access
        public bool MeasurementPhaseActive = false;
        public bool FormulaSelectionPhaseActive = false; // NEW
        public bool ProblemComplete = false;
        public bool OCRPhaseActive = false; // NEW: State for the OCR phase

        public int CurrentLives = 4;
        public int MaxLives = 4;
        public int CurrentScore = 0;

        // Game flow states
        public bool IsDoneMeasuring = false;
        public bool IsStartup = true;

        // Phase tracking
        public bool HasCompletedMeasurement = false;
        public bool HasCompletedFormulaSelection = false;
        public bool HasCompletedOCRInput = false; // NEW

        /*        public void Initialize(GameBehaviour gameMain)
                {
                    main = gameMain;
                    MeasurementPhaseActive = false;
                    FormulaSelectionPhaseActive = false;
                    ProblemComplete = false;
                    CurrentLives = 4;
                    MaxLives = 4;
                    CurrentScore = 0;
                    IsDoneMeasuring = false;
                    IsStartup = true;
                    HasCompletedMeasurement = false;
                    HasCompletedFormulaSelection = false;
                    OCRPhaseActive = false;
                    HasCompletedOCRInput = false;
                    Debug.Log("Game State Manager: Initialized with Phase 2 support");
                }*/
        public void Initialize(GameBehaviour gameMain)
        {
            main = gameMain;
            MeasurementPhaseActive = false;
            FormulaSelectionPhaseActive = false;
            ProblemComplete = false;

            // FIX: Use the preserved starting lives instead of hardcoding
            CurrentLives = GameBehaviour.GameStatePreserver.StartingLives;
            MaxLives = GameBehaviour.GameStatePreserver.StartingLives;

            CurrentScore = 0;
            IsDoneMeasuring = false;
            IsStartup = true;
            HasCompletedMeasurement = false;
            HasCompletedFormulaSelection = false;
            OCRPhaseActive = false;
            HasCompletedOCRInput = false;
            Debug.Log($"Game State Manager: Initialized with {CurrentLives} lives");
        }

        public void SetOCRPhase(bool active)
        {
            OCRPhaseActive = active;
            if (active)
            {
                // When OCR phase starts, the other phases must be inactive
                MeasurementPhaseActive = false;
                FormulaSelectionPhaseActive = false;
                HasCompletedFormulaSelection = true;

                // --- THE FIX IS HERE ---
                // Set the legacy flag for compatibility with older methods like onCast()
                main.gameStateManager.IsDoneMeasuring = true;
            }
            // No need for an else here, the flag will be reset with the problem.
            Debug.Log($"Game State: OCR phase set to {active}");
        }

        public void OnOCRInputCompleted()
        {
            HasCompletedOCRInput = true;
            SetOCRPhase(false);
            ProblemComplete = true; // The problem is now truly complete

            Debug.Log("Game State: OCR Input completed - Problem finished");
            if (main.phaseManager != null)
            {
                main.phaseManager.OnOCRInputCompleted();
            }
        }



        public void SetMeasurementPhase(bool active)
        {
            MeasurementPhaseActive = active;

            // If disabling measurement, might be moving to formula selection
            if (!active && HasCompletedMeasurement && !HasCompletedFormulaSelection)
            {
                FormulaSelectionPhaseActive = true;
            }
            else if (!active)
            {
                FormulaSelectionPhaseActive = false;
            }

            // Update UI Manager
            main.uiManager.IsInMeasurementPhase = active;
            main.uiManager.UpdateUIBasedOnPhase();

            Debug.Log($"Game State: Measurement phase set to {active}, Formula Selection: {FormulaSelectionPhaseActive}");
        }

        public void SetFormulaSelectionPhase(bool active)
        {
            FormulaSelectionPhaseActive = active;

            if (active)
            {
                // Ensure measurement is complete when entering formula selection
                MeasurementPhaseActive = false;
                HasCompletedMeasurement = true;
            }

            Debug.Log($"Game State: Formula Selection phase set to {active}");

            // Update UI if possible
            if (main.uiManager != null)
            {
                main.uiManager.UpdateUIForCurrentPhase();
            }
        }


        public void OnMeasurementCompleted()
        {
            HasCompletedMeasurement = true;
            Debug.Log("Game State: Measurement phase completed");

            // Transition to formula selection instead of next level
            SetMeasurementPhase(false);
            SetFormulaSelectionPhase(true);

            // Notify phase manager if available
            if (main.phaseManager != null)
            {
                main.phaseManager.OnMeasurementCompleted();
            }
        }

        public void OnFormulaSelectionCompleted()
        {
            HasCompletedFormulaSelection = true;
            SetFormulaSelectionPhase(false);
            // MODIFIED: We no longer mark the problem as complete here
            // ProblemComplete = true; 

            Debug.Log("Game State: Formula Selection completed - moving to OCR phase");

            if (main.phaseManager != null)
            {
                // This call now transitions to the OCR phase
                main.phaseManager.OnFormulaSelectionCompleted();
            }
        }

        public void ResetLives()
        {
            CurrentLives = MaxLives;
            main.UpdateLivesDisplay();
            Debug.Log($"Game State: Lives reset to {CurrentLives}");
        }

        public void ReduceLives(int amount = 1)
        {
            CurrentLives = Mathf.Max(0, CurrentLives - amount);
            main.UpdateLivesDisplay();
            Debug.Log($"Game State: Lives reduced to {CurrentLives}");

            GameStatistics.Instance.RecordLivesLost(
                amount,
                main.GetDifficulty(),
                main.levelManager.CurrentLevel,
                main.levelManager.CurrentProblemIndex,
                main.currentShape.ToString(),
                CurrentLives,
                CurrentScore
            );


            if (CurrentLives <= 0)
            {
                Debug.Log("Game State: Lives depleted - triggering game over");
                OnGameOver();
            }
        }

        public void AddScore(int points)
        {
            CurrentScore += points;
            main.UpdateScoreDisplay();
            Debug.Log($"Game State: Score increased by {points} to {CurrentScore}");
        }

        public void OnGameOver()
        {
            Debug.Log("Game State: GAME OVER - No lives remaining");

            GameStatistics.Instance.RecordGameOver(
                main.GetDifficulty(),
                main.levelManager.CurrentLevel,
                main.levelManager.CurrentProblemIndex,
                main.currentShape.ToString(),
                CurrentLives,
                CurrentScore
            );

            // FORCE stop all input immediately
            MeasurementPhaseActive = false;
            FormulaSelectionPhaseActive = false;

            // Disable input systems
            if (main.lineSnapper != null)
            {
                main.lineSnapper.gameObject.SetActive(false);
            }

            // Hide formula selection buttons
            if (main.formulaSelectionManager != null)
            {
                main.formulaSelectionManager.SetFormulaSelectionActive(false);
            }

            // Show game over message briefly, then reset
            main.ShowMeasurementFeedback("Game Over! Restarting...");

            // Reset after a short delay
            main.onRestart();
        }

        public void ResetForNewProblem()
        {
            ProblemComplete = false;
            IsDoneMeasuring = false;
            HasCompletedMeasurement = false;
            HasCompletedFormulaSelection = false;
            MeasurementPhaseActive = false;
            FormulaSelectionPhaseActive = false;
            HasCompletedOCRInput = false;
            OCRPhaseActive = false;
            Debug.Log("Game State: Reset for new problem");
        }


        public void ResetForSameProblem()
        {
            Debug.Log("Game State: Resetting for same problem");

            // Reset all phase flags
            HasCompletedMeasurement = false;
            HasCompletedFormulaSelection = false;
            ProblemComplete = false;
            IsDoneMeasuring = false;
            MeasurementPhaseActive = false;
            FormulaSelectionPhaseActive = false;

            // Reset lives
            ResetLives();

            Debug.Log("Game State: Reset complete - ready for phase manager restart");
        }

        public bool IsInAnyActivePhase()
        {
            return MeasurementPhaseActive || FormulaSelectionPhaseActive;
        }

        public string GetCurrentPhaseDescription()
        {
            if (MeasurementPhaseActive) return "Measurement";
            if (FormulaSelectionPhaseActive) return "Formula Selection";
            if (ProblemComplete) return "Complete";
            return "Unknown";
        }
    }




    [System.Serializable]
    public class OCRManagerClass
    {
        [HideInInspector] public GameBehaviour main;

        // References to the UI this manager controls
        private GameObject ocrInputObject;
        private GameObject formulaDisplayObject;
        private DrawingAndOCRManagerScript ocrScript;

        // Animation transforms
        private Transform ocrStartTransform;
        private Transform ocrEndTransform;

        public void Initialize(GameBehaviour gameMain)
        {
            main = gameMain;

            // Cache all necessary references from the main GameBehaviour script
            this.ocrInputObject = main.ocrInput;
            this.formulaDisplayObject = main.formulaDisplay;
            this.ocrScript = main.ocrScript;
            this.ocrStartTransform = main.ocrStartTransform;
            this.ocrEndTransform = main.ocrEndTransform;

            Debug.Log("OCR Manager: Initialized");
        }


        public void Activate()
        {
            Debug.Log("OCR Manager: Activating...");

            // Clean up line snapper display
            if (main.lineSnapper != null) main.lineSnapper.ToggleLineText();

            // CRITICAL: Ensure OCR script is ready for input
            if (main.ocrScript != null)
            {
                main.ocrScript.processing = false;
                main.ocrScript.ResetColor();
                main.ocrScript.ResetVFX();
            }

            // Start the complete animation sequence
            main.StartCoroutine(SlideOCRBoardCoroutine(true));
        }

        /// <summary>
        /// Deactivates the OCR phase, typically after an answer is submitted.
        /// </summary>
        public void Deactivate()
        {
            Debug.Log("OCR Manager: Deactivating...");
            main.StartCoroutine(SlideOCRBoardCoroutine(false));
        }

        private IEnumerator SlideOCRBoardCoroutine(bool show)
        {
            Debug.Log($"========== SLIDE OCR BOARD COROUTINE START (show={show}) ==========");

            if (show)
            {
                Debug.Log("[Step 1] Moving dialogue to intermediate position");
                Debug.Log($"[Step 1] rtDialogue current: {main.rtDialogue?.anchoredPosition}");

                // STEP 1: Initial setup
                if (main.rtDialogue != null)
                    main.StartCoroutine(main.RectTransformOverTime(main.rtDialogue, 0.25f, new Vector2(600f, -151.46f)));

                if (main.rtDiaButtons != null)
                    main.StartCoroutine(main.RectTransformOverTime(main.rtDiaButtons, 0.25f, GameBehaviour.rightButtonPos));

                yield return GameBehaviour.dialogueWait;

                Debug.Log("[Step 2] Starting main OCR animations");
                Debug.Log($"[Step 2] ocrInput position before: {ocrInputObject?.transform.position}");
                Debug.Log($"[Step 2] ocrEndTransform target: {ocrEndTransform?.position}");

                // STEP 2: Animate three things
                if (ocrEndTransform != null)
                    main.StartCoroutine(main.MoveOverTime(ocrInputObject, 0.35f, ocrEndTransform.position));
                if (main.rtDialogue != null)
                    main.StartCoroutine(main.RectTransformOverTime(main.rtDialogue, 0.35f, GameBehaviour.ocrDialoguePos));
                if (main.pDialogue != null)
                    main.StartCoroutine(main.LocalScaleOverTime(main.pDialogue, 0.35f, GameBehaviour.smallScale));

                yield return GameBehaviour.ocrWait;

                Debug.Log("[Step 3] Enabling input after animation");
                Debug.Log($"[Step 3] ocrInput position after: {ocrInputObject?.transform.position}");
                Debug.Log($"[Step 3] rtDialogue position after: {main.rtDialogue?.anchoredPosition}");

                // STEP 3: Enable input
                if (ocrScript != null)
                {
                    ocrScript.ResetColor();
                    ocrScript.ResetVFX();
                    ocrScript.processing = false;
                    Debug.Log("[Step 3] OCR script reset and enabled");
                }
                if (formulaDisplayObject != null)
                {
                    formulaDisplayObject.SetActive(true);
                    Debug.Log($"[Step 3] formulaDisplay activated: {formulaDisplayObject.activeSelf}");
                }
                if (main.backspaceButton != null)
                {
                    main.backspaceButton.SetActive(true);
                    Debug.Log($"[Step 3] backspaceButton activated: {main.backspaceButton.activeSelf}");
                }
                if (main.characterSay != null) main.characterSay.text = "";

                main.ShowHint(3);
                Debug.Log("[Step 3] Hint shown");

                // Check all children
                if (ocrInputObject != null)
                {
                    Debug.Log($"ocrInputObject child count: {ocrInputObject.transform.childCount}");
                    for (int i = 0; i < ocrInputObject.transform.childCount; i++)
                    {
                        var child = ocrInputObject.transform.GetChild(i);
                        Debug.Log($"  Child {i}: {child.name}, active: {child.gameObject.activeSelf}");
                    }
                }

                // Check Canvas/Renderer components
                var canvas = ocrInputObject?.GetComponent<Canvas>();
                //Debug.Log($"ocrInputObject has Canvas? {canvas != null}, enabled? {canvas?.enabled}");

                var canvasGroup = ocrInputObject?.GetComponent<CanvasGroup>();
                //Debug.Log($"ocrInputObject has CanvasGroup? {canvasGroup != null}, alpha: {canvasGroup?.alpha}");

                //Debug.Log("==============================================");

                //Debug.Log("========== OCR ANIMATION COMPLETE ==========");
            }
            else
            {
                Debug.Log("[Deactivate] Starting OCR deactivation");

                if (ocrScript != null) ocrScript.processing = true;
                if (formulaDisplayObject != null) formulaDisplayObject.SetActive(false);

                if (ocrStartTransform != null)
                    main.StartCoroutine(main.MoveOverTime(ocrInputObject, 0.35f, ocrStartTransform.position));
                if (main.rtDialogue != null)
                    main.StartCoroutine(main.RectTransformOverTime(main.rtDialogue, 0.35f, main.origDiaRT));
                if (main.pDialogue != null)
                    main.StartCoroutine(main.LocalScaleOverTime(main.pDialogue, 0.35f, GameBehaviour.normalScale));

                yield return GameBehaviour.ocrWait;

                if (ocrInputObject != null) ocrInputObject.SetActive(false);

                main.toggleDialogueBox();

                Debug.Log("========== OCR DEACTIVATION COMPLETE ==========");
            }
        }
    }

    #endregion

    #region Module Instances
    [HideInInspector] public UIManagerClass uiManager = new UIManagerClass();
    [HideInInspector] public LevelManagerClass levelManager = new LevelManagerClass();
    [HideInInspector] public GameStateManagerClass gameStateManager = new GameStateManagerClass();
    // MODIFIED: This was previously a stub, now it's a full member.
    [HideInInspector] public TransitionManagerClass transitionManager = new TransitionManagerClass();
    [HideInInspector] public OCRManagerClass ocrManager = new OCRManagerClass(); // NEW: Add the OCR Manager instance


    #endregion

    #region NEW: Modified Game State Variables
    // Lives system (replaces attempt system)
    public int currentLives = 4;
    private const int MAX_LIVES = 4;
    private const int TOTAL_LEVELS = 5;
    private const int PROBLEMS_PER_LEVEL = 5;

    // Statistics and level tracking
    [HideInInspector] public PlayerStats currentLevelStats;
    [HideInInspector] public List<PlayerStats> allLevelStats = new List<PlayerStats>();


    [HideInInspector] public int currentProblemIndex = 0;    // Which variant within the level
    [HideInInspector] public float levelStartTime;

    // 3D Array Structure - map[level][shapeVariant][measurements]
    [HideInInspector]
    public static readonly LevelInfo[] LEVEL_MAP = new LevelInfo[TOTAL_LEVELS]
    {
        // Level 1 - SQUARE variants (different sizes)
        new LevelInfo("Square Mastery", SHAPES.SQUARE, new ShapeSpecs[]
        {
            new ShapeSpecs(3.0f,4.0f),    // Small square
            new ShapeSpecs(4.0f),    // Medium square  
            new ShapeSpecs(5.0f),    // Large square
            new ShapeSpecs(6.0f),    // Extra large square
            new ShapeSpecs(2.5f)     // Tiny square
        }),
        
        // Level 2 - RECTANGLE variants (different dimensions)
        new LevelInfo("Rectangle Workshop", SHAPES.RECTANGLE, new ShapeSpecs[]
        {
            new ShapeSpecs(4.0f, 3.0f),    // Standard rectangle
            new ShapeSpecs(6.0f, 2.0f),    // Wide rectangle
            new ShapeSpecs(3.0f, 5.0f),    // Tall rectangle
            new ShapeSpecs(7.0f, 8.0f),    // Large rectangle
            new ShapeSpecs(7.0f, 3.0f)     // Extra wide rectangle
        }),
        
        // Level 3 - CIRCLE variants (different diameters)
        new LevelInfo("Circle Academy", SHAPES.CIRCLE, new ShapeSpecs[]
        {
            new ShapeSpecs(4.0f),    // Small circle
            new ShapeSpecs(5.0f),    // Medium circle
            new ShapeSpecs(6.0f),    // Large circle
            new ShapeSpecs(3.0f),    // Tiny circle
            new ShapeSpecs(7.0f)     // Extra large circle
        }),
        
        // Level 4 - TRIANGLE variants (different base/height combinations)
        new LevelInfo("Triangle Training", SHAPES.TRIANGLE, new ShapeSpecs[]
        {
            new ShapeSpecs(4.0f, 3.0f),    // Standard triangle
            new ShapeSpecs(5.0f, 4.0f),    // Medium triangle
            new ShapeSpecs(3.0f, 5.0f),    // Tall triangle
            new ShapeSpecs(7.0f, 8.0f),    // Wide triangle
            new ShapeSpecs(4.0f, 6.0f)     // Very tall triangle
        }),
        
        // Level 5 - SEMI_CIRCLE variants (different diameters)
        new LevelInfo("Semi-Circle School", SHAPES.SEMI_CIRCLE, new ShapeSpecs[]
        {
            new ShapeSpecs(4.0f),    // Small semi-circle
            new ShapeSpecs(5.0f),    // Medium semi-circle
            new ShapeSpecs(6.0f),    // Large semi-circle
            new ShapeSpecs(3.0f),    // Tiny semi-circle
            new ShapeSpecs(7.0f)     // Extra large semi-circle
        })
    };

    private int DifficultyLevel
    {
        get { return GameStatePreserver.Difficulty; }
        set { GameStatePreserver.Difficulty = value; }
    }

    public int GetDifficulty()
    {
        return GameStatePreserver.Difficulty;
    }

    /// <summary>
    /// Sets the difficulty level (1-4)
    /// Updates GameStatePreserver directly
    /// </summary>
    public void SetDifficulty(int difficulty)
    {
        GameStatePreserver.Difficulty = Mathf.Clamp(difficulty, 1, 4);
        Debug.Log($"Difficulty set to {GameStatePreserver.Difficulty}");
    }

    public bool measurementPhaseActive = false;
    private bool problemComplete = false;

    // Guide system
    private LineRenderer[] guideLines;
    public bool[] guidesCompleted;
    public int requiredMeasurements = 0;
    private int completedMeasurements = 0;

    // Variable tracking for current shape
    private string[] currentVariables;
    public float[] measuredValues;

    // Track hint coroutine to stop it
    public Coroutine blinkSpriteCoroutine;
    // TASK 2: New coroutine reference for the guide animation.
    private Coroutine guideAnimationCoroutine;
    #endregion

    #region Public Fields (Inspector)
    public SpellCastEvent spellCastEvent;
    public ShapeGenerator shapeGenerator;
    public ShapeFiller shapeFiller;
    public LineSnapper lineSnapper;

    // UI
    public GameObject hud;
    public GameObject quickMenu;
    public GameObject panelMagicScroll;
    public GameObject pConfirm;
    public GameObject pLowerScroll;
    public GameObject pNotify;
    public GameObject pDialogue;
    public GameObject pDiaButtons;
    public GameObject notifyTextObj;
    public LeftHandedMode canvasScript;

    // Text refs
    public Text scoreText;
    public Text livesText;
    public Text textTemp;
    public Text textEME;
    public Text pConfirmText;
    public Text pNotifyText;
    public Text characterSay;
    public Text textFinish;
    public Text confirmText;
    public Text textHUD;
    public Text undoText;

    // TASK 5: New UI elements for the reusable confirmation prompt.
    [Header("Measurement Confirmation")]
    public Text textPlayerConfirmation; // Should be linked to 'Text-Player-Confirmation'
    public Button btnConfirmYes;        // Should be linked to 'Confirm-Yes'
    public Button btnConfirmNo;         // Should be linked to 'Confirm-No'

    [Header("Hint System")]
    public GameObject textHint;
    public GameObject textHintSpell;
    public GameObject textHintUndo;
    public GameObject textHintCalcu;
    public GameObject spriteHint;
    public GameObject spriteHintUndo;
    public GameObject spriteHintSpell;
    public GameObject spriteHintCalcu;
    public Image spriteHintImg;
    public Image spriteHintImgUndo;
    public Image spriteHintImgSpell;
    public Image spriteHintImgCalcu;

    [Header("Equation Panels")]
    public GameObject pEquationTriangle;
    public GameObject pEquationSquare;
    public GameObject pEquationRectangle;
    public GameObject pEquationSCircle;
    public GameObject pEquationCircle;

    [Header("Buttons")]
    public Button bYesHome;
    public Button bYes;
    public Button btnConfirmSpell;
    public Button btnMeasure;

    // OCR and formula
    [Header("OCR & Formula")]
    public GameObject ocrInput;
    public GameObject formulaDisplay;
    public Transform rightStartTransform, rightEndTransform, leftStartTransform, leftEndTransform;
    public Transform ocrStartTransform, ocrEndTransform;
    public GameObject formulaAnalyzerObj;
    public GameObject calcBtnObj;
    public GameObject backspaceButton;

    /*[Header("Variable Displays")]
    public GameObject sqVarDisp1, rectVarDisp1, rectVarDisp2, triVarDisp1, triVarDisp2, cirVarDisp1, semiVarDisp1;*/
    [Header("Variable Display System")]
    public VariableDisplayManager variableDisplayManager = new VariableDisplayManager();

    [Header("Sound")]
    public GameObject soundPlayerObj;

    [Header("Image")]
    public Image undoBtnImg;
    public Image undoBtnLogo;
    public Sprite undoLogoDefault;
    public Sprite undoLogoCast;
    #endregion

    #region Private / Cached Fields
    public SHAPES currentShape;
    private int currentScore = 0;

    // legacy flags (kept for compatibility)
    private bool cp, ls; //Activity states of UI components
    private GameObject mainCamera, classroomCamera;
    public Animator screenFadeAnimator;

    public AnimScript animScript;
    private bool STARTUP = true;
    public float error = 100f;

    // Save/load
    private GameData savedGame;
    private SaveLoadController saverLoader = new SaveLoadController();
    private string savePath;

    // Cached RectTransform references
    public RectTransform rtDialogue;
    public RectTransform rtDiaButtons;

    public bool isDoneMeasuring;

    // OCR and formula
    public DrawingAndOCRManagerScript ocrScript;
    public FormulaAnalyzer fa;
    public Vector2 origDiaRT;

    private float inputAnswer = 0f;

    private Text calcBtnText;

    // Variable display refs
    private GameObject var1Display, var2Display;

    // Sound player
    public GameLevelSoundPlayer soundPlayer;

    // Cached vectors/constants
    public static readonly Vector2 hiddenDialoguePos = new Vector2(600f, -150f);
    public static readonly Vector2 shownDialoguePos = new Vector2(225f, 130f);
    public static readonly Vector2 measuringDialoguePos = new Vector2(600f, 130f);
    public static readonly Vector2 ocrDialoguePos = new Vector2(308f, 100f);
    public static readonly Vector2 leftButtonPos = new Vector2(-493f, -167f);
    public static readonly Vector2 rightButtonPos = new Vector2(-493f, 138f);
    public static readonly Vector3 normalScale = new Vector3(1f, 1f, 1f);
    public static readonly Vector3 smallScale = new Vector3(0.9f, 0.9f, 0.9f);

    public static readonly WaitForSeconds blinkDelay = new WaitForSeconds(STARTDELAY + 0.4f);
    public static readonly WaitForSeconds dialogueWait = new WaitForSeconds(DIALOGUESLIDETIME);
    public static readonly WaitForSeconds ocrWait = new WaitForSeconds(OCRSLIDETIME);

    // Measures
    private float[] currentMeasureArray;
    private float[] currentCircleMeasureArray;

    // Other cached data
    public System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(64);

    // New: previously missing declarations
    private TMP_Text correctionPerc;
    public string chosenShape;

    // Position validation constants
    private const float POSITION_TOLERANCE = 0.01f;
    private const float LENGTH_TOLERANCE = 0.01f;
    #endregion

    #region NEW: Phase 1 Methods



    /// <summary>
    /// Completes Phase 1 setup after problem is ready - FIXED TIMING
    /// </summary>
    public void CompletePhase1Setup()
    {
        Debug.Log("=== CompletePhase1Setup called ===");

        // BUG FIX: Reset the game and measurement state for the new problem.
        // This ensures flags like `IsDoneMeasuring` are reset to false, allowing
        // the gesture input to work correctly on subsequent problems.
        gameStateManager.ResetForNewProblem();
        completedMeasurements = 0;

        // FIXED: Wait for spellCastEvent to be ready before proceeding
        if (spellCastEvent?.problem == null)
        {
            Debug.Log("spellCastEvent not ready, retrying...");
            Invoke(nameof(CompletePhase1Setup), 0.1f);
            return;
        }

        // FIXED: Ensure GridSystem is available before enabling LineSnapper
        GridSystem gridSystem = FindObjectOfType<GridSystem>();
        if (gridSystem == null)
        {
            Debug.LogError("GridSystem not found! Retrying...");
            Invoke(nameof(CompletePhase1Setup), 0.1f);
            return;
        }

        // FIXED: Initialize LineSnapper BEFORE enabling
        if (lineSnapper != null)
        {
            // Make sure LineSnapper has GridSystem reference
            lineSnapper.gridSystem = gridSystem;

            // FORCE INITIALIZE LineSnapper with all dependencies
            lineSnapper.ForceInitialize();

            // BUG FIX: DO NOT activate the LineSnapper here.
            // The UIManager will handle activation based on the current game phase
            // to prevent race conditions during level transitions.
            Debug.Log($"LineSnapper properly initialized and awaits activation by UIManager.");
        }
        else
        {
            Debug.LogError("LineSnapper is null in CompletePhase1Setup!");
            return;
        }

        // Wait a frame to ensure LineSnapper is fully ready, then proceed
        Invoke(nameof(AutoSelectShapeAndProceed), 0.1f);
    }

    /// <summary>
    /// FIXED: Safer initialization that prevents input during transitions
    /// </summary>
    private void InitializeMeasurementPhase()
    {
        Debug.Log("=== InitializeMeasurementPhase called ===");

        // Reset all state
        gameStateManager.SetMeasurementPhase(false); // Start disabled
        gameStateManager.ResetForNewProblem();
        completedMeasurements = 0;

        // Wait for everything to be ready before enabling
        Invoke(nameof(EnableMeasurementPhase), 0.3f);
    }

    /// <summary>
    /// NEW: Separate method to safely enable measurement phase
    /// </summary>
    private void EnableMeasurementPhase()
    {
        Debug.Log("=== EnableMeasurementPhase called ===");

        // Only enable if everything is ready
        if (spellCastEvent?.problem == null)
        {
            Debug.Log("Problem not ready, retrying...");
            Invoke(nameof(EnableMeasurementPhase), 0.1f);
            return;
        }

        gameStateManager.SetMeasurementPhase(true);
        AutoSelectShapeAndProceed();
    }

    /// <summary>
    /// Automatically selects shape and proceeds to measurement - bypasses all UI
    /// </summary>
    private void AutoSelectShapeAndProceed()
    {
        Debug.Log("=== AutoSelectShapeAndProceed called ===");

        // FORCE STOP any running hint animations
        HideMeasureHint();

        // Use the current shape from the problem
        if (spellCastEvent?.problem != null)
        {
            currentShape = spellCastEvent.problem.problemShape;
            Debug.Log($"Shape from spellCastEvent: {currentShape}");
        }
        else
        {
            // Fallback to GlobalVariables if available
            currentShape = GlobalVariables.loSelectedShape;
            Debug.Log($"Shape from GlobalVariables: {currentShape}");
        }

        Debug.Log($"Auto-selected shape: {currentShape}");

        // Set chosenShape to match current shape (for existing validation)
        chosenShape = currentShape.ToString();

        // HIDE ALL SHAPE SELECTION UI
        if (panelMagicScroll != null) panelMagicScroll.SetActive(false);
        if (btnConfirmSpell != null) btnConfirmSpell.gameObject.SetActive(false);
        if (pLowerScroll != null) pLowerScroll.SetActive(false);

        // Skip directly to measurement setup
        SkipToMeasurementSetup();
    }

    /// <summary>
    /// Skips to measurement setup - mimics what btnYes() does but automatically
    /// </summary>
    private void SkipToMeasurementSetup()
    {
        Debug.Log("=== SkipToMeasurementSetup called ===");

        hideDiaBoxWhileMeasuring();
        if (btnConfirmSpell != null) btnConfirmSpell.gameObject.SetActive(false);
        if (panelMagicScroll != null) panelMagicScroll.SetActive(false);
        if (pDiaButtons != null) pDiaButtons.SetActive(true);

        ActivateEquationForShape(chosenShape);
        SetupGuidesForShape(currentShape);
        //UpdateVariableDisplay();
        UpdateScoreDisplay();
        UpdateLivesDisplay();

        if (pDialogue != null) pDialogue.SetActive(false);
        if (characterSay != null) characterSay.text = "";

        // CRITICAL: Ensure measurement phase is active for input
        gameStateManager.SetMeasurementPhase(true);
        phaseManager.TransitionToPhase(PhaseManagerClass.GamePhase.Measurement);

        Debug.Log($"Phase 1 setup complete for: {currentShape}");
    }

    /// <summary>
    /// Sets up semi-transparent guide lines for the current shape - FIXED COLOR
    /// </summary>
    private void SetupGuidesForShape(SHAPES shape)
    {
        // Determine required measurements based on existing code patterns
        switch (shape)
        {
            case SHAPES.SQUARE:
            case SHAPES.CIRCLE:
            case SHAPES.SEMI_CIRCLE:
                requiredMeasurements = 1;
                currentVariables = new string[1]; // Size initialized, value set later
                break;
            case SHAPES.RECTANGLE:
            case SHAPES.TRIANGLE:
                requiredMeasurements = 2;
                currentVariables = new string[2]; // Size initialized, values set later
                break;
            default:
                requiredMeasurements = 0;
                currentVariables = new string[0];
                break;
        }

        // Initialize arrays
        guideLines = new LineRenderer[requiredMeasurements];
        guidesCompleted = new bool[requiredMeasurements];
        measuredValues = new float[requiredMeasurements];

        // Create guide lines
        CreateGuideLines();

        // TASK 4: After creating guides, correctly assign variable names based on orientation.
        AssignVariableNamesByOrientation();
    }

    /// <summary>
    /// Creates STRONG WHITE guide lines - FIXED COLOR
    /// </summary>
    private void CreateGuideLines()
    {
        for (int i = 0; i < requiredMeasurements; i++)
        {
            GameObject guideObj = new GameObject($"GuideLine_{i}");
            guideObj.transform.parent = transform;

            LineRenderer guide = guideObj.AddComponent<LineRenderer>();
            guide.positionCount = 2;
            guide.startWidth = 0.15f;
            guide.endWidth = 0.15f;
            guide.useWorldSpace = true;

            guide.material = new Material(Shader.Find("Sprites/Default"));
            Color guideColor = Color.white;
            guide.startColor = guideColor;
            guide.endColor = guideColor;

            PositionGuide(guide, i);

            guideLines[i] = guide;
            guidesCompleted[i] = false;
        }

        // TASK 2: Start the guide animation coroutine after creating the lines.
        if (guideAnimationCoroutine != null) StopCoroutine(guideAnimationCoroutine);
        guideAnimationCoroutine = StartCoroutine(AnimateGuideLines());
    }

    // TASK 2: New coroutine to animate the guide lines with a pulsing glow effect.
    private IEnumerator AnimateGuideLines()
    {
        // Loop indefinitely while the coroutine is active.
        while (true)
        {
            // Mathf.PingPong creates a value that goes from 0.0 to 1.0 and back to 0.0.
            // This is perfect for a smooth pulsing effect.
            float alpha = Mathf.PingPong(Time.time, 1.0f);

            // Iterate through all the guide lines.
            for (int i = 0; i < guideLines.Length; i++)
            {
                // Only animate lines that exist and have not been completed yet.
                if (guideLines[i] != null && !guidesCompleted[i])
                {
                    // Get the current color, update its alpha, and apply it back.
                    Color newColor = guideLines[i].startColor;
                    newColor.a = alpha;
                    guideLines[i].startColor = newColor;
                    guideLines[i].endColor = newColor;
                }
            }
            // Wait until the next frame to update again.
            yield return null;
        }
    }


    /// <summary>
    /// Positions guide lines consistently for each shape
    /// </summary>
    private void PositionGuide(LineRenderer guide, int index)
    {
        if (spellCastEvent?.problem?.problemObjectShape == null) return;

        Vector3 shapePos = spellCastEvent.problem.problemObjectShape.transform.position;

        // SAFE: Get bounds with null check
        Renderer shapeRenderer = spellCastEvent.problem.problemObjectShape.GetComponent<Renderer>();
        if (shapeRenderer == null) return;

        Bounds shapeBounds = shapeRenderer.bounds;

        Vector3 start, end;

        switch (currentShape)
        {
            case SHAPES.SQUARE:
            case SHAPES.CIRCLE:
            case SHAPES.SEMI_CIRCLE:
                // Single horizontal line for diameter/side
                start = new Vector3(shapeBounds.min.x, shapePos.y, 0);
                end = new Vector3(shapeBounds.max.x, shapePos.y, 0);
                break;

            case SHAPES.RECTANGLE:
                if (index == 0) // Length - horizontal
                {
                    start = new Vector3(shapeBounds.min.x, shapePos.y, 0);
                    end = new Vector3(shapeBounds.max.x, shapePos.y, 0);
                }
                else // Width - vertical
                {
                    start = new Vector3(shapePos.x, shapeBounds.min.y, 0);
                    end = new Vector3(shapePos.x, shapeBounds.max.y, 0);
                }
                break;

            case SHAPES.TRIANGLE:
                if (index == 0) // Base - horizontal at bottom
                {
                    start = new Vector3(shapeBounds.min.x, shapeBounds.min.y, 0);
                    end = new Vector3(shapeBounds.max.x, shapeBounds.min.y, 0);
                }
                else // Height - vertical from base to apex
                {
                    start = new Vector3(shapePos.x, shapeBounds.min.y, 0);
                    end = new Vector3(shapePos.x, shapeBounds.max.y, 0);
                }
                break;

            default:
                start = Vector3.zero;
                end = Vector3.zero;
                break;
        }

        guide.SetPosition(0, start);
        guide.SetPosition(1, end);
    }

    // TASK 4: New method to assign correct variable names based on guide orientation.
    // REPLACE the existing AssignVariableNamesByOrientation() method

    private void AssignVariableNamesByOrientation()
    {
        if (currentVariables == null || guideLines == null) return;

        switch (currentShape)
        {
            case SHAPES.SQUARE:
                currentVariables[0] = "S";
                break;

            case SHAPES.CIRCLE:
            case SHAPES.SEMI_CIRCLE:
                currentVariables[0] = "D"; // Diameter
                break;

            case SHAPES.RECTANGLE:
                // For rectangles: ALWAYS assign based on actual guide positions
                AssignRectangleVariables();
                break;

            case SHAPES.TRIANGLE:
                // For triangles: ALWAYS assign based on actual guide positions
                AssignTriangleVariables();
                break;
        }
    }

    private void AssignRectangleVariables()
    {
        // Strategy: Find which guide is horizontal and which is vertical.
        int horizontalIndex = -1;
        int verticalIndex = -1;

        for (int i = 0; i < guideLines.Length; i++)
        {
            if (guideLines[i] == null) continue;

            Vector3 start = guideLines[i].GetPosition(0);
            Vector3 end = guideLines[i].GetPosition(1);

            float deltaX = Mathf.Abs(end.x - start.x);
            float deltaY = Mathf.Abs(end.y - start.y);

            if (deltaX > deltaY) // The line is wider than it is tall, so it's horizontal.
            {
                horizontalIndex = i;
            }
            else // The line is taller than it is wide, so it's vertical.
            {
                verticalIndex = i;
            }
        }

        // CRITICAL FIX: Assign variable names based on the guide's physical orientation.
        // This ensures the UI labels are always correct.
        if (horizontalIndex != -1)
        {
            // The horizontal guide ALWAYS represents Width ('W').
            currentVariables[horizontalIndex] = "W";
            Debug.Log($"Rectangle: Guide {horizontalIndex} is HORIZONTAL → Assigning 'W'");
        }
        if (verticalIndex != -1)
        {
            // The vertical guide ALWAYS represents Length ('L').
            currentVariables[verticalIndex] = "L";
            Debug.Log($"Rectangle: Guide {verticalIndex} is VERTICAL → Assigning 'L'");
        }
    }
    private void AssignTriangleVariables()
    {
        // Strategy: Find which guide is horizontal (Base) and which is vertical (Height)
        int horizontalIndex = -1;
        int verticalIndex = -1;

        for (int i = 0; i < guideLines.Length; i++)
        {
            if (guideLines[i] == null) continue;

            Vector3 start = guideLines[i].GetPosition(0);
            Vector3 end = guideLines[i].GetPosition(1);

            float deltaX = Mathf.Abs(end.x - start.x);
            float deltaY = Mathf.Abs(end.y - start.y);

            if (deltaX > deltaY)
            {
                horizontalIndex = i; // This is the Base (B)
            }
            else
            {
                verticalIndex = i; // This is the Height (H)
            }
        }

        // CRITICAL: Assign variables based on actual orientation
        if (horizontalIndex != -1)
        {
            currentVariables[horizontalIndex] = "B";
            Debug.Log($"Triangle: Guide {horizontalIndex} is HORIZONTAL → B");
        }
        if (verticalIndex != -1)
        {
            currentVariables[verticalIndex] = "H";
            Debug.Log($"Triangle: Guide {verticalIndex} is VERTICAL → H");
        }
    }

    [System.Serializable]
    private class MeasurementAttempt
    {
        public int measurementIndex;
        public float value;
        public Vector3 drawnStart;
        public Vector3 drawnEnd;
    }

    private MeasurementAttempt lastMeasurementAttempt;
    public void OnMeasurementCompleted(int measurementIndex, float value, Vector3 drawnStart, Vector3 drawnEnd)
    {
        Debug.Log($"=== OnMeasurementCompleted called: index={measurementIndex}, value={value} ===");
        Debug.Log($"=== Drawn line from {drawnStart} to {drawnEnd} ===");

        if (!gameStateManager.MeasurementPhaseActive)
        {
            Debug.Log("Measurement phase not active - ignoring measurement");
            return;
        }

        if (measurementIndex < 0 || measurementIndex >= requiredMeasurements)
        {
            Debug.Log($"Invalid measurement index: {measurementIndex}, required: {requiredMeasurements}");
            return;
        }

        // NEW: Store the measurement attempt for validation after confirmation
        lastMeasurementAttempt = new MeasurementAttempt
        {
            measurementIndex = measurementIndex,
            value = value,
            drawnStart = drawnStart,
            drawnEnd = drawnEnd
        };

        // ALWAYS show confirmation dialog, don't validate yet
        ShowSingleMeasurementConfirmation();
    }

    private void ShowSingleMeasurementConfirmation()
    {
        // 1. Perform initial safety checks to ensure all data is available.
        if (lastMeasurementAttempt == null || currentVariables == null || guidesCompleted == null)
        {
            Debug.LogError("Confirmation prompt cannot be shown: Missing required data (lastMeasurementAttempt, currentVariables, or guidesCompleted).");
            return;
        }

        // 2. Determine the variable name for the *new* measurement being confirmed.
        int likelyGuideIndex = FindClosestGuideByPosition(lastMeasurementAttempt.drawnStart, lastMeasurementAttempt.drawnEnd);
        string newVarName = "Sukat"; // Default to a generic term

        // --- BUG FIX STARTS HERE ---
        // If the player drew in the wrong place, FindClosestGuideByPosition will fail.
        // In that case, we assume they were trying to measure the FIRST available uncompleted guide.
        // This prevents the prompt from showing the unhelpful "Sukat" default value.
        if (likelyGuideIndex == -1)
        {
            Debug.Log("Positional match failed. Assuming user intended to measure the first uncompleted guide.");
            // Find the index of the first guide that has not been completed yet.
            for (int i = 0; i < guidesCompleted.Length; i++)
            {
                if (!guidesCompleted[i])
                {
                    likelyGuideIndex = i; // We found it.
                    break;                // Stop searching.
                }
            }
        }
        // --- BUG FIX ENDS HERE ---

        if (likelyGuideIndex != -1 && likelyGuideIndex < currentVariables.Length)
        {
            newVarName = currentVariables[likelyGuideIndex];
        }

        // 3. Build the confirmation message string.
        var confirmMsg = new System.Text.StringBuilder("Sigurado ka na ba sa sukat ng hugis ay ");

        // If this is a two-measurement shape and one is already done, show it first.
        if (requiredMeasurements > 1)
        {
            // Find the index of the already completed guide.
            int completedGuideIndex = Array.IndexOf(guidesCompleted, true);
            if (completedGuideIndex != -1)
            {
                // Append the already-confirmed measurement ("L = 4.0 at ")
                string oldVarName = currentVariables[completedGuideIndex];
                float oldVarValue = measuredValues[completedGuideIndex];
                confirmMsg.Append($"{oldVarName} = {oldVarValue:F1} at ");
            }
        }

        // 4. Always append the new measurement that the player just drew.
        confirmMsg.Append($"{newVarName} = {lastMeasurementAttempt.value:F1}");
        confirmMsg.Append("?");

        // 5. Display the dialog with the fully constructed message.
        uiManager.ShowConfirmationDialog(
            confirmMsg.ToString(),
            ValidateLastMeasurement,
            UndoLastMeasurementDirect
        );
    }
    /// <summary>
    /// NEW: Validate the stored measurement attempt after player confirms
    /// </summary>
    private void ValidateLastMeasurement()
    {
        if (lastMeasurementAttempt == null) return;

        Debug.Log("Player confirmed measurement. Now validating...");

        // Find which guide this measurement matches (any order)
        int matchedGuideIndex = FindMatchingGuide(
            lastMeasurementAttempt.drawnStart,
            lastMeasurementAttempt.drawnEnd,
            lastMeasurementAttempt.value
        );

        if (matchedGuideIndex >= 0)
        {
            // Check if this guide was already completed
            if (guidesCompleted != null && guidesCompleted[matchedGuideIndex])
            {
                Debug.Log($"Guide {matchedGuideIndex} already completed - treating as incorrect");
                OnIncorrectMeasurement();
                return;
            }

            // Success! Mark this specific guide as completed
            measuredValues[matchedGuideIndex] = lastMeasurementAttempt.value;
            OnCorrectMeasurement(matchedGuideIndex);
        }
        else
        {
            Debug.Log("No matching guide found for this measurement");
            OnIncorrectMeasurement();
        }

        lastMeasurementAttempt = null;
    }

    /// <summary>
    /// NEW: Undo measurement without validation (player said "No" to confirmation)
    /// </summary>
    private void UndoLastMeasurementDirect()
    {
        Debug.Log("Player chose to undo measurement without validation.");
        if (lineSnapper != null)
        {
            lineSnapper.OnUndoPressed();
        }
        lastMeasurementAttempt = null;
    }


    public int FindMatchingGuide(Vector3 drawnStart, Vector3 drawnEnd, float drawnValue)
    {
        if (guideLines == null) return -1;

        for (int i = 0; i < guideLines.Length; i++)
        {
            if (guideLines[i] == null || (guidesCompleted != null && guidesCompleted[i]))
                continue; // Skip completed or null guides

            // Check both position and length for this guide
            bool positionMatches = ValidateLinePositionForGuide(i, drawnStart, drawnEnd);
            bool lengthMatches = ValidateMeasurementLengthForGuide(i, drawnValue);

            Debug.Log($"Guide {i} check - Position: {positionMatches}, Length: {lengthMatches}");

            if (positionMatches && lengthMatches)
            {
                Debug.Log($"Found matching guide: {i}");
                return i;
            }
        }

        Debug.Log("No guide matched the drawn line");
        return -1; // No match found
    }

    /// <summary>
    /// NEW: Validate position for a specific guide index
    /// </summary>
    private bool ValidateLinePositionForGuide(int guideIndex, Vector3 drawnStart, Vector3 drawnEnd)
    {
        if (guideLines == null || guideIndex >= guideLines.Length || guideLines[guideIndex] == null)
            return false;

        LineRenderer guide = guideLines[guideIndex];
        Vector3 guideStart = guide.GetPosition(0);
        Vector3 guideEnd = guide.GetPosition(1);

        Debug.Log($"Guide {guideIndex}: {guideStart} to {guideEnd}");
        Debug.Log($"Drawn line: {drawnStart} to {drawnEnd}");

        // Check if drawn line aligns with guide (allowing left-to-right or right-to-left)
        bool alignmentA = IsLineAligned(drawnStart, drawnEnd, guideStart, guideEnd);
        bool alignmentB = IsLineAligned(drawnStart, drawnEnd, guideEnd, guideStart); // Reversed

        bool isAligned = alignmentA || alignmentB;
        Debug.Log($"Guide {guideIndex} alignment: Normal={alignmentA}, Reversed={alignmentB}, Final={isAligned}");

        return isAligned;
    }

    private bool ValidateMeasurementLengthForGuide(int guideIndex, float drawnValue)
    {
        if (spellCastEvent?.problem == null)
        {
            Debug.LogError("spellCastEvent.problem is null in ValidateMeasurementLengthForGuide");
            return false;
        }

        float expectedValue = GetExpectedMeasurementForGuide(guideIndex);
        bool isValid = Mathf.Abs(drawnValue - expectedValue) <= LENGTH_TOLERANCE;

        Debug.Log($"Guide {guideIndex} length validation: Expected={expectedValue:F2}, Got={drawnValue:F2}, Tolerance={LENGTH_TOLERANCE}, Valid={isValid}");

        return isValid;
    }

    private float GetExpectedMeasurementForGuide(int guideIndex)
    {
        if (spellCastEvent?.problem == null) return 1.0f;

        Debug.Log($"Getting expected measurement for guide {guideIndex}, shape: {currentShape}");
        Debug.Log($"Problem p_measure: {spellCastEvent.problem.p_measure}, s_measure: {spellCastEvent.problem.s_measure}");

        switch (currentShape)
        {
            case SHAPES.SQUARE:
            case SHAPES.CIRCLE:
            case SHAPES.SEMI_CIRCLE:
                // Only one guide, always use p_measure
                return spellCastEvent.problem.p_measure;

            case SHAPES.RECTANGLE:
            case SHAPES.TRIANGLE:
                // Two guides - map based on the guide positioning, not draw order
                return MapGuideToMeasurement(guideIndex);

            default:
                Debug.LogError($"Unknown shape: {currentShape}");
                return 1.0f;
        }
    }

    // REPLACE MapGuideToMeasurement()

    private float MapGuideToMeasurement(int guideIndex)
    {
        if (guideLines == null || guideIndex >= guideLines.Length || guideLines[guideIndex] == null)
            return 1.0f;

        // Determine the orientation of the specific guide we are checking.
        LineRenderer guide = guideLines[guideIndex];
        Vector3 start = guide.GetPosition(0);
        Vector3 end = guide.GetPosition(1);
        bool isHorizontal = Mathf.Abs(end.x - start.x) > Mathf.Abs(end.y - start.y);

        Debug.Log($"Validating Guide {guideIndex}: isHorizontal = {isHorizontal}");

        switch (currentShape)
        {
            case SHAPES.RECTANGLE:
                // CONVENTION: p_measure stores Width (horizontal), s_measure stores Length (vertical).
                // This logic correctly returns the expected value based on the guide's orientation.
                return isHorizontal ? spellCastEvent.problem.p_measure : spellCastEvent.problem.s_measure;

            case SHAPES.TRIANGLE:
                // CONVENTION: p_measure stores Base (horizontal), s_measure stores Height (vertical).
                return isHorizontal ? spellCastEvent.problem.p_measure : spellCastEvent.problem.s_measure;

            default:
                return spellCastEvent.problem.p_measure;
        }
    }

    /// <summary>
    /// NEW: Check if two lines are aligned within tolerance
    /// </summary>
    private bool IsLineAligned(Vector3 drawnStart, Vector3 drawnEnd, Vector3 guideStart, Vector3 guideEnd)
    {
        // Check if start points are close
        float startDistance = Vector3.Distance(drawnStart, guideStart);
        float endDistance = Vector3.Distance(drawnEnd, guideEnd);

        bool startClose = startDistance <= POSITION_TOLERANCE;
        bool endClose = endDistance <= POSITION_TOLERANCE;

        Debug.Log($"Distance check - Start: {startDistance:F2} <= {POSITION_TOLERANCE} ({startClose}), End: {endDistance:F2} <= {POSITION_TOLERANCE} ({endClose})");

        return startClose && endClose;
    }

    public void OnCorrectMeasurement(int guideIndex)
    {
        Debug.Log($"=== OnCorrectMeasurement called for guide {guideIndex} ===");

        GameStatistics.Instance.RecordCorrectAnswer(
            GetDifficulty(),
            levelManager.CurrentLevel,
            levelManager.CurrentProblemIndex,
            currentShape.ToString(),
            gameStateManager.CurrentLives,
            gameStateManager.CurrentScore
        );

        if (gameStateManager.CurrentLives <= 0) return;

        if (currentLevelStats != null) currentLevelStats.AddCorrectAttempt();

        if (guidesCompleted != null && guideIndex < guidesCompleted.Length) guidesCompleted[guideIndex] = true;

        if (guideLines != null && guideIndex < guideLines.Length && guideLines[guideIndex] != null)
        {
            // TASK 2: Make the completed line fully transparent to stop the animation.
            Color transparent = guideLines[guideIndex].startColor;
            transparent.a = 0f;
            guideLines[guideIndex].startColor = transparent;
            guideLines[guideIndex].endColor = transparent;
        }

        completedMeasurements++;
        Debug.Log($"Completed measurements: {completedMeasurements}/{requiredMeasurements}");

        gameStateManager.AddScore(POINTS_PER_CORRECT);
        UpdateVariableDisplay();

        if (completedMeasurements >= requiredMeasurements && requiredMeasurements > 0)
        {
            Debug.Log("All measurements completed for this problem!");
            gameStateManager.SetMeasurementPhase(false);

            // MODIFIED: No confirmation here, proceed directly
            ProceedToFormulaSelection();
        }

        PlayCorrectSound();
    }

    // TASK 5: New method to be called when the player confirms their measurements.
    public void ProceedToFormulaSelection()
    {
        Debug.Log("All measurements confirmed. Proceeding to formula selection.");
        CleanupGuides();
        if (lineSnapper != null) lineSnapper.gameObject.SetActive(false);
        gameStateManager.OnMeasurementCompleted();
    }

    // TASK 5: New method to be called when player wants to undo their last measurement.
    /*public void UndoLastMeasurement()
    {
        Debug.Log("Player chose to undo the last measurement.");
        if (lineSnapper != null) lineSnapper.OnUndoPressed();

        // We must re-enable the measurement phase for the player to draw again.
        gameStateManager.SetMeasurementPhase(true);
    }*/


    // FIX 4: Modified OnIncorrectMeasurement() method  
    // REPLACE the existing method with this:

    // REPLACE the existing OnIncorrectMeasurement method with this one in GameBehaviour.cs

    public void OnIncorrectMeasurement()
    {
        Debug.Log($"=== OnIncorrectMeasurement called ===");

        // Check if already at 0 lives to prevent double-processing
        if (gameStateManager.CurrentLives <= 0)
        {
            Debug.Log("Already at 0 lives - ignoring incorrect measurement");
            return;
        }

        // Reduce lives by 1
        GameStatistics.Instance.RecordWrongAnswer(
            GetDifficulty(),
            levelManager.CurrentLevel,
            levelManager.CurrentProblemIndex,
            currentShape.ToString(),
            gameStateManager.CurrentLives,
            gameStateManager.CurrentScore
        );

        gameStateManager.ReduceLives(1);

        // Track wrong attempt in stats
        if (currentLevelStats != null)
        {
            currentLevelStats.AddWrongAttempt();
        }

        Debug.Log($"Lives remaining: {gameStateManager.CurrentLives}/{gameStateManager.MaxLives}");

        // Auto-undo the incorrect line
        if (lineSnapper != null)
        {
            lineSnapper.OnUndoPressed();
            Debug.Log("Auto-undid incorrect measurement");
        }

        // Show helpful feedback only if still alive
        if (gameStateManager.CurrentLives > 0)
        {
            ShowMeasurementFeedback($"Incorrect! {gameStateManager.CurrentLives} lives remaining.");
        }

        // Visual and audio feedback
        PlayIncorrectVisualFX();
        PlayIncorrectSound();

        // *** THE FIX IS HERE ***
        // After undoing the line, refresh the variable display to show "?".
        // This ensures the UI is in a clean state for the player's next attempt.
        UpdateVariableDisplay();

        // Game over is handled automatically by GameStateManager.ReduceLives()
    }

    /// <summary>
    /// Cleanup guide lines
    /// </summary>
    public void CleanupGuides()
    {
        // TASK 2: Stop the animation coroutine when cleaning up guides.
        if (guideAnimationCoroutine != null)
        {
            StopCoroutine(guideAnimationCoroutine);
            guideAnimationCoroutine = null;
        }

        if (guideLines != null)
        {
            for (int i = 0; i < guideLines.Length; i++)
            {
                if (guideLines[i] != null)
                {
                    Destroy(guideLines[i].gameObject);
                }
            }
        }
        guideLines = null;
        guidesCompleted = null;
    }

    /// <summary>
    /// Updates variable display - B = ?, H = ?, S = ?
    /// </summary>
    private void UpdateVariableDisplay()
    {
        variableDisplayManager.UpdateDisplay(currentVariables, guidesCompleted, measuredValues);
    }

    /// <summary>
    /// Updates score with big numbers for kids
    /// </summary>
    public void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = gameStateManager.CurrentScore.ToString("N0");
            scoreText.fontSize = 48; // Big font for kids
        }
    }

    /// <summary>
    /// Updates lives display
    /// </summary>
    public void UpdateLivesDisplay()
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {gameStateManager.CurrentLives}";
        }
    }

    /// <summary>
    /// NEW: Show helpful feedback to user
    /// </summary>
    private void ShowMeasurementFeedback(string message)
    {
        Debug.Log($"User Feedback: {message}");

        // If you have a feedback text UI element, you can update it here
        if (characterSay != null)
        {
            characterSay.text = message;
        }
    }

    /// <summary>
    /// Visual FX for incorrect - placeholder for teammates
    /// </summary>
    private void PlayIncorrectVisualFX()
    {
        // Empty placeholder - teammates implement their visual effects here
        Debug.Log("Play Incorrect Visual FX - Implement your effects here");
    }

    /// <summary>
    /// Sound effects using existing system
    /// </summary>
    private void PlayCorrectSound()
    {
        // TASK 1: Fixed pitch and assuming clip index 2 is for "correct".
        if (soundPlayer != null)
            soundPlayer.PlaySFX(2, 1.0f, 1.0f);
    }

    private void PlayIncorrectSound()
    {
        // TASK 1: Fixed pitch and assuming clip index 1 is for "incorrect".
        if (soundPlayer != null)
            soundPlayer.PlaySFX(1, 1.0f, 1.0f);
    }

    // Wrapper method for level manager
    public void StartNextProblem()
    {
        levelManager.StartNextProblem();
    }

    // Wrapper method for level manager
    public void StartNextLevel()
    {
        levelManager.StartNextLevel();
    }

    #endregion

    #region Analytics and Results
    /// <summary>
    /// Get current level statistics
    /// </summary>
    public PlayerStats GetCurrentLevelStats()
    {
        return currentLevelStats;
    }

    /// <summary>
    /// Get all completed level statistics
    /// </summary>
    public List<PlayerStats> GetAllLevelStats()
    {
        return new List<PlayerStats>(allLevelStats);
    }

    /// <summary>
    /// Get level configuration for external access
    /// </summary>
    public LevelInfo GetLevelInfo(int level)
    {
        if (level >= 0 && level < TOTAL_LEVELS)
            return LEVEL_MAP[level];
        return null;
    }

    /// <summary>
    /// Show final results screen
    /// </summary>
    public void ShowFinalResults()
    {
        Debug.Log("=== FINAL RESULTS ===");

        int totalCorrect = 0;
        int totalWrong = 0;
        float totalTime = 0f;

        foreach (PlayerStats stats in allLevelStats)
        {
            Debug.Log($"Level {stats.level} ({stats.levelName}): {stats.correctAttempts} correct, {stats.wrongAttempts} wrong, {stats.GetAccuracyPercentage():F1}% accuracy");
            totalCorrect += stats.correctAttempts;
            totalWrong += stats.wrongAttempts;
            totalTime += stats.completionTime;
        }

        Debug.Log($"OVERALL: {totalCorrect} correct, {totalWrong} wrong, Total time: {totalTime:F1}s");

        // You can display this in UI or save to file
        SaveStatisticsToFile();
    }

    /// <summary>
    /// Save statistics to file for later analysis
    /// </summary>
    private void SaveStatisticsToFile()
    {
        string statsJson = JsonUtility.ToJson(new { levels = allLevelStats });
        string path = Path.Combine(Application.persistentDataPath, "player_stats.json");
        File.WriteAllText(path, statsJson);
        Debug.Log($"Statistics saved to: {path}");
    }
    #endregion

    #region Awake/Start/Update
    void Awake()
    {
        PlayerPrefs.DeleteAll();
        // Pre-calculate save path once
        savePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        savedGame = saverLoader.loadGame(savePath);

        currentShape = SHAPES.NONE;

  
        // *** MOVED UP - Activate Left handed mode BEFORE initializing managers ***
        // Activate Left handed mode based on save data
        if (savedGame.isLeftHanded)
        {
            canvasScript.ToggleLeftHandedMode();

            //Set OCR transforms
            ocrStartTransform = leftStartTransform;
            ocrEndTransform = leftEndTransform;

            //Move OCR board to new start pos
            ocrInput.transform.position = ocrStartTransform.position;

            //OFfsets to correct positions
            formulaDisplay.GetComponent<RectTransform>().anchoredPosition = new Vector2(90, 10); //Offset to correct text pos
            ocrEndTransform.position -= new Vector3(3f, 0f, 0f); //Offset to correct board position
        }
        else //Default right positions
        {
            ocrStartTransform = rightStartTransform;
            ocrEndTransform = rightEndTransform;
        }
        // *** END OF MOVED SECTION ***

        // Initialize modular systems AFTER transforms are assigned
        uiManager.Initialize(this);
        levelManager.Initialize(this);
        gameStateManager.Initialize(this);
        transitionManager.Initialize(this);
        ocrManager.Initialize(this);



        // Pre-cache measure arrays based on level
        CacheMeasureArrays();

        variableDisplayManager.Initialize(this);

        phaseManager.Initialize(this);
        formulaSelectionManager.Initialize(this);
    }



    void Start()
    {
        GameConfiguration.Create()
        .SetLevel("Triangle", 3)      // 3rd triangle problem. O-index
        .SetDifficulty(4)              // Hardest difficulty
        .SetLives(2)                   // Only 2 lives
        .SetShowHints(true)           // No variable hints
        .Apply(); 

        // NEW: Set Phase 1 flag IMMEDIATELY to block hint system
        gameStateManager.SetMeasurementPhase(true);
        phaseManager.TransitionToPhase(PhaseManagerClass.GamePhase.Measurement);
        //Debug.Log("=== PHASE 1 ACTIVATED THROUGH PHASE MANAGER ===");
        Debug.Log("=== PHASE 1 ACTIVATED IMMEDIATELY ===");

        // Batch GameObject.Find calls and cache results
        InitializeTextComponents();
        InitializeUIComponents();
        InitializeGameState();

        if (STARTUP)
        {
            screenFadeAnimator?.SetTrigger("fadeIn");
            HideNewUI();
            soundPlayer = soundPlayerObj?.GetComponent<GameLevelSoundPlayer>();
        }

        correctionPerc = GameObject.Find("ManaFillCorrectPerc")?.GetComponent<TMP_Text>();

        mainCamera = GameObject.Find("Main Camera");
        mainCamera?.SetActive(false);
        classroomCamera = GameObject.Find("ClassroomCamera");
        classroomCamera?.SetActive(true);

        if (lineSnapper != null) lineSnapper.animScript = this.animScript;

        // ---- Former Reset logic ----

        currentShape = SHAPES.NONE;
        correctionPerc?.gameObject.SetActive(false);

        // If there was a previous problem shape, clean it up (only meaningful on restarts)
        levelManager.CurrentLevel = GameStatePreserver.LevelToLoad;

        if (!STARTUP)
        {
            if (spellCastEvent?.problem?.problemObjectShape != null)
                Destroy(spellCastEvent.problem.problemObjectShape);
        }


        if (lineSnapper != null)
        {
            // Call undo twice (kept from original logic)
            lineSnapper.OnUndoPressed();
            lineSnapper.OnUndoPressed();
        }
        else
        {
            Debug.LogError("LineSnapper is null in Start()!");
        }

        if (!STARTUP)
            screenFadeAnimator?.SetTrigger("fadeIn");

        ToClass();
        Invoke(nameof(StartLevelAnim), STARTDELAY);

        // MODIFIED: Initialize current level instead of random problem
        levelManager.InitializeCurrentLevel();
        Invoke(nameof(CreateLevelProblemWrapper), 0.1f);
        Invoke(nameof(InitFillShape), 0.2f);

        // NEW: Complete Phase 1 setup after problem is ready
        Invoke(nameof(CompletePhase1Setup), 0.3f);

        STARTUP = false;

        // ----------------------------------------------

        correctionPerc?.gameObject.SetActive(false);
        if (lineSnapper != null)
        {
            lineSnapper.gameObject.SetActive(false);
            Debug.Log("LineSnapper disabled again at end of Start()");
        }
    }

    public void RestartCurrentProblem()
    {
        Debug.Log("=== RestartCurrentProblem called ===");

        // Reset to measurement phase
        gameStateManager.ResetForSameProblem();

        // Reset measurement state
        completedMeasurements = 0;

        // Clean up any existing guides
        CleanupGuides();

        // Reset LineSnapper
        if (lineSnapper != null)
        {
            lineSnapper.OnUndoPressed();
            lineSnapper.OnUndoPressed();
            lineSnapper.gameObject.SetActive(true);
            lineSnapper.enabled = true;
        }

        // Setup measurement phase again
        SetupGuidesForShape(currentShape);
        UpdateVariableDisplay();
        UpdateScoreDisplay();
        UpdateLivesDisplay();

        Debug.Log("Current problem restarted - back to measurement phase");
    }

    void Update()
    {
        if (fa.GetIsEquMode() && undoBtnImg.color != Color.cyan) //Change to cyan on final answer
        {
            undoText.text = undoBtnText2;
            undoBtnImg.color = Color.cyan;
            undoBtnLogo.sprite = undoLogoCast;
        }
        else if (!fa.GetIsEquMode() && undoBtnImg.color != Color.red) //Change to Red on not final answer
        {
            undoText.text = undoBtnText1;
            undoBtnImg.color = Color.red;
            undoBtnLogo.sprite = undoLogoDefault;
        }

        // Update UI visibility based on current phase
        uiManager.UpdateUIForCurrentPhase();
    }
    #endregion

    #region Cache & Init helpers
    /*private void CacheGameObjectReferences()
    {
        var sf = GameObject.Find("ScreenFade");
        if (sf != null) screenFadeAnimator = sf.GetComponent<Animator>();

        var ah = GameObject.Find("AnimHolder");
        if (ah != null) animScript = ah.GetComponent<AnimScript>();

        if (ocrInput != null)
        {
            var d = ocrInput.transform.Find("DrawingAndOCRManager");
            if (d != null) ocrScript = d.GetComponent<DrawingAndOCRManagerScript>();
        }

        if (formulaAnalyzerObj != null) fa = formulaAnalyzerObj.GetComponent<FormulaAnalyzer>();

        if (notifyTextObj != null) pNotifyText = notifyTextObj.GetComponent<Text>();

        if (calcBtnObj != null)
        {
            var cb = calcBtnObj.transform.Find("textFinish");
            if (cb != null) calcBtnText = cb.gameObject.GetComponent<Text>();
            calcBtnObj.SetActive(false);
        }
    }
*/
    // In GameBehaviour.cs

    private void CacheGameObjectReferences()
    {
        var sf = GameObject.Find("ScreenFade");
        if (sf != null) screenFadeAnimator = sf.GetComponent<Animator>();

        var ah = GameObject.Find("AnimHolder");
        if (ah != null) animScript = ah.GetComponent<AnimScript>();

        // --- BUG FIX IS HERE ---
        // The original code used transform.Find(), which is not recursive and can fail if the
        // object hierarchy changes. GetComponentInChildren is much more robust.
        if (ocrInput != null)
        {
            ocrScript = ocrInput.GetComponentInChildren<DrawingAndOCRManagerScript>();
            if (ocrScript == null)
            {
                Debug.LogError("FATAL: DrawingAndOCRManagerScript could not be found anywhere within the ocrInput GameObject hierarchy!");
            }
        }
        // --- END OF FIX ---

        if (formulaAnalyzerObj != null) fa = formulaAnalyzerObj.GetComponent<FormulaAnalyzer>();

        if (notifyTextObj != null) pNotifyText = notifyTextObj.GetComponent<Text>();

        if (calcBtnObj != null)
        {
            var cb = calcBtnObj.transform.Find("textFinish");
            if (cb != null) calcBtnText = cb.gameObject.GetComponent<Text>();
            calcBtnObj.SetActive(false);
        }
    }
    private void CacheMeasureArrays()
    {
        switch (GlobalVariables.level)
        {
            case 0:
            case 1:
                currentMeasureArray = GlobalVariables.loMeasures1;
                currentCircleMeasureArray = GlobalVariables.loCircleMeasures1;
                break;
            case 2:
            case 3:
                currentMeasureArray = GlobalVariables.loMeasures2;
                currentCircleMeasureArray = GlobalVariables.loCircleMeasures2;
                break;
        }
    }

    /*private void InitializeVariableDisplays()
    {
        switch (GlobalVariables.loSelectedShape)
        {
            case SHAPES.SQUARE:
                var1Display = sqVarDisp1;
                break;
            case SHAPES.RECTANGLE:
                var1Display = rectVarDisp1;
                var2Display = rectVarDisp2;
                break;
            case SHAPES.TRIANGLE:
                var1Display = triVarDisp1;
                var2Display = triVarDisp2;
                break;
            case SHAPES.CIRCLE:
                var1Display = cirVarDisp1;
                break;
            case SHAPES.SEMI_CIRCLE:
                var1Display = semiVarDisp1;
                break;
        }
    }*/

    private void InitializeTextComponents()
    {
        characterSay = GameObject.Find("characterSay")?.GetComponent<Text>();
        if (textFinish != null) textFinish.text = castBtnText1;
    }

    private void InitializeUIComponents()
    {
        pDialogue = GameObject.Find("PanelCasting");
        if (pDialogue != null) rtDialogue = pDialogue.GetComponent<RectTransform>();
        origDiaRT = rtDialogue != null ? rtDialogue.anchoredPosition : Vector2.zero;
        pDiaButtons = GameObject.Find("pDiaButtons");
        if (pDiaButtons != null) rtDiaButtons = pDiaButtons.GetComponent<RectTransform>();

        stringBuilder.Clear();
        stringBuilder.Append(savedGame != null ? savedGame.currRoom : string.Empty);
        stringBuilder.Append(" ROOM");
        if (textHUD != null) textHUD.text = stringBuilder.ToString();
    }

    private void InitializeGameState()
    {
        gameStateManager.IsDoneMeasuring = false;

        if (pDialogue != null) pDialogue.SetActive(true);
        if (pConfirm != null) pConfirm.SetActive(false);
        if (pLowerScroll != null) pLowerScroll.SetActive(true);
        if (pNotify != null) pNotify.SetActive(false);

        hideAllEquation();

        if (pDiaButtons != null) pDiaButtons.SetActive(false);

        InitializeHintSystem();

        if (btnConfirmSpell != null) btnConfirmSpell.gameObject.SetActive(false);
        if (btnMeasure != null) btnMeasure.gameObject.SetActive(true);

        if (pConfirmText != null) pConfirmText.text = "";
        if (bYesHome != null) bYesHome.gameObject.SetActive(false);
        if (bYes != null) bYes.gameObject.SetActive(false);

        ShowHint(0);

        // set transparent spell hint color
        if (spriteHintImgSpell != null)
        {
            var spellColor = spriteHintImgSpell.color;
            spellColor.a = 0f;
            spriteHintImgSpell.color = spellColor;
        }
    }

    private void InitializeHintSystem()
    {
        GameObject[] hintsToHide = {
            spriteHint, spriteHintSpell, spriteHintCalcu, textHint,
            spriteHintUndo, textHintUndo, textHintCalcu, textHintSpell
        };

        for (int i = 0; i < hintsToHide.Length; i++)
            if (hintsToHide[i] != null)
                hintsToHide[i].SetActive(false);
    }
    #endregion

    #region Button handlers & UI methods - Now using UIManager
    public void onRestart()
    {
        levelManager.onRestart();
    }

    public void onRestartOriginal()
    {
        Debug.Log("=== onRestart called ===");

        // CRITICAL: Disable input immediately during restart
        measurementPhaseActive = false;
        if (lineSnapper != null)
        {
            lineSnapper.gameObject.SetActive(false);
        }

        if (formulaDisplay != null) formulaDisplay.SetActive(false);
        screenFadeAnimator?.SetTrigger("sceneOut");

        // set spell hint transparent
        if (spriteHintImgSpell != null)
        {
            var spellColor = spriteHintImgSpell.color;
            spellColor.a = 0f;
            spriteHintImgSpell.color = spellColor;
        }

        GameStatePreserver.LevelToLoad = levelManager.CurrentLevel;

        Invoke(nameof(LoadSceneDelay), TRANSITIONDELAY);
    }

    private void LoadSceneDelay()
    {
        SceneManager.LoadScene("LoadingScreen");
    }

    public void onQuit()
    {
        error = 100f;
        screenFadeAnimator?.SetTrigger("sceneOut");
        Invoke(nameof(EndGameFunctions), TRANSITIONDELAY);
    }

    public void onUndo()
    {
        uiManager.onUndo();
    }

    public void toggleConfirmScreen(string what)
    {
        if (what == "shape")
            what = chosenShape;

        if (pConfirm == null) return;

        bool isActive = !pConfirm.activeInHierarchy;
        pConfirm.SetActive(isActive);

        if (isActive)
            HideMeasureHint();

        bool isHomeConfirm = what == "confirmHome";
        bYesHome?.gameObject.SetActive(isHomeConfirm);
        bYes?.gameObject.SetActive(!isHomeConfirm && pConfirm.activeInHierarchy);

        if (isHomeConfirm)
        {
            if (pConfirmText != null) pConfirmText.text = homeConfirmMsg;
            if (confirmText != null) confirmText.text = progressNotSavedMsg;
        }
        else if (pConfirm.activeInHierarchy)
        {
            if (pConfirmText != null) pConfirmText.text = correctChoiceMsg;
            stringBuilder.Clear();
            stringBuilder.Append("[ ");
            stringBuilder.Append(what);
            stringBuilder.Append(" ]?");
            if (confirmText != null) confirmText.text = stringBuilder.ToString();
        }
    }

    public void toggleMagicScroll()
    {
        if (pLowerScroll != null)
            pLowerScroll.SetActive(!pLowerScroll.activeInHierarchy);
    }

    public void hideAllEquation()
    {
        pEquationSquare?.SetActive(false);
        pEquationSCircle?.SetActive(false);
        pEquationCircle?.SetActive(false);
        pEquationRectangle?.SetActive(false);
        pEquationTriangle?.SetActive(false);
    }

    public void chooseSquare() => ChooseShape("SQUARE", pEquationSquare);
    public void chooseSemiCircle() => ChooseShape("SEMI_CIRCLE", pEquationSCircle);
    public void chooseCircle() => ChooseShape("CIRCLE", pEquationCircle);
    public void chooseRectangle() => ChooseShape("RECTANGLE", pEquationRectangle);
    public void chooseTriangle() => ChooseShape("TRIANGLE", pEquationTriangle);

    private void ChooseShape(string shape, GameObject equation)
    {
        chosenShape = shape;
        hideAllEquation();
        equation?.SetActive(true);
    }

    public void btnNo()
    {
        toggleConfirmScreen("");
    }

    public void hideDiaBoxWhileMeasuring()
    {
        if (rtDialogue != null)
            StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, hiddenDialoguePos));
        backspaceButton?.SetActive(false);
    }

    public void showDiaBoxAfterMeasuring()
    {
        if (rtDialogue != null)
            StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, shownDialoguePos));
    }

    public void toggleDialogueBox()
    {
        if (rtDialogue == null || rtDiaButtons == null) return;

        if (Math.Abs(rtDialogue.anchoredPosition.y - 100f) < 1f)
        {
            StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, measuringDialoguePos));
            StartCoroutine(RectTransformOverTime(rtDiaButtons, DIALOGUESLIDETIME, leftButtonPos));
        }
        else
        {
            StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, new Vector2(600f, -151.46f)));

            if (!gameStateManager.IsDoneMeasuring)
            {
                StartCoroutine(RectTransformOverTime(rtDiaButtons, DIALOGUESLIDETIME, leftButtonPos));
                btnMeasure?.gameObject.SetActive(true);
                backspaceButton?.SetActive(false);
            }
            else
            {
                btnMeasure?.gameObject.SetActive(false);
                backspaceButton?.SetActive(true);
                StartCoroutine(RectTransformOverTime(rtDiaButtons, DIALOGUESLIDETIME, rightButtonPos));
            }
        }
    }

    public void onCast()
    {
        uiManager.onCast();
    }
    #endregion

    #region Hints
    private void ShowHint(int step)
    {
        // PHASE 1: Skip all hints
        if (gameStateManager.MeasurementPhaseActive)
        {
            Debug.Log("Phase 1 active - skipping hints");
            return;
        }

        HideMeasureHint();

        switch (step)
        {
            case 0:
                spriteHintSpell?.SetActive(true);
                blinkSpriteCoroutine = StartCoroutine(BlinkSprite(step));
                break;
            case 1:
                spriteHint?.SetActive(true);
                textHint?.SetActive(true);
                spriteHintUndo?.SetActive(true);
                textHintUndo?.SetActive(true);
                blinkSpriteCoroutine = StartCoroutine(BlinkSprite(step));
                break;
            case 3:
                textHintCalcu?.SetActive(true);
                spriteHintCalcu?.SetActive(true);
                blinkSpriteCoroutine = StartCoroutine(BlinkSprite(step));
                break;
        }
    }

    private void HideMeasureHint()
    {
        // PHASE 1: Stop the specific hint coroutine
        if (gameStateManager.MeasurementPhaseActive && blinkSpriteCoroutine != null)
        {
            StopCoroutine(blinkSpriteCoroutine);
            blinkSpriteCoroutine = null;
            Debug.Log("Stopped hint animation coroutine for Phase 1");
        }

        GameObject[] hintsToHide = {
            spriteHint, textHint, spriteHintUndo, textHintUndo,
            textHintCalcu, spriteHintCalcu, spriteHintSpell, textHintSpell
        };

        for (int i = 0; i < hintsToHide.Length; i++)
            if (hintsToHide[i] != null)
                hintsToHide[i].SetActive(false);
    }

    private IEnumerator BlinkSprite(int step)
    {
        Color color0 = spriteHintImgSpell != null ? spriteHintImgSpell.color : Color.white;
        Color color1 = spriteHintImg != null ? spriteHintImg.color : Color.white;
        Color color2 = spriteHintImgUndo != null ? spriteHintImgUndo.color : Color.white;
        Color color3 = spriteHintImgCalcu != null ? spriteHintImgCalcu.color : Color.white;

        if (step == 0)
        {
            yield return blinkDelay;
            textHintSpell?.SetActive(true);
            btnConfirmSpell?.gameObject.SetActive(true);
        }

        float elapsed = 0f;

        while (true)
        {
            // PHASE 1: Exit if measurement phase is active
            if (gameStateManager.MeasurementPhaseActive)
            {
                Debug.Log("BlinkSprite stopped - Phase 1 active");
                yield break;
            }

            if (Input.GetMouseButtonDown(0))
            {
                HideMeasureHint();
                yield break;
            }

            elapsed += Time.deltaTime;
            float alpha = Mathf.PingPong(elapsed, 1f);

            color0.a = alpha;
            color1.a = alpha;
            color2.a = alpha;
            color3.a = alpha;

            if (spriteHintImgSpell != null) spriteHintImgSpell.color = color0;
            if (spriteHintImg != null) spriteHintImg.color = color1;
            if (spriteHintImgUndo != null) spriteHintImgUndo.color = color2;
            if (spriteHintImgCalcu != null) spriteHintImgCalcu.color = color3;

            yield return null;
        }
    }
    #endregion

    #region Selection / Confirmation
    public void btnYes()
    {
        if (savedGame != null && savedGame.currRoom == chosenShape)
        {
            hideDiaBoxWhileMeasuring();
            ShowHint(1);
            btnConfirmSpell?.gameObject.SetActive(false);
            panelMagicScroll?.SetActive(false);
            pDiaButtons?.SetActive(true);
            lineSnapper?.gameObject.SetActive(true);

            ActivateEquationForShape(chosenShape);
        }
        else
        {
            notifyWrongShape();
        }

        toggleConfirmScreen("");
    }

    public void ActivateEquationForShape(string shape)
    {
        switch (shape)
        {
            case "TRIANGLE": pEquationTriangle?.SetActive(true); break;
            case "SQUARE": pEquationSquare?.SetActive(true); break;
            case "RECTANGLE": pEquationRectangle?.SetActive(true); break;
            case "CIRCLE": pEquationCircle?.SetActive(true); break;
            case "SEMI_CIRCLE": pEquationSCircle?.SetActive(true); break;
        }
    }

    public void ActivateSpell(SHAPES s)
    {
        animScript?.VideoPlayerScript?.PlaySpellIntro(s);
    }
    #endregion

    #region Notifications / OCR
  
 

    private void ResumeOCR()
    {
        if (ocrScript != null) ocrScript.processing = false;
    }

    #region Notifications / OCR

    /// <summary>
    /// A centralized and safe method for showing the notification panel.
    /// It handles null checks, sets the message, shows the panel, and locks OCR input.
    /// </summary>
    /// <param name="message">The text to display in the notification panel.</param>
    private void ShowNotification(string message)
    {
        // Safety Check: If the panel or its text component isn't assigned in the Inspector,
        // log a clear error and do nothing. This prevents the game from crashing or soft-locking.
        if (pNotify == null || pNotifyText == null)
        {
            Debug.LogError($"Cannot show notification because pNotify or pNotifyText is not assigned in the Inspector! Message was: '{message}'");
            return;
        }

        // Lock OCR input to prevent drawing while the notification is visible.
        if (ocrScript != null)
        {
            ocrScript.processing = true;
        }

        // Set the message and show the panel.
        pNotifyText.text = message;
        pNotify.SetActive(true);
    }

    /// <summary>
    /// Hides the notification panel and safely re-enables OCR input.
    /// This should be called by the 'OK' button on the pNotify panel.
    /// </summary>
    public void CloseNotification()
    {
        if (pNotify != null)
        {
            pNotify.SetActive(false);
        }

        // Only re-enable OCR input if we are in a state where it's expected.
        if (gameStateManager.IsDoneMeasuring)
        {
            ResetOCRInput();
        }
    }

    // --- REWORKED NOTIFICATION CALLS ---

    public void notifyWrongShape()
    {
        // FIX: This now correctly shows the panel AND sets the appropriate error message.
        ShowNotification(wrongShapeMsg);
    }

    public void NotifyInvalidFormula()
    {
        ShowNotification(invalidFormulaMsg);
    }

    public void NotifyMismatchedAnswer()
    {
        ShowNotification(mismatchedAnswerMsg);
    }

    #endregion
    #endregion

    #region Calculator / OCR toggle
    public void ToggleCalcMode()
    {
        ocrScript?.ResetColor();
        ocrScript?.ResetVFX();

        if (fa == null) return;

        if (fa.calcMode)
        {
            if (calcBtnText != null) calcBtnText.text = "Calculator";
            fa.ExitCalc();
        }
        else
        {
            if (calcBtnText != null) calcBtnText.text = "Formula Input";
            fa.EnterCalc();
        }
    }
    #endregion

    #region Measurement & Casting Flow
    /*    public void DoneMeasure()
        {
            gameStateManager.IsDoneMeasuring = true;
            if (textFinish != null) textFinish.text = castBtnText2;

            if (GlobalVariables.level < 3)
                calcBtnObj?.SetActive(true);

            if (var1Display != null)
                var1Display.GetComponent<Text>().text = lineSnapper?.value1;
            if (var2Display != null)
                var2Display.GetComponent<Text>().text = lineSnapper?.value2;

            StartCoroutine(SlideOCRBoard(true));
            lineSnapper?.ToggleLineText();
        }*/

    /* public void UndoMeasure()
     {
         if (lineSnapper != null)
         {
             if (lineSnapper.lineCount >= 1)
                 lineSnapper.value2 = "???";
             if (lineSnapper.lineCount < 1)
                 lineSnapper.value1 = "???";
         }

         if (gameStateManager.IsDoneMeasuring)
         {
             StartCoroutine(SlideOCRBoard(false));
             Invoke(nameof(ToggleLineDelay), OCRSLIDETIME);
         }

         if (textFinish != null) textFinish.text = castBtnText1;
         if (calcBtnObj != null && calcBtnObj.activeInHierarchy)
             calcBtnObj.SetActive(false);
         gameStateManager.IsDoneMeasuring = false;
     }
 */
    private void ToggleLineDelay()
    {
        lineSnapper?.ToggleLineText();
    }

    public void OnBackspacePressed()
    {
        fa?.BackspaceInput();
        ocrScript?.ResetColor();
        ocrScript?.ResetVFX();
    }

    public void InputAnswer(float ans = 0f)
    {
        inputAnswer = ans;
        ocrManager.Deactivate();
        CalcError();

        if (correctionPerc != null)
        {
            stringBuilder.Clear();
            stringBuilder.Append("Error: ");
            stringBuilder.Append(Math.Round(Math.Abs(error), 2));
            stringBuilder.Append("%");
            correctionPerc.text = stringBuilder.ToString();
            correctionPerc.gameObject.SetActive(true);
        }

        // NEW: Add configurable extra delay after fill
        float totalDelay = FILLTIMEAPROX + OCRSLIDETIME + extraFillDelay;
        Invoke(nameof(CallCastAnimation), totalDelay);
    }
    /*
    public void InputAnswer(float ans = 0f)
    {
        inputAnswer = ans;

        // DELETED: The old, direct UI manipulation calls are now obsolete.
        // toggleDialogueBox();
        // HideNewUI();
        // pDiaButtons?.SetActive(false);
        // StartCoroutine(SlideOCRBoard(false));

        // NEW: Deactivate the OCR phase using the dedicated manager.
        // This single call now handles hiding the OCR board and its related UI elements cleanly.
        ocrManager.Deactivate();

        // This part remains the same: calculate the player's accuracy.
        CalcError();

        // This part remains the same: display the error percentage to the player.
        if (correctionPerc != null)
        {
            stringBuilder.Clear();
            stringBuilder.Append("Error: ");
            stringBuilder.Append(Math.Round(Math.Abs(error), 2));
            stringBuilder.Append("%");
            correctionPerc.text = stringBuilder.ToString();
            correctionPerc.gameObject.SetActive(true);
        }

        // NEW: Notify the GameStateManager that the OCR/Input phase is complete.
        // This is the critical step that allows the PhaseManager to advance the game
        // to the 'Complete' state, which in turn triggers the transition to the next problem.
        //gameStateManager.OnOCRInputCompleted();

        // This part remains the same: it schedules the final spell casting animation sequence.
        Invoke(nameof(CallCastAnimation), FILLTIMEAPROX + OCRSLIDETIME);
    }*/


    #endregion

    #region Error calculation and end-game
    private void CalcError()
    {
        if (spellCastEvent == null || shapeFiller == null || soundPlayer == null) return;

        float clamped = spellCastEvent.GetFillPercentage();
        shapeFiller.fillMaxValue = clamped;
        shapeFiller.isFillingActive = true;

        if (clamped > 2.0f)
            clamped = 2.0f;

        error = (1 - clamped) * 100f;

        // FIX: Always use normal pitch (1.0f), don't modify it
        int sfxIndex = (error == 0f) ? 2 : 1;
        soundPlayer.PlaySFX(sfxIndex, 1.0f, 1.0f);  // volume=1, pitch=1
    }
    private void ToClass()
    {
        animScript?.VideoPlayerScript?.Stop();

        cp = correctionPerc != null && correctionPerc.IsActive();
        ls = lineSnapper != null && lineSnapper.gameObject.activeSelf;

        if (correctionPerc != null) correctionPerc.gameObject.SetActive(false);
        if (lineSnapper != null) lineSnapper.gameObject.SetActive(false);

        mainCamera?.SetActive(false);
        classroomCamera?.SetActive(true);
    }

    private void ToUI()
    {
        animScript?.VideoPlayerScript?.Stop(); // Keep this commented out or remove it to prevent interference
        animScript?.VideoPlayerScript?.PlayBGAnim(); // This might be causing issues, let's disable it during transitions.

        if (correctionPerc != null) correctionPerc.gameObject.SetActive(cp);
        if (lineSnapper != null) lineSnapper.gameObject.SetActive(ls);

        // This is the most important part: ensure the main camera is on and the other is off.
        if (mainCamera != null) mainCamera.SetActive(true);
        if (classroomCamera != null) classroomCamera.SetActive(false);
        Debug.Log("Switched to Main Camera (ToUI).");
    }

    private IEnumerator DelayedCastAnimation()
    {
        yield return new WaitForSeconds(TRANSITIONTIME + 0.1f);

        HideNewUI();

        int state = (error > 0) ? 0 : (error < 0) ? 1 : 2;
        animScript.VideoPlayerScript.PlaySpellAnim(currentShape, state);

        yield return new WaitUntil(() => animScript.VideoPlayerScript.videoPlayer.isPrepared);

        float len = animScript.VideoPlayerScript.GetVideoLength();
        float sd = state == 2 ? len : 0f;

        yield return new WaitForSeconds(sd + ENDDELAY);

        FadeDelay();

        yield return new WaitForSeconds(1f);

        EndGameFunctions();
    }

    private void FadeDelay()
    {
        screenFadeAnimator?.SetTrigger("fadeOut");
    }

    /*    private void EndGameFunctions()
        {
            bool isWin = error == 0f;
            GlobalVariables.playerWin = isWin;

            if (isWin)
            {
                GlobalVariables.percent = (GlobalVariables.level < 3) ? 0f : 1f;
            }
            else
            {
                GlobalVariables.percent = Mathf.Clamp01(1f - Mathf.Abs(error) * 0.01f);
            }

            GlobalVariables.gameFinished = true;
            GlobalVariables.isLOGame = true;

            SceneManager.LoadScene("LevelSelect");
        }*/
    private void EndGameFunctions()
    {
        bool isWin = error == 0f;
        GlobalVariables.playerWin = isWin;

        if (isWin)
        {
            GlobalVariables.percent = (GlobalVariables.level < 3) ? 0f : 1f;
        }
        else
        {
            GlobalVariables.percent = Mathf.Clamp01(1f - Mathf.Abs(error) * 0.01f);
        }

        GlobalVariables.gameFinished = true;
        GlobalVariables.isLOGame = true;

        // MOVED HERE: Now that animation is complete, trigger cleanup and next problem
        gameStateManager.OnOCRInputCompleted();

        // REMOVED: SceneManager.LoadScene("LevelSelect");
        // REPLACED WITH: Placeholder for classmates to implement
        OnProblemComplete();
    }

    private void OnProblemComplete()
    {
       
        //screenFadeAnimator?.SetTrigger("sceneOut");
        Debug.Log("Problem complete. Waiting for level transition implementation...");
        ShowPostProblemStatistics();

    }

    private void CallCastAnimation()
    {
        screenFadeAnimator?.SetTrigger("fade");
        Invoke(nameof(ToClass), TRANSITIONTIME);
        StartCoroutine(DelayedCastAnimation());
    }
    #endregion

    #region Coroutines: movement & UI transitions
    public IEnumerator RectTransformOverTime(RectTransform rt, float duration, Vector2 endTransform)
    {
        if (rt == null) yield break;

        Vector2 startTransform = rt.anchoredPosition;
        float elapsed = 0f;
        float invDuration = 1f / duration;

        while (elapsed < duration)
        {
            float t = elapsed * invDuration;
            rt.anchoredPosition = Vector2.Lerp(startTransform, endTransform, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rt.anchoredPosition = endTransform;
    }

    public IEnumerator MoveOverTime(GameObject obj, float duration, Vector3 endPosition)
    {
        if (obj == null) yield break;

        Vector3 startPosition = obj.transform.position;
        float elapsed = 0f;
        float invDuration = 1f / duration;

        while (elapsed < duration)
        {
            float t = elapsed * invDuration;
            obj.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = endPosition;
    }

    public IEnumerator LocalScaleOverTime(GameObject obj, float duration, Vector3 endScale)
    {
        if (obj == null) yield break;

        Vector3 startScale = obj.transform.localScale;
        float elapsed = 0f;
        float invDuration = 1f / duration;

        while (elapsed < duration)
        {
            float t = elapsed * invDuration;
            obj.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.localScale = endScale;
    }

    /*private IEnumerator SlideOCRBoard(bool show)
    {
        if (show)
        {
            toggleDialogueBox();
            yield return dialogueWait;

            ocrInput?.SetActive(true);
            if (ocrEndTransform != null) StartCoroutine(MoveOverTime(ocrInput, OCRSLIDETIME, ocrEndTransform.position));
            if (rtDialogue != null) StartCoroutine(RectTransformOverTime(rtDialogue, OCRSLIDETIME, ocrDialoguePos));
            if (pDialogue != null) StartCoroutine(LocalScaleOverTime(pDialogue, OCRSLIDETIME, smallScale));
        }
        else
        {
            if (ocrScript != null) ocrScript.processing = true;
            if (formulaDisplay != null) formulaDisplay.SetActive(false);

            if (ocrStartTransform != null) StartCoroutine(MoveOverTime(ocrInput, OCRSLIDETIME, ocrStartTransform.position));
            if (rtDialogue != null) StartCoroutine(RectTransformOverTime(rtDialogue, OCRSLIDETIME, origDiaRT));
            if (pDialogue != null) StartCoroutine(LocalScaleOverTime(pDialogue, OCRSLIDETIME, normalScale));
        }

        yield return ocrWait;

        if (show)
        {
            if (ocrScript != null) { ocrScript.ResetColor(); ocrScript.ResetVFX(); ocrScript.processing = false; }
            if (formulaDisplay != null) formulaDisplay.SetActive(true);
            ShowHint(3);
            if (characterSay != null) characterSay.text = "";
        }
        else
        {
            if (ocrInput != null) ocrInput.SetActive(false);
            toggleDialogueBox();
        }
    }
    */
    #endregion

    #region Problem creation & Spell classes
    public class Problem
    {
        public SHAPES problemShape;
        public float p_measure = UNUSED;
        public float s_measure = UNUSED;
        private float offX = 0, offY = 0;
        private const float LVL3XOFF = 1.75f;
        private const float LVL3YOFF = 1.75f;

        private const int minLimitXY = 3;
        private const int limitXY = 8;

        public GameBehaviour main;
        public GameObject problemObjectShape;

        private static readonly System.Random staticRand = new System.Random((int)DateTime.Now.Ticks);

        public Problem(SHAPES shape, GameBehaviour main, float x = -1, float y = -1)
        {
            this.main = main;
            this.problemShape = shape;

            if (x == -1 && y == -1)
            {
                // Intentionally minimal logging after cleanup
                GenerateRandomProblem();
            }
            else
            {
                GenerateManualProblem(x, y);
            }
        }

        private void GenerateRandomProblem()
        {
            switch (problemShape)
            {
                case SHAPES.SQUARE:
                    p_measure = staticRand.Next(minLimitXY, limitXY);
                    problemObjectShape = main.shapeGenerator.CreateSquare(Vector2.zero, p_measure);
                    break;
                case SHAPES.TRIANGLE:
                    p_measure = staticRand.Next(minLimitXY, limitXY);
                    s_measure = staticRand.Next(minLimitXY, limitXY);
                    problemObjectShape = main.shapeGenerator.CreateTriangle(Vector2.zero, p_measure, s_measure);
                    break;
                case SHAPES.CIRCLE:
                    p_measure = staticRand.Next(minLimitXY, limitXY);
                    problemObjectShape = main.shapeGenerator.CreateCircle(Vector2.zero, p_measure, false);
                    break;
                case SHAPES.RECTANGLE:
                    do
                    {
                        p_measure = staticRand.Next(minLimitXY, limitXY);
                        s_measure = staticRand.Next(minLimitXY, limitXY);
                    } while (p_measure == s_measure);
                    problemObjectShape = main.shapeGenerator.CreateRectangle(Vector2.zero, p_measure, s_measure);
                    break;
                case SHAPES.SEMI_CIRCLE:
                    p_measure = staticRand.Next(minLimitXY, limitXY);
                    problemObjectShape = main.shapeGenerator.CreateCircle(Vector2.zero, p_measure, true);
                    break;
            }
        }

        private void GenerateManualProblem(float x, float y)
        {
            p_measure = x;
            s_measure = y;

            Vector2 offset = new Vector2(offX, offY);

            switch (problemShape)
            {
                case SHAPES.SQUARE:
                    problemObjectShape = main.shapeGenerator.CreateSquare(offset, p_measure);
                    break;
                case SHAPES.TRIANGLE:
                    problemObjectShape = main.shapeGenerator.CreateTriangle(offset, p_measure, s_measure);
                    break;
                case SHAPES.CIRCLE:
                    problemObjectShape = main.shapeGenerator.CreateCircle(Vector2.zero, p_measure, false);
                    break;
                case SHAPES.RECTANGLE:
                    problemObjectShape = main.shapeGenerator.CreateRectangle(offset, p_measure, s_measure);
                    break;
                case SHAPES.SEMI_CIRCLE:
                    problemObjectShape = main.shapeGenerator.CreateCircle(Vector2.zero, p_measure, true);
                    break;
            }
        }
    }

    public class SpellCastEvent
    {
        public GameBehaviour main;
        public Problem problem;

        private double p_measure = UNUSED;
        private double s_measure = UNUSED;

        public SpellCastEvent(GameBehaviour behavior, Problem prob)
        {
            main = behavior;
            problem = prob;
            p_measure = problem.p_measure;
            s_measure = problem.s_measure;
        }

        private const double HalfPI = Math.PI * 0.5;

        public float GetFillPercentage()
        {
            double result = CalculateArea();
            float compX = (float)Math.Round(result, 2);
            float compY = main.inputAnswer;

            return compY / compX;
        }

        private double CalculateArea()
        {
            switch (problem.problemShape)
            {
                case SHAPES.TRIANGLE:
                    return 0.5 * p_measure * s_measure;
                case SHAPES.CIRCLE:
                    double radius = p_measure * 0.5;
                    return Math.PI * radius * radius;
                case SHAPES.RECTANGLE:
                    return p_measure * s_measure;
                case SHAPES.SQUARE:
                    return p_measure * p_measure;
                case SHAPES.SEMI_CIRCLE:
                    double semiRadius = p_measure * 0.5;
                    return HalfPI * semiRadius * semiRadius;
                default:
                    throw new Exception("Invalid shape");
            }
        }
    }
    #endregion

    #region Wait & Problem init (merged Reset flow called from Start)
    private void StartLevelAnim()
    {
        screenFadeAnimator?.SetTrigger("fade");
        Invoke(nameof(ToUI), TRANSITIONTIME);
        Invoke(nameof(ShowNewUI), TRANSITIONTIME);
        Invoke(nameof(RemoveRoomText), TRANSITIONTIME);
        Invoke(nameof(PlayMusic), TRANSITIONTIME);
    }

    private void PlayMusic()
    {
        soundPlayer?.PlayBGM(0, 1, 0.4f);
    }

    private void HideNewUI()
    {
        GameObject[] uiElements = { hud, pDialogue, panelMagicScroll, quickMenu };
        for (int i = 0; i < uiElements.Length; i++)
            if (uiElements[i] != null) uiElements[i].SetActive(false);

        if (calcBtnObj != null) 
            calcBtnObj.SetActive(false);

        // CRITICAL: Ensure ScreenFade animator stays active for transitions
        if (screenFadeAnimator != null && !screenFadeAnimator.gameObject.activeInHierarchy)
        {
            screenFadeAnimator.gameObject.SetActive(true);
            Debug.Log("HideNewUI: Forced ScreenFade to stay active");
        }
    }

    private void ShowNewUI()
    {
        // PHASE 1: Don't show shape selection UI
        if (gameStateManager.MeasurementPhaseActive)
        {
            Debug.Log("Phase 1 active - skipping shape selection UI");
            // Only show essential UI, NOT dialogue/shape selection
            if (hud != null) hud.SetActive(true);
            if (quickMenu != null) quickMenu.SetActive(true);
            UpdateVariableDisplay();
            return;
        }

        GameObject[] uiElements = { hud, pDialogue, panelMagicScroll, quickMenu };
        for (int i = 0; i < uiElements.Length; i++)
            if (uiElements[i] != null) uiElements[i].SetActive(true);

        if (variableDisplayManager != null && variableDisplayManager.primaryText != null)
        {
            // Call UpdateDisplay to show the variables with current state
            if (currentVariables != null && guidesCompleted != null && measuredValues != null)
            {
                variableDisplayManager.UpdateDisplay(currentVariables, guidesCompleted, measuredValues);
            }
        }
    }

    private void RemoveRoomText()
    {
        hud?.SetActive(false);
    }

    private void SetManualProblem(SHAPES shape, float x, float y = 1, int setSeed = -1)
    {
        currentShape = shape;
        Problem problem = new Problem(shape, this, x, y);

        double result = CalculateShapeArea(problem);
        Debug.Log($"Result: {Math.Round(result, 2)}");

        spellCastEvent = new SpellCastEvent(this, problem);
        ActivateSpell(currentShape);
    }

    private double CalculateShapeArea(Problem problem)
    {
        switch (problem.problemShape)
        {
            case SHAPES.TRIANGLE:
                return 0.5 * problem.p_measure * problem.s_measure;
            case SHAPES.CIRCLE:
                double radius = problem.p_measure * 0.5;
                return Math.PI * radius * radius;
            case SHAPES.RECTANGLE:
                return problem.p_measure * problem.s_measure;
            case SHAPES.SQUARE:
                return problem.p_measure * problem.p_measure;
            case SHAPES.SEMI_CIRCLE:
                double semiRadius = problem.p_measure * 0.5;
                return 0.5 * Math.PI * semiRadius * semiRadius;
            default:
                throw new Exception("Invalid shape");
        }
    }

    public void generateProblem()
    {
        System.Random random = new System.Random((int)DateTime.Now.Ticks);
        SHAPES randomShape = (SHAPES)random.Next(1, Enum.GetValues(typeof(SHAPES)).Length);

        currentShape = randomShape;
        Problem problem = new Problem(randomShape, this);

        double result = CalculateShapeArea(problem);
        spellCastEvent = new SpellCastEvent(this, problem);
        ActivateSpell(currentShape);
    }

    private void InitFillShape()
    {
        if (shapeFiller != null && spellCastEvent?.problem?.problemObjectShape != null)
            shapeFiller.InitializeFill(spellCastEvent.problem.problemObjectShape, Color.green, 0.5f, 0f);
    }

    // Wrapper method for level manager
    private void CreateLevelProblemWrapper()
    {
        levelManager.CreateLevelProblem();
    }



    #endregion


    /*    public void DelayedGameOverReset()
        {
            Debug.Log("=== DelayedGameOverReset called ===");

            // Reset game state first
            gameStateManager.ResetForSameProblem();

            // Reset measurement state
            completedMeasurements = 0;

            // Clean up any existing guides
            CleanupGuides();

            // Reset LineSnapper
            if (lineSnapper != null)
            {
                lineSnapper.OnUndoPressed();
                lineSnapper.OnUndoPressed();
                lineSnapper.ForceInitialize(); // Make sure it's properly initialized
                lineSnapper.gameObject.SetActive(true);
                lineSnapper.enabled = true;
            }

            // Go back to measurement phase through phase manager
            if (phaseManager != null)
            {
                phaseManager.RestartPhases();
            }
            else
            {
                // Fallback if phase manager isn't available
                gameStateManager.SetMeasurementPhase(true);
            }

            // Setup measurement phase again
            SetupGuidesForShape(currentShape);
            UpdateVariableDisplay();
            UpdateScoreDisplay();
            UpdateLivesDisplay();

            // Clear any feedback messages
            ShowMeasurementFeedback("");

            Debug.Log("Game over reset complete - back to measurement phase");
        }
    */
    public static class GameStatePreserver
    {
        // The level we should be on when the game scene loads.
        // It defaults to 0 (Level 1) for the very first time the game starts.
        public static int LevelToLoad = 0;

        public static int ProblemToLoad = 0;        // NEW: Which problem to start at (0-4)
        public static int Difficulty = 1;
        public static int StartingLives = 4;
        public static bool ShowVariableHints = true;
    }

    // MODIFIED: This class is now fully implemented to handle transitions.
    // MODIFIED: This class is now fully implemented to handle transitions.
    // MODIFIED: This class is now fully implemented to handle transitions.
    [System.Serializable]
    public class TransitionManagerClass
    {
        [HideInInspector] public GameBehaviour main;

        // The time it takes for the screen to fade in or out.
        // Ensure this matches your "sceneOut" and "fadeIn" animation durations.
        private const float FADE_TIME = 0.5f;

        public void Initialize(GameBehaviour gameMain)
        {
            main = gameMain;
            Debug.Log("Transition Manager: Initialized.");
        }

        /// <summary>
        /// Starts the complete, safe transition sequence to the next problem.
        /// </summary>
        public void BeginTransitionToNextProblem()
        {
            Debug.Log("Transition Manager: Beginning transition to next problem...");
            main.StartCoroutine(TransitionCoroutine());
        }

        /// <summary>
        /// A coroutine that manages the entire fade-out, setup, and fade-in sequence reliably.
        /// </summary>
        private IEnumerator TransitionCoroutine()
        {
            Debug.Log("=== INSTANT TRANSITION START ===");

            // No fade out - immediate transition
            Debug.Log("=== STARTING CLEANUP ===");
            CleanupOldProblem();
            Debug.Log("=== CLEANUP COMPLETE ===");

            Debug.Log("=== STARTING NEXT PROBLEM ===");
            main.levelManager.StartNextProblem();
            Debug.Log("=== NEXT PROBLEM CREATED ===");

            Debug.Log("=== FINALIZING TRANSITION ===");
            main.FinalizeTransition();
            Debug.Log("=== TRANSITION FINALIZED ===");

            yield return null; // Just wait one frame

            Debug.Log("=== INSTANT TRANSITION COMPLETE ===");
        }

        /// <summary>
        /// Performs all necessary cleanup of the previous problem's state.
        /// </summary>
        public void CleanupOldProblem()
        {
            Debug.Log("Transition Manager: Cleaning up old problem assets.");

            if (main.spellCastEvent?.problem?.problemObjectShape != null)
            {
                GameObject.Destroy(main.spellCastEvent.problem.problemObjectShape);
            }

            main.CleanupGuides();

            if (main.lineSnapper != null)
            {
                main.lineSnapper.OnUndoPressed();
                main.lineSnapper.OnUndoPressed();
                main.lineSnapper.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Fades the screen back in to reveal the new, fully loaded problem.
        /// </summary>
        /// <summary>
        /// Fades the screen back in to reveal the new, fully loaded problem.
        /// </summary>
        public void RevealNewProblem()
        {
            /*Debug.Log("Transition Manager: Fading in to reveal new problem.");
            if (main.screenFadeAnimator != null)
            {
                // CRITICAL FIX: Force the ScreenFade GameObject to be active.
                // This prevents it from being hidden by other UI management scripts like HideNewUI().
                if (!main.screenFadeAnimator.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning("ScreenFade object was inactive. Forcing it active now.");
                    main.screenFadeAnimator.gameObject.SetActive(true);
                }

                main.screenFadeAnimator.SetTrigger("fadeIn");
            }
            else
            {
                Debug.LogError("FATAL: screenFadeAnimator is NULL. Cannot fade in!");
            }*/
        }

        // In TransitionManagerClass

        /// <summary>
        /// Starts the complete, safe transition sequence to the NEXT LEVEL.
        /// </summary>
        public void BeginTransitionToNextLevel()
        {
            Debug.Log("Transition Manager: Beginning transition to NEXT LEVEL...");
            main.StartCoroutine(TransitionToNextLevelCoroutine());
        }

        private IEnumerator TransitionToNextLevelCoroutine()
        {
            Debug.Log("Transition Coroutine (Next Level): Fading out.");
            if (main.screenFadeAnimator != null)
            {
                main.screenFadeAnimator.SetTrigger("sceneOut");
            }

            yield return new WaitForSeconds(FADE_TIME);

            Debug.Log("Transition Coroutine (Next Level): Screen is black. Cleaning up and starting next level.");

            CleanupOldProblem();
            main.levelManager.StartNextLevel();
            main.FinalizeTransition();

            yield return new WaitForSeconds(0.1f);

            Debug.Log("Transition Coroutine (Next Level): Fading in.");
            RevealNewProblem();
        }


    }
    public void FinalizeTransition()
    {
        Debug.Log("Finalizing transition: Switching to UI camera and showing HUD.");

        // Explicitly switch from the classroom/casting camera back to the main UI camera.
        ToUI();

        // Ensure essential UI elements are visible for the new problem.
        if (hud != null) hud.SetActive(true);
        if (quickMenu != null) quickMenu.SetActive(true);
    }

    [System.Serializable]
    // In GameBehaviour.cs
    public class VariableDisplayManager
    {
        [Header("Measurement Display Texts")]
        [Tooltip("Shows primary measurement (S, L, B, or D)")]
        public Text primaryText;

        [Tooltip("Shows secondary measurement (W or H) - only for Rectangle/Triangle")]
        public Text secondaryText;

        private GameBehaviour main;

        public void Initialize(GameBehaviour gameMain)
        {
            main = gameMain;
            if (primaryText != null) primaryText.gameObject.SetActive(false);
            if (secondaryText != null) secondaryText.gameObject.SetActive(false);
        }

        /// <summary>
        /// The single source of truth for updating the variable display UI.
        /// It correctly handles shapes with one or two variables.
        /// </summary>
        public void UpdateDisplay(string[] variableNames, bool[] completed, float[] values)
        {
            if (variableNames == null || completed == null || values == null) 
                return;

            if (main.GetDifficulty() == 4 || !GameBehaviour.GameStatePreserver.ShowVariableHints)
            {
                if (primaryText != null) primaryText.gameObject.SetActive(false);
                if (secondaryText != null) secondaryText.gameObject.SetActive(false);
                Debug.Log("Variable displays hidden (Difficulty 4 or setting disabled)");
                return;
            }

            // Ensure the array lengths match to prevent errors.
            if (variableNames.Length != completed.Length || variableNames.Length != values.Length)
            {
                Debug.LogError("Variable display array lengths do not match!");
                return;
            }

            // Handle the primary measurement text (always exists)
            if (primaryText != null && variableNames.Length > 0)
            {
                primaryText.gameObject.SetActive(true);
                primaryText.text = completed[0]
                    ? $"{variableNames[0]} = {values[0]:F1}"
                    : $"{variableNames[0]} = ?";
            }

            // Handle the secondary measurement text (only for shapes like rectangles/triangles)
            if (secondaryText != null)
            {
                if (variableNames.Length > 1)
                {
                    secondaryText.gameObject.SetActive(true);
                    secondaryText.text = completed[1]
                        ? $"{variableNames[1]} = {values[1]:F1}"
                        : $"{variableNames[1]} = ?";
                }
                else
                {
                    // Hide the secondary text for shapes that don't need it (Square, Circle).
                    secondaryText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Updates the UI with a live preview of the measurement as the user drags their mouse.
        /// </summary>
        public void OnMeasurementPreview(int lineIndex, float currentValue, Vector3 startPos, Vector3 endPos)
        {
            if (main == null || !main.gameStateManager.MeasurementPhaseActive || main.currentVariables == null) return;

            // 1. Create temporary copies of the current measurement state.
            bool[] previewCompleted = (bool[])main.guidesCompleted.Clone();
            float[] previewValues = (float[])main.measuredValues.Clone();

            // 2. **FIX: Determine the guide index based on the ORIENTATION of the drawn line**
            int likelyGuideIndex = DetermineGuideIndexByOrientation(startPos, endPos);

            // 3. If a valid guide is found, update its state in our temporary arrays.
            if (likelyGuideIndex != -1 && likelyGuideIndex < previewValues.Length)
            {
                previewValues[likelyGuideIndex] = currentValue;
                previewCompleted[likelyGuideIndex] = true;
            }

            // 4. Call the main UpdateDisplay method with our temporary "preview" state.
            UpdateDisplay(main.currentVariables, previewCompleted, previewValues);
        }

        /// <summary>
        /// NEW: Determines which guide index the drawn line represents based on its orientation.
        /// This ensures the live preview always shows the correct variable (W/L for rectangles, B/H for triangles).
        /// </summary>
        private int DetermineGuideIndexByOrientation(Vector3 drawnStart, Vector3 drawnEnd)
        {
            if (main.guideLines == null || main.guidesCompleted == null) return -1;

            // Calculate if the drawn line is horizontal or vertical
            float deltaX = Mathf.Abs(drawnEnd.x - drawnStart.x);
            float deltaY = Mathf.Abs(drawnEnd.y - drawnStart.y);
            bool drawnIsHorizontal = deltaX > deltaY;

            // Find the first uncompleted guide that matches this orientation
            for (int i = 0; i < main.guideLines.Length; i++)
            {
                if (main.guideLines[i] == null || main.guidesCompleted[i])
                    continue; // Skip completed or null guides

                // Check the orientation of this guide
                Vector3 guideStart = main.guideLines[i].GetPosition(0);
                Vector3 guideEnd = main.guideLines[i].GetPosition(1);
                float guideDeltaX = Mathf.Abs(guideEnd.x - guideStart.x);
                float guideDeltaY = Mathf.Abs(guideEnd.y - guideStart.y);
                bool guideIsHorizontal = guideDeltaX > guideDeltaY;

                // If orientations match, this is the guide being measured
                if (drawnIsHorizontal == guideIsHorizontal)
                {
                    return i;
                }
            }

            // Fallback: return the first uncompleted guide if no orientation match
            for (int i = 0; i < main.guidesCompleted.Length; i++)
            {
                if (!main.guidesCompleted[i])
                {
                    return i;
                }
            }

            return -1;
        }
    }
    private int FindClosestGuideByPosition(Vector3 drawnStart, Vector3 drawnEnd)
    {
        if (guideLines == null) return -1;

        for (int i = 0; i < guideLines.Length; i++)
        {
            if (guideLines[i] == null || (guidesCompleted != null && guidesCompleted[i]))
                continue; // Skip completed or null guides

            // ValidateLinePositionForGuide checks if the drawn line is on top of the guide line.
            if (ValidateLinePositionForGuide(i, drawnStart, drawnEnd))
            {
                return i; // Return the first uncompleted guide that matches positionally
            }
        }
        return -1; // No positional match found for any uncompleted guide
    }
    public void ResetOCRInput()
    {
        if (ocrScript != null)
        {
            ocrScript.processing = false;
            ocrScript.ResetColor();
            ocrScript.ResetVFX();
        }

        if (formulaDisplay != null)
        {
            formulaDisplay.SetActive(true);
        }

        if (backspaceButton != null)
        {
            backspaceButton.SetActive(true);
        }

        Debug.Log("OCR Input reset and re-enabled for new input");
    }

    public void OnFormulaInputError(string errorType)
    {
        Debug.Log($"Formula input error: {errorType}");

        switch (errorType)
        {
            case "invalid":
                NotifyInvalidFormula();
                break;
            case "mismatched":
                NotifyMismatchedAnswer();
                break;
            default:
                // Generic error - just reset
                ResetOCRInput();
                break;
        }
    }


    /// <summary>
    /// Configuration builder for GameBehaviour
    /// Usage examples:
    ///   GameConfiguration.Create().SetLevel(SHAPES.SQUARE, 2).SetDifficulty(3).LoadGame();
    ///   GameConfiguration.Create().SetLevel("Circle", 0).SetDifficulty(2).LoadGame();
    ///   GameConfiguration.Create().SetLevel(SHAPES.TRIANGLE).LoadGame(); // defaults to problem 0
    /// </summary>
    public class GameConfiguration
    {
        private int levelIndex = 0;        // Which world (0-4)
        private int problemIndex = 0;      // Which problem within that world (0-4)
        private int difficulty = 1;
        private int startLives = 4;
        private bool showHints = true;

        // Private constructor - force use of Create()
        private GameConfiguration() { }

        /// <summary>
        /// Start building configuration
        /// </summary>
        public static GameConfiguration Create()
        {
            return new GameConfiguration();
        }

        /// <summary>
        /// Set level using SHAPES enum (e.g., SHAPES.SQUARE)
        /// </summary>
        public GameConfiguration SetLevel(GameBehaviour.SHAPES shape, int problemNumber = 0)
        {
            this.levelIndex = ShapeToLevelIndex(shape);
            this.problemIndex = Mathf.Clamp(problemNumber, 0, 4);
            return this;
        }

        /// <summary>
        /// Set level using string name (e.g., "Square", "Circle")
        /// Case-insensitive
        /// </summary>
        public GameConfiguration SetLevel(string shapeName, int problemNumber = 0)
        {
            GameBehaviour.SHAPES shape = StringToShape(shapeName);
            return SetLevel(shape, problemNumber);
        }

        /// <summary>
        /// Set difficulty (1-4)
        /// </summary>
        public GameConfiguration SetDifficulty(int diff)
        {
            this.difficulty = Mathf.Clamp(diff, 1, 4);
            return this;
        }

        /// <summary>
        /// Set starting lives
        /// </summary>
        public GameConfiguration SetLives(int lives)
        {
            this.startLives = Mathf.Max(1, lives);
            return this;
        }

        /// <summary>
        /// Show/hide variable hints (L=?, W=?, etc.)
        /// </summary>
        public GameConfiguration SetShowHints(bool show)
        {
            this.showHints = show;
            return this;
        }

        /// <summary>
        /// Apply configuration and load the game scene
        /// </summary>
        public void LoadGame()
        {
            // Store in static class so GameBehaviour can read it
            GameBehaviour.GameStatePreserver.LevelToLoad = this.levelIndex;
            GameBehaviour.GameStatePreserver.ProblemToLoad = this.problemIndex;
            GameBehaviour.GameStatePreserver.Difficulty = this.difficulty;
            GameBehaviour.GameStatePreserver.StartingLives = this.startLives;
            GameBehaviour.GameStatePreserver.ShowVariableHints = this.showHints;

            Debug.Log($"Loading game: Level={levelIndex}, Problem={problemIndex}, Difficulty={difficulty}");

            // Load the game scene
            SceneManager.LoadScene("LoadingScreen");
        }

        /// <summary>
        /// Apply configuration without loading (for testing)
        /// </summary>
        public void Apply()
        {
            GameBehaviour.GameStatePreserver.LevelToLoad = this.levelIndex;
            GameBehaviour.GameStatePreserver.ProblemToLoad = this.problemIndex;
            GameBehaviour.GameStatePreserver.Difficulty = this.difficulty;
            GameBehaviour.GameStatePreserver.StartingLives = this.startLives;
            GameBehaviour.GameStatePreserver.ShowVariableHints = this.showHints;
        }

        // ============================================
        // HELPER METHODS - Convert between formats
        // ============================================

        /// <summary>
        /// Convert SHAPES enum to level index (0-4)
        /// </summary>
        private int ShapeToLevelIndex(GameBehaviour.SHAPES shape)
        {
            switch (shape)
            {
                case GameBehaviour.SHAPES.SQUARE:
                    return 0;
                case GameBehaviour.SHAPES.RECTANGLE:
                    return 1;
                case GameBehaviour.SHAPES.CIRCLE:
                    return 2;
                case GameBehaviour.SHAPES.TRIANGLE:
                    return 3;
                case GameBehaviour.SHAPES.SEMI_CIRCLE:
                    return 4;
                default:
                    Debug.LogWarning($"Unknown shape: {shape}, defaulting to Square (0)");
                    return 0;
            }
        }

        /// <summary>
        /// Convert string to SHAPES enum
        /// Accepts: "Square", "SQUARE", "square", etc.
        /// </summary>
        private GameBehaviour.SHAPES StringToShape(string shapeName)
        {
            string normalized = shapeName.Trim().ToUpperInvariant();

            switch (normalized)
            {
                case "SQUARE":
                    return GameBehaviour.SHAPES.SQUARE;

                case "RECTANGLE":
                case "RECT":
                    return GameBehaviour.SHAPES.RECTANGLE;

                case "CIRCLE":
                    return GameBehaviour.SHAPES.CIRCLE;

                case "TRIANGLE":
                case "TRI":
                    return GameBehaviour.SHAPES.TRIANGLE;

                case "SEMICIRCLE":
                case "SEMI_CIRCLE":
                case "SEMI CIRCLE":
                case "SEMI-CIRCLE":
                    return GameBehaviour.SHAPES.SEMI_CIRCLE;

                default:
                    Debug.LogWarning($"Unknown shape name: '{shapeName}', defaulting to SQUARE");
                    return GameBehaviour.SHAPES.SQUARE;
            }
        }
    }

    private GameStatistics.GameContext GetCurrentGameContext()
    {
        int difficulty = GetDifficulty();
        int levelIndex = levelManager.CurrentLevel;
        int problemIndex = levelManager.CurrentProblemIndex;
        string shapeName = currentShape.ToString();
        int lives = gameStateManager.CurrentLives;
        int score = gameStateManager.CurrentScore;

        return new GameStatistics.GameContext(difficulty, levelIndex, problemIndex, shapeName, lives, score);
    }

    /// <summary>
    /// Display comprehensive statistics after problem completion
    /// Call this after a problem or level is completed
    /// </summary>
    private void ShowPostProblemStatistics()
    {
        Debug.Log("========================================");
        Debug.Log("    POST-PROBLEM STATISTICS REPORT    ");
        Debug.Log("========================================");

        // Current session stats
        Debug.Log("\n--- THIS SESSION ---");
        Debug.Log($"Game Overs: {GameStatistics.Instance.GetSessionGameOvers()}");
        Debug.Log($"Lives Lost: {GameStatistics.Instance.GetSessionLivesLost()}");
        Debug.Log($"Problems Completed: {GameStatistics.Instance.GetData().currentSessionProblemsCompleted}");
        Debug.Log($"Correct Answers: {GameStatistics.Instance.GetData().currentSessionCorrectAnswers}");
        Debug.Log($"Wrong Answers: {GameStatistics.Instance.GetData().currentSessionWrongAnswers}");

        float sessionAccuracy = 0f;
        int sessionTotal = GameStatistics.Instance.GetData().currentSessionCorrectAnswers +
                           GameStatistics.Instance.GetData().currentSessionWrongAnswers;
        if (sessionTotal > 0)
        {
            sessionAccuracy = (float)GameStatistics.Instance.GetData().currentSessionCorrectAnswers / sessionTotal * 100f;
        }
        Debug.Log($"Session Accuracy: {sessionAccuracy:F1}%");

        // Current level stats
        Debug.Log("\n--- CURRENT LEVEL ---");
        var currentLevelStats = GameStatistics.Instance.GetLevelStats(levelManager.CurrentLevel);
        if (currentLevelStats != null)
        {
            Debug.Log($"Level: {currentLevelStats.levelName}");
            Debug.Log($"Times Played: {currentLevelStats.timesPlayed}");
            Debug.Log($"Times Completed: {currentLevelStats.timesCompleted}");
            Debug.Log($"Game Overs: {currentLevelStats.gameOvers}");
            Debug.Log($"Lives Lost: {currentLevelStats.livesLost}");
            Debug.Log($"Level Accuracy: {currentLevelStats.GetAccuracy():F1}%");
            Debug.Log($"Best Time: {(currentLevelStats.bestTime < float.MaxValue ? $"{currentLevelStats.bestTime:F2}s" : "N/A")}");
        }

        // Current difficulty stats
        Debug.Log("\n--- CURRENT DIFFICULTY ---");
        var diffStats = GameStatistics.Instance.GetDifficultyStats(GetDifficulty());
        if (diffStats != null)
        {
            Debug.Log($"Difficulty {diffStats.difficulty}:");
            Debug.Log($"Game Overs: {diffStats.gameOvers}");
            Debug.Log($"Lives Lost: {diffStats.livesLost}");
            Debug.Log($"Problems Completed: {diffStats.problemsCompleted}");
            Debug.Log($"Difficulty Accuracy: {diffStats.GetAccuracy():F1}%");
        }

        // Recent events (last 5)
        Debug.Log("\n--- RECENT EVENTS ---");
        var allEvents = GameStatistics.Instance.GetEventHistory();
        int startIndex = Mathf.Max(0, allEvents.Count - 5);
        for (int i = startIndex; i < allEvents.Count; i++)
        {
            var evt = allEvents[i];
            Debug.Log($"[{evt.eventType}] Diff:{evt.context.difficulty} Level:{evt.context.levelIndex} Problem:{evt.context.problemIndex} Lives:{evt.context.currentLives} Score:{evt.context.currentScore}");
        }

        // All-time totals
        Debug.Log("\n--- ALL-TIME TOTALS ---");
        Debug.Log($"Total Game Overs: {GameStatistics.Instance.GetTotalGameOvers()}");
        Debug.Log($"Total Lives Lost: {GameStatistics.Instance.GetTotalLivesLost()}");
        Debug.Log($"Total Problems Completed: {GameStatistics.Instance.GetData().totalProblemsCompleted}");
        Debug.Log($"Total Play Time: {GameStatistics.Instance.GetData().totalPlayTime / 60f:F1} minutes");

        float totalAccuracy = 0f;
        int totalAnswers = GameStatistics.Instance.GetData().totalCorrectAnswers +
                           GameStatistics.Instance.GetData().totalWrongAnswers;
        if (totalAnswers > 0)
        {
            totalAccuracy = (float)GameStatistics.Instance.GetData().totalCorrectAnswers / totalAnswers * 100f;
        }
        Debug.Log($"Overall Accuracy: {totalAccuracy:F1}%");

        Debug.Log("\n========================================\n");
    }
}