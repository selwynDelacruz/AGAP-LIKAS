
using TMPro;
using UnityEngine;

public class LeaderboardElement : MonoBehaviour
{
	public TextMeshProUGUI RankText;

	public TextMeshProUGUI NameText;

	public TextMeshProUGUI ScoreText;

	public void SetData(int rank, string _playerName, int score_)
	{
		RankText.text = rank.ToString();
		NameText.text = _playerName;
		ScoreText.text = score_.ToString();
	}
}

