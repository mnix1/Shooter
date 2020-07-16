using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace Game
{
    public class GameEventsUIController : MonoBehaviour, IOnEventCallback
    {
        public static GameEventsUIController Instance;
        public int eventDisplayingSeconds;
        public RectTransform killed;
        public RectTransform resurrected;
        private TMP_Text _resurrectedSource;
        private TMP_Text _killedSource;
        private TMP_Text _killedTarget;
        private List<GameEvent> _events = new List<GameEvent>();
        private DateTime _timeWhenCurrentEventHandled = DateTime.Now;
        private bool _handling = false;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _resurrectedSource = resurrected.Find("Source").GetComponent<TMP_Text>();
            _killedSource = killed.Find("Source").GetComponent<TMP_Text>();
            _killedTarget = killed.Find("Target").GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            if (GameManager.instance.networkEnabled)
            {
                PhotonNetwork.AddCallbackTarget(this);
            }
        }

        private void OnDisable()
        {
            if (GameManager.instance.networkEnabled)
            {
                PhotonNetwork.RemoveCallbackTarget(this);
            }
        }

        private void EventHandled()
        {
            _events.RemoveAt(0);
            killed.gameObject.SetActive(false);
            resurrected.gameObject.SetActive(false);
            _handling = false;
        }

        private void FixedUpdate()
        {
            if (!_handling && _events.Count == 0)
            {
                return;
            }

            if (_handling && _timeWhenCurrentEventHandled < DateTime.Now)
            {
                EventHandled();
            }

            if (_handling)
            {
                return;
            }

            HandleNextEvent();
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == GameManager.PlayerKilledPlayerEventCode
                || photonEvent.Code == GameManager.PlayerResurrectedEventCode)
            {
                _events.Add(new GameEvent {Code = photonEvent.Code, Data = photonEvent.CustomData});
            }
        }

        public void OnEvent(GameEvent gameEvent)
        {
            _events.Add(gameEvent);
        }

        private void HandleNextEvent()
        {
            if (_events.Count == 0)
            {
                return;
            }

            _handling = true;
            var firstEvent = _events[0];
            if (firstEvent.Code == GameManager.PlayerKilledPlayerEventCode)
            {
                HandlePlayerKilledPlayerEvent(firstEvent);
            }
            else if (firstEvent.Code == GameManager.PlayerResurrectedEventCode)
            {
                HandlePlayerResurrectedEvent(firstEvent);
            }

            _timeWhenCurrentEventHandled = DateTime.Now.AddSeconds(eventDisplayingSeconds);
        }

        private void HandlePlayerKilledPlayerEvent(GameEvent gameEvent)
        {
            var data = (int[]) gameEvent.Data;
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player.ActorNumber == data[0])
                {
                    _killedSource.text = player.NickName;
                }
                else if (player.ActorNumber == data[1])
                {
                    _killedTarget.text = player.NickName;
                }
            }

            killed.gameObject.SetActive(true);
        }

        private void HandlePlayerResurrectedEvent(GameEvent gameEvent)
        {
            var data = (int) gameEvent.Data;
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player.ActorNumber == data)
                {
                    _resurrectedSource.text = player.NickName;
                }
            }

            resurrected.gameObject.SetActive(true);
        }

        public class GameEvent
        {
            public byte Code { get; set; }
            public object Data { get; set; }
        }
    }
}