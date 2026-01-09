using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneChanger : MonoBehaviour
{
    public void GotoDelayScene()
    {
        SceneManager.LoadScene("DelayScene");
    }

    public void GotoTransformScene()
    {
        SceneManager.LoadScene("TransformScene");
    }

    public void GotoRateScene()
    {
        SceneManager.LoadScene("RateScene");
    }

    public void GotoTutorialScene()
    {
        SceneManager.LoadScene("TutorialScene");
    }
}
