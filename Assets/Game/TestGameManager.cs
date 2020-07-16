using System.Linq;
using UnityEngine;

namespace Game
{
    public class TestGameManager : MonoBehaviour
    {
        private GameManager _gameManager;
        private bool _keyHandled = false;

        void Start()
        {
            _gameManager = GameManager.instance;
        }

        void FixedUpdate()
        {
            if (Input.GetKey(KeyCode.P))
            {
                if (!_keyHandled)
                {
                    _keyHandled = true;
                    _gameManager.InitPlayer();
                }
            }
            else if (Input.GetKey(KeyCode.O))
            {
                if (!_keyHandled)
                {
                    _keyHandled = true;
                    ControlNextPlayer();
                }
            }
            else if (_keyHandled)
            {
                _keyHandled = false;
            }
        }

        public void ControlNextPlayer()
        {
            var players = FindObjectsOfType<PlayerManager>().ToList();
            if (players.Count == 0)
            {
                return;
            }
            var controlActiveIndex = players.FindIndex(c => c.IsControlActive());
            if (controlActiveIndex != -1)
            {
                players[controlActiveIndex].SetControlActive(false, null);
            }

            var nextControlActiveIndex = (controlActiveIndex + 1) % players.Count;
            players[nextControlActiveIndex].SetControlActive(true, _gameManager.playerUIController);
        }
    }
}