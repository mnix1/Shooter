using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Game.Bullet;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static string Version = "0.1.0";
        public bool networkEnabled;
        public static GameManager instance;
        public GameObject bulletBallPrefab;
        public List<Transform> spawns;
        public PlayerUIController playerUIController;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            if (networkEnabled)
            {
                GetComponent<LobbyManager>().enabled = true;
            }
            else
            {
                OnMatchStarted();
            }
        }

        public void OnMatchStarted()
        {
            InitPlayer().SetControlActive(true, playerUIController);
        }

        public GameObject Create(GameObject prefab, string prefabPath, Transform transform)
        {
            if (networkEnabled)
            {
                return PhotonNetwork.Instantiate(prefabPath, transform.position, transform.rotation);
            }

            return Instantiate(prefab, transform.position, transform.rotation);
        }

        public void Delete(GameObject gameObject)
        {
            if (networkEnabled)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlayerShoot(Transform shootSource, Color bulletColor, Rigidbody shooterRigidbody,
            PlayerManager shooter, int? shooterActorNumber)
        {
            GameObject bullet = Instantiate(bulletBallPrefab, shootSource.position, shootSource.rotation);
            bullet.GetComponent<MeshRenderer>().material.color = bulletColor;
            bullet.GetComponent<BulletController>().SetShooter(shooter);
            if (shooterActorNumber.HasValue)
            {
                bullet.GetComponent<BulletController>().SetShooterActorNumber(shooterActorNumber.Value);
            }

            var bulletRigidbody = bullet.GetComponent<Rigidbody>();
            bulletRigidbody.AddForce(shootSource.forward * 20, ForceMode.VelocityChange);
            shooterRigidbody.AddForce(shootSource.forward * -bulletRigidbody.mass, ForceMode.VelocityChange);
        }

        //only when network enabled
        public void OtherPlayerShoot(Vector3 shootSourcePosition, Vector3 shootSourceForward, Color bulletColor,
            int shooterActorNumber)
        {
            GameObject bullet = Instantiate(bulletBallPrefab, shootSourcePosition, Quaternion.identity);
            bullet.GetComponent<MeshRenderer>().material.color = bulletColor;
            bullet.GetComponent<BulletController>().SetShooterActorNumber(shooterActorNumber);
            var bulletRigidbody = bullet.GetComponent<Rigidbody>();
            bulletRigidbody.AddForce(shootSourceForward * 20, ForceMode.VelocityChange);
            // shooterRigidbody.AddForce(shootSource.forward * -bulletRigidbody.mass, ForceMode.VelocityChange);
        }

        public Transform RandomSpawn()
        {
            return spawns[Random.Range(0, spawns.Count)];
        }

        private List<Character> AvailableCharacters()
        {
            return new List<Character>()
            {
                new Character {Name = "Aj"},
                new Character {Name = "Akai"},
                new Character {Name = "Big Vegas"},
                new Character {Name = "Claire"},
                new Character {Name = "Dreyar"},
                new Character {Name = "Jasper"},
                new Character {Name = "Knight Pelegrini"},
                new Character {Name = "Medea"},
            };
        }

        public PlayerManager InitPlayer()
        {
            var spawn = RandomSpawn();
            var availableCharacters = AvailableCharacters();
            var character = availableCharacters[Random.Range(0, availableCharacters.Count)];
            return Create(character.GetPlayerPrefab(), character.GetPlayerPrefabPath(), spawn)
                .GetComponent<PlayerManager>();
        }

        public static byte PlayerKilledPlayerEventCode = 2;
        public static byte PlayerResurrectedEventCode = 3;

        public void PlayerKilledPlayer(PlayerManager targetPlayerManager, PlayerManager shooterPlayerManager)
        {
            Debug.Log("PlayerKilledPlayer");
            var targetOwner = targetPlayerManager.photonView.Owner;
            var targetProps = new Hashtable();
            targetProps["deaths"] = targetOwner.CustomProperties.ContainsKey("deaths")
                ? 1 + (int) targetOwner.CustomProperties["deaths"]
                : 1;
            targetOwner.SetCustomProperties(targetProps);
            var shooterOwner = shooterPlayerManager.photonView.Owner;
            var shooterProps = new Hashtable();
            shooterProps["kills"] = shooterOwner.CustomProperties.ContainsKey("kills")
                ? 1 + (int) shooterOwner.CustomProperties["kills"]
                : 1;
            shooterOwner.SetCustomProperties(shooterProps);

            PhotonNetwork.RaiseEvent(PlayerKilledPlayerEventCode,
                new[] {shooterOwner.ActorNumber, targetOwner.ActorNumber},
                new RaiseEventOptions {Receivers = ReceiverGroup.All},
                new SendOptions {Reliability = true});
        }
    }
}