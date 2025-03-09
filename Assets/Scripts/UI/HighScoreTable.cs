using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreTable : MonoBehaviour
{
    [SerializeField] private string highScoreFile = "scores.txt";
    [SerializeField] private Font scoreFont;

    void Start()
    {
        LoadHighScoreTable();
        SortHighScoreEntries();
        CreateHighScoreText();
    }
    struct HighScoreEntry
    {
        public int score;
        public string name;
    }
    List<HighScoreEntry> allScores = new List<HighScoreEntry>();
    private void LoadHighScoreTable()
    {
        using (TextReader file = File.OpenText(highScoreFile))
        {
            string text = null;
            while ((text = file.ReadLine()) != null)
            {
                string[] splits = text.Split(' ');
                HighScoreEntry entry;
                entry.name = splits[0];
                entry.score = int.Parse(splits[1]);
                allScores.Add(entry);
            }
        }
    }
    private void CreateHighScoreText()
    {
        for (int i = 0; i < allScores.Count && i<10; ++i)
        {
            GameObject obj = new GameObject();
            GameObject scoreObj = new GameObject();
            obj.transform.parent = transform;
            scoreObj.transform.parent = transform;
            Text txt = obj.AddComponent<Text>();
            Text scoreTxt = scoreObj.AddComponent<Text>();
            txt.text = allScores[i].name;
            scoreTxt.text = allScores[i].score.ToString();
            txt.font = scoreFont;
            txt.fontSize = 50;
            txt.color = Color.black;
            txt.raycastTarget = false;
            scoreTxt.font = scoreFont;
            scoreTxt.fontSize = 50;
            scoreTxt.color = Color.black;
            scoreTxt.raycastTarget = false;
            obj.transform.localPosition = new Vector3(0, -(i) * 6, 0);
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 100);
            scoreObj.transform.localPosition = new Vector3(25, -(i) * 6, 0);
            scoreObj.transform.localRotation = Quaternion.identity;
            scoreObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            scoreObj.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 100);
        }
    }
    public void SortHighScoreEntries()
    {
        allScores.Sort((x,y) => y.score.CompareTo(x.score));
    }
}