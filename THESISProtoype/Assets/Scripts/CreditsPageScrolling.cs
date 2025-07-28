using TMPro;
using UnityEngine;

public class CreditsPageScrolling : MonoBehaviour
{
    private TextMeshProUGUI childText;
    private int numTextPages = 0;
    private int currentPage = 1;

    private float timer = 0f;
    private const float PAGETIME = 3f;

    void Start()
    {
        childText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer > PAGETIME)
        {
            ChangePage();
            timer = 0f;
        }
    }

    private void ChangePage()
    {
        if (numTextPages == 0)
            numTextPages = childText.textInfo.pageCount;

        if (currentPage < numTextPages)
            currentPage++;

        else
            currentPage = 1;

        childText.pageToDisplay = currentPage;
    }
}
