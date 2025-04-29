using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FormulaDropdownManager : MonoBehaviour
{
    [SerializeField] private Transform dropdownContainer;
    [SerializeField] private TextMeshProUGUI resultText;

    private List<TMP_Dropdown> formulaDropdowns = new List<TMP_Dropdown>();
    private List<TMP_Dropdown> symbolDropdowns = new List<TMP_Dropdown>();
    private List<TMP_InputField> inputFields = new List<TMP_InputField>();
    private Dictionary<TMP_InputField, string> previousValues = new Dictionary<TMP_InputField, string>();

    // Constants for formulas
    public const string SQUARE_FORMULA = "S*S";
    public const string CIRCLE_FORMULA = "ƒÎ*R*R";
    public const string RECTANGLE_FORMULA = "W*H";
    public const string TRIANGLE_FORMULA = "B*H*1/2";
    public const string SEMI_CIRCLE = "ƒÎ*R*R*1/2";

    public const string ADD_SYMBOL = "+";
    public const string SUB_SYMBOL = "-";

    // List of formula options
    private List<string> formulaOptions = new List<string>()
    {
        "Square Area: " + SQUARE_FORMULA,
        "Circle Area: " + CIRCLE_FORMULA,
        "Rectangle Area: " + RECTANGLE_FORMULA,
        "Triangle Area: " + TRIANGLE_FORMULA,
        "Semi-Circle Area: " + SEMI_CIRCLE
    };

    // List of symbol options
    private List<string> symbolOptions = new List<string>()
    {
        ADD_SYMBOL,
        SUB_SYMBOL
    };

    // Create formula equation with specified number of formulas
    public void SetUpDropdownEquation(int formulaCount)
    {
        if (formulaCount < 1)
        {
            Debug.LogError("Formula count must be at least 1");
            return;
        }

        // Clear existing dropdowns
        ClearDropdowns();

        // Make sure the container has a Vertical Layout Group
        VerticalLayoutGroup layoutGroup = dropdownContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            // Remove any existing layout group
            HorizontalLayoutGroup hlg = dropdownContainer.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null) Destroy(hlg);

            layoutGroup = dropdownContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 20f; // Space between rows
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.padding = new RectOffset(20, 20, 20, 20);
        }

        // Create rows for each formula and symbol
        for (int i = 0; i < formulaCount; i++)
        {
            // Create a horizontal container for this row
            GameObject rowContainer = new GameObject("Row_" + i);
            rowContainer.transform.SetParent(dropdownContainer, false);
            RectTransform rowRect = rowContainer.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(550, 60); // Increased width for input field

            // Add horizontal layout for the row
            HorizontalLayoutGroup rowLayout = rowContainer.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 20f;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = false;

            // Add input field
            TMP_InputField inputField = CreateInputField("InputField_" + i, rowContainer.transform);
            inputFields.Add(inputField);
            previousValues[inputField] = "";

            // Add formula dropdown
            TMP_Dropdown formulaDropdown = CreateDropdown("FormulaDropdown_" + i, 350, rowContainer.transform);
            formulaDropdown.ClearOptions();
            formulaDropdown.AddOptions(formulaOptions);
            formulaDropdowns.Add(formulaDropdown);

            // Add operation symbol (+ or =)
            if (i < formulaCount - 1)
            {
                // Add a symbol dropdown for all except the last formula
                TMP_Dropdown symbolDropdown = CreateDropdown("SymbolDropdown_" + i, 80, rowContainer.transform);
                symbolDropdown.ClearOptions();
                symbolDropdown.AddOptions(symbolOptions);
                symbolDropdowns.Add(symbolDropdown);
            }
            else
            {
                // For the last formula, add "="
                GameObject equalsLabel = new GameObject("EqualsLabel");
                equalsLabel.transform.SetParent(rowContainer.transform, false);
                equalsLabel.AddComponent<RectTransform>().sizeDelta = new Vector2(80, 60);
                TextMeshProUGUI equalsText = equalsLabel.AddComponent<TextMeshProUGUI>();
                equalsText.text = " = ";
                equalsText.fontSize = 48;
                equalsText.alignment = TextAlignmentOptions.Center;
                equalsText.color = Color.black;
            }
        }

        // Set up event listeners for value changes
        SetupEventListeners();
    }

    private TMP_InputField CreateInputField(string name, Transform parent)
    {
        // Create input field GameObject
        GameObject inputObj = new GameObject(name);
        inputObj.transform.SetParent(parent, false);

        // Add RectTransform with specific size
        RectTransform rectTransform = inputObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 60);

        // Add Image component as background
        Image background = inputObj.AddComponent<Image>();
        background.color = new Color(0.95f, 0.95f, 0.95f);

        // Create text area for input
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputObj.transform, false);
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(10, 5);
        textAreaRect.offsetMax = new Vector2(-10, -5);

        // Create text component
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(textArea.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 28;
        text.color = Color.black;

        // Add the input field component
        TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
        inputField.textComponent = text;
        inputField.textViewport = textAreaRect;
        inputField.characterLimit = 7; // Reasonable limit for numbers
        inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
        inputField.keyboardType = TouchScreenKeyboardType.NumberPad;

        // Configure the caret
        inputField.caretWidth = 2;
        inputField.customCaretColor = true;
        inputField.caretColor = Color.black;
        inputField.caretBlinkRate = 0.85f; // Standard blink rate

        // Set placeholder text
        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(textArea.transform, false);
        RectTransform placeholderRect = placeholderObj.AddComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;

        TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderText.text = "...";
        placeholderText.alignment = TextAlignmentOptions.Center;
        placeholderText.fontSize = 28;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        inputField.placeholder = placeholderText;

        // Add a non-alloc listener for input validation
        inputField.onValueChanged.AddListener((newValue) => ValidateInput(inputField, newValue));

        return inputField;
    }

    private void ValidateInput(TMP_InputField inputField, string text)
    {
        // Regex pattern that allows only digits and a single decimal point
        if (text.Length < 8 && Regex.IsMatch(text, @"^[0-9]*\.?[0-9]*$"))
        {
            // Valid input, save it as the previous value
            previousValues[inputField] = text;
        }
        else
        {
            // Invalid input, revert to previous valid value
            inputField.text = previousValues[inputField];
            // This is important: move the caret position to maintain a good editing experience
            inputField.caretPosition = previousValues[inputField].Length;
        }
    }

    private TMP_Dropdown CreateDropdown(string name, float width, Transform parent = null)
    {
        // Create dropdown GameObject
        GameObject dropdownObj = new GameObject(name);
        dropdownObj.transform.SetParent(parent != null ? parent : dropdownContainer, false);

        // Add RectTransform with specific size
        RectTransform rectTransform = dropdownObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(width, 60);

        // Add Image component as background
        Image background = dropdownObj.AddComponent<Image>();
        background.color = Color.white;

        // Add TMP_Dropdown component
        TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();

        // Create caption text
        GameObject captionObj = new GameObject("Caption");
        captionObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform captionRect = captionObj.AddComponent<RectTransform>();
        captionRect.anchorMin = new Vector2(0, 0);
        captionRect.anchorMax = new Vector2(1, 1);
        captionRect.offsetMin = new Vector2(10, 0);
        captionRect.offsetMax = new Vector2(-30, 0);

        TextMeshProUGUI captionText = captionObj.AddComponent<TextMeshProUGUI>();
        captionText.alignment = TextAlignmentOptions.Left;
        captionText.fontSize = 28;
        captionText.color = Color.black;

        // Create arrow
        GameObject arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(dropdownObj.transform, false);
        RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
        arrowRect.sizeDelta = new Vector2(20, 20);
        arrowRect.anchorMin = new Vector2(1, 0.5f);
        arrowRect.anchorMax = new Vector2(1, 0.5f);
        arrowRect.pivot = new Vector2(1, 0.5f);
        arrowRect.anchoredPosition = new Vector2(-10, 0);

        Image arrowImage = arrowObj.AddComponent<Image>();
        arrowImage.color = Color.black;

        // Create a streamlined template
        GameObject template = CreateCompactTemplate(dropdown, width);
        dropdown.template = template.GetComponent<RectTransform>();

        // Set the caption text reference
        dropdown.captionText = captionText;

        return dropdown;
    }

    private GameObject CreateCompactTemplate(TMP_Dropdown dropdown, float width)
    {
        // Create template container
        GameObject template = new GameObject("Template");
        template.transform.SetParent(dropdown.transform, false);
        RectTransform templateRect = template.AddComponent<RectTransform>();
        templateRect.sizeDelta = new Vector2(width, 150); // More compact height
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.anchorMin = new Vector2(0.5f, 0);
        templateRect.anchorMax = new Vector2(0.5f, 0);
        templateRect.anchoredPosition = new Vector2(0, 0);
        template.AddComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f);

        // Create viewport directly (more streamlined)
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(template.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        // Add mask to viewport
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0.9f, 0.9f, 0.9f);
        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        // Create content container
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);

        // Calculate height based on options (account for 5px padding)
        int itemCount = dropdown.name.Contains("Formula") ? formulaOptions.Count : symbolOptions.Count;
        float contentHeight = itemCount * 40; // 40px per item
        contentRect.sizeDelta = new Vector2(0, contentHeight);

        // Add vertical layout group for automatic item positioning
        VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.childAlignment = TextAnchor.UpperCenter;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = false;
        contentLayout.spacing = 0;
        contentLayout.padding = new RectOffset(5, 5, 5, 5);

        // Create item template
        GameObject item = new GameObject("Item");
        item.transform.SetParent(content.transform, false);
        RectTransform itemRect = item.AddComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(width - 10, 40); // Item height 40px

        // Add toggle component to item
        Toggle itemToggle = item.AddComponent<Toggle>();

        // Create background for the toggle
        GameObject itemBg = new GameObject("Item Background");
        itemBg.transform.SetParent(item.transform, false);
        RectTransform itemBgRect = itemBg.AddComponent<RectTransform>();
        itemBgRect.anchorMin = Vector2.zero;
        itemBgRect.anchorMax = Vector2.one;
        itemBgRect.offsetMin = Vector2.zero;
        itemBgRect.offsetMax = Vector2.zero;
        Image itemBgImage = itemBg.AddComponent<Image>();
        itemBgImage.color = new Color(0.9f, 0.9f, 0.9f);

        // Set as target graphic for toggle
        itemToggle.targetGraphic = itemBgImage;

        // Add colors for better user feedback
        ColorBlock colors = itemToggle.colors;
        colors.highlightedColor = new Color(0.8f, 0.8f, 0.8f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        colors.selectedColor = new Color(0.6f, 0.6f, 0.9f);
        itemToggle.colors = colors;

        // Create item label
        GameObject itemLabel = new GameObject("Item Label");
        itemLabel.transform.SetParent(item.transform, false);
        RectTransform itemLabelRect = itemLabel.AddComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(10, 0);
        itemLabelRect.offsetMax = new Vector2(-10, 0);
        TextMeshProUGUI itemText = itemLabel.AddComponent<TextMeshProUGUI>();
        itemText.fontSize = 24; // Slightly smaller font for items
        itemText.color = Color.black;
        itemText.alignment = TextAlignmentOptions.Left;

        // Set up the references in dropdown
        dropdown.itemText = itemText;

        // Add scroll rect to template
        ScrollRect scrollRect = template.AddComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 40f;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        // Deactivate template
        template.SetActive(false);

        return template;
    }

    private void ClearDropdowns()
    {
        // Destroy all child objects in container
        foreach (Transform child in dropdownContainer)
        {
            Destroy(child.gameObject);
        }

        formulaDropdowns.Clear();
        symbolDropdowns.Clear();
        inputFields.Clear();
        previousValues.Clear();
    }

    private void SetupEventListeners()
    {
        // Add listeners to all dropdowns to update the equation when changed
        foreach (var dropdown in formulaDropdowns)
        {
            dropdown.onValueChanged.AddListener(delegate { UpdateEquation(); });
        }

        foreach (var dropdown in symbolDropdowns)
        {
            dropdown.onValueChanged.AddListener(delegate { UpdateEquation(); });
        }

        // Initial update
        UpdateEquation();
    }

    private void UpdateEquation()
    {
        string equation = "";

        for (int i = 0; i < formulaDropdowns.Count; i++)
        {
            // Get selected formula (removing display text)
            string selectedFormula = GetSelectedFormula(formulaDropdowns[i].value);
            equation += selectedFormula;

            // Add symbol if not the last formula
            if (i < symbolDropdowns.Count)
            {
                equation += " " + symbolOptions[symbolDropdowns[i].value] + " ";
            }
        }

        // Display the resulting equation
        if (resultText != null)
        {
            resultText.text = equation;
        }
    }

    private string GetSelectedFormula(int index)
    {
        switch (index)
        {
            case 0: return SQUARE_FORMULA;
            case 1: return CIRCLE_FORMULA;
            case 2: return RECTANGLE_FORMULA;
            case 3: return TRIANGLE_FORMULA;
            case 4: return SEMI_CIRCLE;
            default: return "";
        }
    }

    // Method to get the input values
    public List<float> GetInputValues()
    {
        List<float> values = new List<float>();
        foreach (var inputField in inputFields)
        {
            float value = 0f;
            if (!string.IsNullOrEmpty(inputField.text))
            {
                float.TryParse(inputField.text, out value);
            }
            values.Add(value);
        }
        return values;
    }

    // Method to calculate the actual answer based on inputs, formulas, and operations
    public float CalculateAnswer()
    {
        List<float> values = GetInputValues();
        float result = 0f;

        // Start with the first value
        if (values.Count > 0)
        {
            result = values[0];
        }

        // Apply operations to subsequent values
        for (int i = 0; i < symbolDropdowns.Count && i + 1 < values.Count; i++)
        {
            int symbolIndex = symbolDropdowns[i].value;
            string operation = symbolOptions[symbolIndex];

            // Apply operation based on selected symbol
            if (operation == ADD_SYMBOL)
            {
                result += values[i + 1];
            }
            else if (operation == SUB_SYMBOL)
            {
                result -= values[i + 1];
            }
        }

        return result;
    }
}