using PhotonGame.Utility;
using TMPro;
using UnityEngine;

namespace PhotonGame.TankGame
{
    public class ScoreLobbyUI : PooledObject
    {
        [SerializeField] private SpriteRenderer _crown;
        [SerializeField] private TextMeshPro _score;
        [SerializeField] private TextMeshPro _playerName;

        public void SetPlayerName(Player player)
        {
            _playerName.text = player.playerName.Value;

            Color textColor = player.playerMaterial.GetColor("_SilhouetteColor");
            _score.color = textColor;
            _playerName.color = textColor;
        }

        public void SetScore(int newScore)
        {
            _score.text = newScore.ToString();
        }

        public void ToggleCrown(bool on)
        {
            _crown.enabled = on;
        }
    }
}