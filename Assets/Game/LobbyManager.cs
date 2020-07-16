using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class LobbyManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public bool delay = true;
        public byte maxPlayers = 20;
        public float waitingTimeWhenFull;
        public float waitingTimeWhenNotFull;
        private bool _counting;
        private float _timeToStart;

        public override void OnEnable()
        {
            Debug.Log("LobbyManager OnEnable()");
            base.OnEnable();
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = GameManager.Version;
            if (PlayerPrefs.HasKey("nickname"))
            {
                PhotonNetwork.NickName = PlayerPrefs.GetString("nickname");
            }

            PhotonNetwork.ConnectUsingSettings();
            _timeToStart = waitingTimeWhenNotFull;
            PhotonNetwork.AddCallbackTarget(this);
        }

        public override void OnDisable()
        {
            Debug.Log("LobbyManager OnDisable()");
            base.OnDisable();
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("LobbyManager OnConnectedToMaster()");
            PhotonNetwork.JoinRandomRoom();
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("LobbyManager OnJoinRandomFailed()");
            PhotonNetwork.CreateRoom(null, new RoomOptions {MaxPlayers = maxPlayers});
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("LobbyManager OnJoinedRoom()");
            if (!delay)
            {
                GameManager.instance.OnMatchStarted();
            }
        }

        public override void OnLeftRoom()
        {
            
            PhotonNetwork.Disconnect();
            SceneManager.LoadScene("Menu");
        }

        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("LobbyManager OnPlayerEnteredRoom() actorNumber={0} userId={1} nickname={2}",
                other.ActorNumber, other.UserId, other.NickName);
            if (delay && PhotonNetwork.IsMasterClient)
            {
                _counting = true;
                if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers)
                {
                    _timeToStart = Math.Min(waitingTimeWhenFull, _timeToStart);
                }
            }
        }

        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.LogFormat("LobbyManager OnPlayerLeftRoom() {0}", other);
            if (delay && PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount < 2)
            {
                _counting = false;
                _timeToStart = waitingTimeWhenNotFull;
            }
        }

        private static byte MatchStartedEventCode = 1;

        void FixedUpdate()
        {
            if (_counting)
            {
                _timeToStart -= Time.deltaTime;
                Debug.LogFormat("LobbyManager FixedUpdate() Counting TimeToStart={0}", _timeToStart);
            }

            if (_timeToStart < 0)
            {
                Debug.Log("LobbyManager FixedUpdate() RaiseEvent MatchStartedEventCode");
                PhotonNetwork.CurrentRoom.IsOpen = false;
                _counting = false;
                _timeToStart = waitingTimeWhenNotFull;
                PhotonNetwork.RaiseEvent(MatchStartedEventCode, null,
                    new RaiseEventOptions {Receivers = ReceiverGroup.All},
                    new SendOptions {Reliability = true});
            }
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == MatchStartedEventCode)
            {
                Debug.Log("LobbyManager OnEvent() MatchStartedEventCode");
                GameManager.instance.OnMatchStarted();
            }
        }
    }
}