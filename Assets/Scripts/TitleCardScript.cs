using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleCardScript : MonoBehaviour
{
    [SerializeField] private Button _button;
    public List<TextMeshProUGUI> StartTextboxes;
    public List<TextMeshProUGUI> EndingTextboxes;
    [SerializeField] private Image _titleCardImage;

    [SerializeField] private GameManager _gameMgr;
	[SerializeField] private NodeScript _nodeScriptMgr;

	private void StartGame()
	{
        _gameMgr.gameObject.SetActive(true);
        _nodeScriptMgr.gameObject.SetActive(true);

        if (StartTextboxes != null)
        {
            foreach (var textbox in StartTextboxes)
            {
                textbox.gameObject.SetActive(false);
            }
        }

		if (EndingTextboxes != null)
		{
			foreach (var textbox in EndingTextboxes)
			{
				textbox.gameObject.SetActive(true);
			}
		}

		_button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(Restart);
        _button.GetComponentInChildren<TextMeshProUGUI>(true).text = "RESTART";

        _titleCardImage.gameObject.SetActive(false);
	}

	private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
