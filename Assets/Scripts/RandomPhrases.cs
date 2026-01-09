using UnityEngine;
using TMPro;

public class RandomPhrases : MonoBehaviour
{
    [Header("목표 문장을 표시할 Text Mesh Pro")]
    public TextMeshProUGUI targetText;

    private string[] phrases;

    private void Awake()
    {
        LoadPhrases();
    }

    void Start()
    {
        ShowRandomSentence();
    }

    private void LoadPhrases()
    {
        // Resources 폴더 안에 phrases2.csv가 위치해야함
        TextAsset csvFile = Resources.Load<TextAsset>("phrases2");
        if (csvFile != null)
        {
            phrases = csvFile.text.Split(
                new[] { '\n', '\r' },
                System.StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            Debug.LogError("phrases2.csv 파일 못찾았음. 파일이 경로 상에 제대로 위치하는지 확인하셈");
            phrases = new string[] { "(No phrases file found)" };
        }
    }

    public void ShowRandomSentence()
    {
        if (phrases == null || phrases.Length == 0)
            return;

        int index = UnityEngine.Random.Range(0, phrases.Length);
        string sentence = phrases[index].Trim();

        if (targetText != null)
        {
            targetText.text = sentence;
        }
    }
}
