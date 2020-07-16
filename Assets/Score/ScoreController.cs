using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class ScoreController : MonoBehaviour
{
    public GameObject content;
    private GameObject _row;
    private Transform _positions;
    private Canvas _canvas;
    private DateTime _lastUpdated;

    void Start()
    {
        _row = content.transform.Find("Row").gameObject;
        _positions = content.transform.Find("Positions");
        _canvas = GetComponent<Canvas>();
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Tab))
        {
            UpdateUI();

            _canvas.enabled = true;
        }
        else if (_canvas.enabled)
        {
            _canvas.enabled = false;
        }
    }

    private void UpdateUI()
    {
        if (DateTime.Now - _lastUpdated < TimeSpan.FromSeconds(1))
        {
            return;
        }

        foreach (var row in _positions)
        {
            Destroy(((Transform) row).gameObject);
        }

        List<PlayerPosition> playerPositions;
        if (GameManager.instance.networkEnabled)
        {
            playerPositions = PhotonNetwork.PlayerList.Select(p => new PlayerPosition(p)).ToList();
        }
        else
        {
            playerPositions = FindObjectsOfType<PlayerManager>().Select(p => new PlayerPosition(p)).ToList();
        }

        playerPositions.Sort((p1, p2) =>
            (p1.Kills * 10000 - p1.Deaths).CompareTo(p2.Kills * 10000 - p2.Deaths));
        for (int i = 0; i < Math.Min(playerPositions.Count, 5); i++)
        {
            var player = playerPositions[i];
            var row = Instantiate(_row, _positions);
            var transform = row.GetComponent<RectTransform>();
            transform.anchoredPosition = new Vector2(0, -50 * i);
            row.SetActive(true);
            transform.Find("Position").GetComponent<TMP_Text>().text = (i + 1).ToString();
            transform.Find("Name").GetComponent<TMP_Text>().text = player.Name;
            transform.Find("Kills").GetComponent<TMP_Text>().text = player.Kills.ToString();
            transform.Find("Deaths").GetComponent<TMP_Text>().text = player.Deaths.ToString();
        }

        _lastUpdated = DateTime.Now;
    }

    class PlayerPosition
    {
        public string Name { get; }
        public int Kills { get; }
        public int Deaths { get; }

        public PlayerPosition(Player player)
        {
            Name = player.NickName ?? player.ActorNumber.ToString();
            var props = player.CustomProperties;
            Kills = props.ContainsKey("kills") ? (int) props["kills"] : 0;
            Deaths = props.ContainsKey("deaths") ? (int) props["deaths"] : 0;
        }

        public PlayerPosition(PlayerManager player)
        {
            Name = player.GetName();
            Kills = player.GetKills();
            Deaths = player.GetDeaths();
        }
    }
}