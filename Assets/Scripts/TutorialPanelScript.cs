using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanelScript : MonoBehaviour
{
    public GameObject tutorialPanel;
    public Button helpButton;
    public Button homeButton;
    public Button restartButton;
    public Button exitButton;
    public Button nextButton;
    public Button previousButton;
    public Text pageDisplay;
    public Image tutorialImage;
    public TextMeshProUGUI tutorialTextplanation;

    public ButtonSFXPlayer nextSounds;
    public ButtonSFXPlayer prevSounds;
    public ButtonSFXPlayer exitSounds;
    public ButtonSFXPlayer[] quickMenuSounds;

    public Sprite[] loImages;
    public Sprite[] hoImages;
    public string[] loExplain;
    public string[] hoExplain;

    private Sprite[] currentImages;
    private string[] currentText;
    private int currentPage = 1; //Remember to use currentPage - 1 for the array
    private const float defaultVolume = 1f;
    private const float startDelayOnForceRead = 5f;
    
    void Awake()
    {
        // Add Listeners to the buttons
        helpButton.onClick.AddListener(HelpButtonFunction);
        exitButton.onClick.AddListener(ExitButtonFunction);
        nextButton.onClick.AddListener(NextPage);
        previousButton.onClick.AddListener(PrevPage);

        //Determine which images and text to use
        if (GlobalVariables.enteringLO)
        {
            currentImages = loImages;
            currentText = loExplain;
        }

        else
        {
            currentImages = hoImages;
            currentText = hoExplain;
        }
    }

    void Start()
    {
        // Force Read on level 0 LO or HO
        if (GlobalVariables.enteringLO && GlobalVariables.loSelectedShape == GameBehaviour.SHAPES.SQUARE && GlobalVariables.level <= 0)
            ForceReadTutorial();
        else if (!GlobalVariables.enteringLO && GlobalVariables.level <= 0)
            ForceReadTutorial();
    }

    private void ForceReadTutorial()
    {
        exitButton.interactable = false;
        exitSounds.volume = 0f;
        Invoke(nameof(HelpButtonFunction), startDelayOnForceRead);
    }

    private void FinishedRead()
    {
        exitButton.interactable = true;
        exitSounds.volume = defaultVolume;
    }

    private void ChangePage(int page)
    {
        //Change current page
        currentPage = page;

        //Logic for enabling buttons
        if (currentPage > 1)
        {
            previousButton.interactable = true;
            prevSounds.volume = defaultVolume;
        }
        else
        {
            previousButton.interactable = false;
            prevSounds.volume = 0;
        }

        if (currentPage >= currentImages.Length)
        {
            nextButton.interactable = false;
            nextSounds.volume = 0;
        }
        else
        {
            nextButton.interactable = true;
            nextSounds.volume = 1;
        }

        //Limit value of currentPage
        if(currentPage <= 0)
            currentPage = 1;
        else if(currentPage > currentImages.Length)
            currentPage = currentImages.Length;

        //Logic for changing image, text, and pageDisplay
        pageDisplay.text = $"{currentPage.ToString()}/{currentImages.Length}";
        tutorialImage.sprite = currentImages[currentPage - 1];
        tutorialTextplanation.text = currentText[currentPage - 1];

        //Re-enable exit button on finish read
        if(currentPage >= currentImages.Length)
            FinishedRead();

        Debug.Log($"CurrentPage: {currentPage}");
    }

    private void NextPage()
    {
        ChangePage(currentPage + 1);
    }

    private void PrevPage()
    {
        ChangePage(currentPage - 1);
    }

    private void HelpButtonFunction()
    {
        //DataCollection
        GlobalVariables.helpPressCounter++;
        
        tutorialPanel.SetActive(true);

        //Turn off the buttons first
        helpButton.interactable = false;
        homeButton.interactable = false;
        restartButton.interactable = false;

        foreach(ButtonSFXPlayer sound in quickMenuSounds)
        {
            sound.volume = 0;
        }

        //Update Page display by calling change page on same page
        ChangePage(currentPage);
    }

    private void ExitButtonFunction()
    {
        // Close Panel
        tutorialPanel.SetActive(false);

        //Turn on the buttons
        helpButton.interactable = true;
        homeButton.interactable = true;
        restartButton.interactable = true;

        foreach (ButtonSFXPlayer sound in quickMenuSounds)
        {
            sound.volume = defaultVolume;
        }
    }
}
