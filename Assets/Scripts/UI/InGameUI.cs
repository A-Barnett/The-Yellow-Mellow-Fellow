using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Image life1;
    [SerializeField] private Image life2;
    [SerializeField] private Image life3;
    [HideInInspector] public int scoreP1;
    [HideInInspector] public int scoreP2;
    public void UpdateLives()
    {
        if (PlayerPrefs.GetInt("lives") == 4)
        {
            life3.gameObject.SetActive(false);
        }else if (PlayerPrefs.GetInt("lives") == 2)
        {
            life2.gameObject.SetActive(false);
        }
        else if(PlayerPrefs.GetInt("lives") ==0)
        {
            life1.gameObject.SetActive(false);
        }
    }

    public void UpdateScore()
    {
        int scoreTotal = scoreP1 + scoreP2;
        if (scoreText)
        {
            scoreText.text = scoreTotal.ToString(); 
        }
    }

    public void SetScoreDeath()
    {
        int scoreTotal = scoreP1 + scoreP2;
        PlayerPrefs.SetInt("Score", scoreTotal);
    }
}
