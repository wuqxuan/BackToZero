using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;
public class Menu : MonoBehaviour
{
    public RectTransform m_back;
    public Text m_makeDetail;


    // Use this for initialization
    void Start()
    {
        m_back.DOAnchorPos(new Vector2(0.0f, 0.0f), 7.0f, false);
		m_makeDetail.DOFade(1.0f, 7.0f);
		LoadLevel();
    }

    public void LoadLevel()
    {
        StartCoroutine(WaitforLoadLevel(9f));
    }
    IEnumerator WaitforLoadLevel(float duration)
    {
        yield return new WaitForSeconds(duration);
        SceneManager.LoadScene("Level0");
    }

}
