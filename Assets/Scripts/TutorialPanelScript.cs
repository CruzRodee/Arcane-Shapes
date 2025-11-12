using System.Collections;
using System.Collections.Generic;
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

    public ButtonSFXPlayer nextSounds;
    public ButtonSFXPlayer prevSounds;
    public ButtonSFXPlayer[] quickMenuSounds;

    public Image[] loImages;
    public Image[] hoImages;

    private Image[] currentImages;
    private int currentPage = 1; //Remember to use currentPage - 1 for the array
    private const float defaultVolume = 1f;
    
    void Awake()
    {
        // Add Listeners to the buttons
        helpButton.onClick.AddListener(HelpButtonFunction);
        exitButton.onClick.AddListener(ExitButtonFunction);
        nextButton.onClick.AddListener(NextPage);
        previousButton.onClick.AddListener(PrevPage);

        //Determine which images to use
        if (GlobalVariables.enteringLO)
            currentImages = loImages;
        else
            currentImages = hoImages;
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

        //Logic for changing image and pageDisplay
        pageDisplay.text = currentPage.ToString();

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
    }

    private void ExitButtonFunction()
    {
        // Reset variables
        currentPage = 1;

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
