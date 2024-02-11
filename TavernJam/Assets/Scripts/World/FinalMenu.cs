using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FinalMenu : MonoBehaviour
{
    public GameObject finalMenuUI;
    public Button quitButton;
    public GameObject player;
    public GameObject scoreTextObject;

    private bool isGameOver = false;

    void Start()
    {
        // Ensure the final menu is initially hidden
        ToggleFinalMenu(false);

        // Assign functions to buttons
        quitButton.onClick.AddListener(QuitGame);
    }

    void Update()
    {
        // Check if the player is dead
        if ((player.GetComponent<Player>().isDying && !isGameOver) || (player.GetComponent<Player>().youWon))
        {
            // Trigger the final menu
            GameOver();
        }
    }

    void GameOver()
    {
        // Set the game over state
        isGameOver = true;

        // Pause the game (timeScale set to 0)
        Time.timeScale = 0f;

        // Show the final menu
        ToggleFinalMenu(true);

        // Get the player's score from the Player script
        float playerScore = player.GetComponent<Player>().finalPoints;

        // Find the Text GameObject by tag (replace "ScoreText" with your actual tag)


        if (scoreTextObject != null)
        {
            // Get the Text component from the Text GameObject
            Text scoreText = scoreTextObject.GetComponent<Text>();

            if (scoreText != null)
            {
                // Set the text to display the player's score
                scoreText.text = "Score: " + playerScore.ToString();
            }
        }
    }

    void ToggleFinalMenu(bool isShowing)
    {
        // Activate/deactivate the final menu UI
        finalMenuUI.SetActive(isShowing);
    }

    void QuitGame()
    {
        // Quit the game (works in the standalone build)
        SceneManager.LoadSceneAsync(0);
    }
}
