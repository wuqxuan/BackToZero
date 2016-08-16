using UnityEngine;
using UnityEngine.SceneManagement;
// using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class UITipsController : MonoBehaviour {
	public Image m_Tips;

	private float m_timer;
	
	// Update is called once per frame
	void Update () {
		m_timer += Time.deltaTime;
		// Debug.Log(m_timer + " : m_timer");
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			SceneManager.LoadScene("LevelMenu");
			m_Tips.gameObject.SetActive(true);
		}
		if(m_timer > 10.0f)
		{
			m_Tips.gameObject.SetActive(false);
		}
	}
}
