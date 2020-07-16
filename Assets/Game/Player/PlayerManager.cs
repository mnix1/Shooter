using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Game
{
    public class PlayerManager : MonoBehaviourPun
    {
        public Transform gun;
        public Color color;
        private Rigidbody _rigidbody;
        private Transform _shootSource;
        private bool _controlActive = false;
        private int _damage;
        private int _health;
        private float _shootTime;
        private float _bullets;
        private float _bulletsGeneratingSpeed;
        private int _maxBullets;
        private float _stamina;
        private float _staminaGeneratingSpeed;
        private bool _staminaGenerating;
        private int _maxStamina;
        private int _kills = 0;
        private int _deaths = 0;
        private List<Damage> _damages = new List<Damage>();
        private PlayerUIController _uiController;
        private string _name;
        private bool _dead;
        private readonly object lockObject = new object();

        void ResetProps()
        {
            _dead = false;
            _shootTime = 0.2f;
            _bullets = 0;
            _stamina = 0;
            _damage = 10;
            _health = 100;
            _bulletsGeneratingSpeed = 2;
            _maxBullets = 20;
            _stamina = 0;
            _staminaGeneratingSpeed = 4;
            _staminaGenerating = true;
            _maxStamina = 100;
        }

        private void Awake()
        {
            ResetProps();
        }

        protected void Start()
        {
            _shootSource = gun.Find("Armature/Gun1/Gun2/Gun3/Gun4/ShootSource");
            _rigidbody = GetComponent<Rigidbody>();
            gun.Find("Gun").GetComponent<SkinnedMeshRenderer>().material.color = color;
            if (GameManager.instance.networkEnabled)
            {
                _name = String.IsNullOrEmpty(photonView.Owner.NickName)
                    ? photonView.Owner.ActorNumber.ToString()
                    : photonView.Owner.NickName;
            }
            else
            {
                _name = Guid.NewGuid().ToString().Substring(0, 5);
            }
        }

        void Update()
        {
            if (_dead)
            {
                return;
            }

            GenerateBullets();
            GenerateStamina();
        }

        private void GenerateStamina()
        {
            if (_stamina >= _maxStamina || !_staminaGenerating)
            {
                return;
            }

            _stamina = Math.Min(_maxStamina, _stamina + _staminaGeneratingSpeed * Time.deltaTime);
            UpdateStaminaUI();
        }

        public int GetDamage()
        {
            return _damage;
        }

        private void GenerateBullets()
        {
            if (_bullets >= _maxBullets)
            {
                return;
            }

            _bullets = Math.Min(_maxBullets, _bullets + _bulletsGeneratingSpeed * Time.deltaTime);
            UpdateBulletsUI();
        }

        //only on master client
        [PunRPC]
        public void GiveDamage(int targetActorNumber, int shooterActorNumber, int reporterActorNumber,
            Vector3 damagePosition)
        {
            bool updateHealth = false;
            var targetPlayerManager = FindPlayerManager(targetActorNumber);
            var shooterPlayerManager = FindPlayerManager(shooterActorNumber);
            var damage = new Damage(shooterActorNumber, reporterActorNumber, damagePosition, DateTime.Now);
            lock (lockObject)
            {
                var matchingDamages = targetPlayerManager.FindMatchingDamage(damage);
                if (matchingDamages.Count + 1 > PhotonNetwork.CurrentRoom.PlayerCount / 2f)
                {
                    foreach (var damageToRemove in matchingDamages)
                    {
                        _damages.Remove(damageToRemove);
                    }

                    updateHealth = true;
                }
                else
                {
                    _damages.Add(damage);
                }
            }

            if (updateHealth)
            {
                if (targetPlayerManager.SetHealth(targetPlayerManager.GetHealth() - shooterPlayerManager.GetDamage()))
                {
                    GameManager.instance.PlayerKilledPlayer(targetPlayerManager, shooterPlayerManager);
                }

                targetPlayerManager.photonView.RPC("UpdateHealth", RpcTarget.Others, targetActorNumber,
                    targetPlayerManager.GetHealth());
            }
        }

        //on all clients except master
        [PunRPC]
        public void UpdateHealth(int targetActorNumber, int health)
        {
            var targetPlayerManager = FindPlayerManager(targetActorNumber);
            targetPlayerManager.SetHealth(health);
        }

        private List<Damage> FindMatchingDamage(Damage damageToTest)
        {
            List<Damage> matchingDamages = new List<Damage>();
            List<Damage> damagesToRemove = new List<Damage>();
            foreach (var damage in _damages)
            {
                if (damageToTest.CloseTo(damage))
                {
                    matchingDamages.Add(damage);
                }
                else if (damage.ToRemove(DateTime.Now))
                {
                    damagesToRemove.Add(damage);
                }
            }

            foreach (var damage in damagesToRemove)
            {
                _damages.Remove(damage);
            }

            return matchingDamages;
        }

        private static PlayerManager FindPlayerManager(int actorNumber)
        {
            foreach (var playerManager in FindObjectsOfType<PlayerManager>())
            {
                if (playerManager.photonView.Owner.ActorNumber == actorNumber)
                {
                    return playerManager;
                }
            }

            return null;
        }

        public void Shoot()
        {
            _bullets -= 1;
            if (GameManager.instance.networkEnabled)
            {
                var shooterActorNumber = photonView.Owner.ActorNumber;
                photonView.RPC("OtherPlayerShoot", RpcTarget.Others, _shootSource.position, _shootSource.forward,
                    color.r, color.g, color.b, shooterActorNumber);
                GameManager.instance.PlayerShoot(_shootSource, color, _rigidbody, this, shooterActorNumber);
            }
            else
            {
                GameManager.instance.PlayerShoot(_shootSource, color, _rigidbody, this, null);
            }
        }

        public void Fire()
        {
            _rigidbody.AddForce(_shootSource.up * 100, ForceMode.Force);
            // Instantiate()
        }

        [PunRPC]
        private void OtherPlayerShoot(Vector3 shootSourcePosition, Vector3 shootSourceForward, float bulletColorRed,
            float bulletColorGreen, float bulletColorBlue, int sourceActorNumber)
        {
            GameManager.instance.OtherPlayerShoot(shootSourcePosition, shootSourceForward,
                new Color(bulletColorRed, bulletColorGreen, bulletColorBlue), sourceActorNumber);
        }

        public int GetHealth()
        {
            return _health;
        }

        public bool SetHealth(int health)
        {
            _health = health;
            UpdateHealthUI();

            if (_health <= 0)
            {
                Dead();
                // if ((GameManager.instance.networkEnabled && photonView.IsMine) || !GameManager.instance.networkEnabled)

                return true;
            }

            return false;
        }

        public void Dead()
        {
            if (_uiController)
            {
                _uiController.gameObject.SetActive(false);
                _uiController = null;
            }

            SetControlActive(false, null);
            _dead = true;
            ShowOrHide(false);

            StartCoroutine(ResurrectAfterSeconds(5));

            // GameManager.instance.Delete(gameObject);
        }

        private IEnumerator ResurrectAfterSeconds(int seconds)
        {
            yield return new WaitForSeconds(seconds);
            var newTransform = GameManager.instance.RandomSpawn();
            transform.position = newTransform.position;
            transform.rotation = newTransform.rotation;
            Resurrect();
        }

        public void Resurrect()
        {
            ResetProps();
            ShowOrHide(true);

            if (GameManager.instance.networkEnabled)
            {
                if (photonView.IsMine)
                {
                    SetControlActive(true, GameManager.instance.playerUIController);
                }
                else
                {
                    SetControlActive(false, null);
                }

                photonView.RPC("PlayerResurrected", RpcTarget.Others, photonView.Owner.ActorNumber);
            }
            else
            {
                SetControlActive(true, GameManager.instance.playerUIController);
            }
        }

        public void ShowOrHide(bool show)
        {
            foreach (var child in transform)
            {
                var gameObject = ((Transform) child).gameObject;
                if (gameObject.name != "CameraRig")
                {
                    gameObject.SetActive(show);
                }
            }

            GetComponent<CapsuleCollider>().enabled = show;
        }

        [PunRPC]
        void PlayerResurrected(int actorNumber)
        {
            foreach (var playerManager in FindObjectsOfType<PlayerManager>())
            {
                if (playerManager.photonView.Owner.ActorNumber == actorNumber)
                {
                    playerManager.ResetProps();
                    playerManager.ShowOrHide(true);
                }
            }
            GameEventsUIController.Instance.OnEvent(new GameEventsUIController.GameEvent{Code = GameManager.PlayerResurrectedEventCode, Data = actorNumber});
        }

        public bool IsControlActive()
        {
            return _controlActive;
        }

        public void SetControlActive(bool value, PlayerUIController uiController)
        {
            if (_controlActive == value)
            {
                return;
            }

            gameObject.transform.Find("CameraRig").gameObject.SetActive(value);
            gameObject.GetComponent<ShooterCharacter>().enabled = value;
            gameObject.GetComponent<ShooterUserControl>().enabled = value;
            gameObject.GetComponent<GunUserControl>().enabled = value;
            if (uiController != null)
            {
                uiController.gameObject.SetActive(value);
            }

            _uiController = uiController;
            if (value)
            {
                UpdateUI();
            }

            _controlActive = value;
        }

        private void UpdateUI()
        {
            UpdateHealthUI();
            UpdateBulletsUI();
            UpdateStaminaUI();
        }

        private void UpdateHealthUI()
        {
            if (_uiController)
            {
                _uiController.SetHealth(_health);
            }
        }

        private void UpdateBulletsUI()
        {
            if (_uiController)
            {
                _uiController.SetBullets(_bullets, _maxBullets);
            }
        }

        private void UpdateStaminaUI()
        {
            if (_uiController)
            {
                _uiController.SetStamina(_stamina, _maxStamina);
            }
        }

        private class Damage
        {
            private int shooterActorNumber;
            private int reporterActorNumber;
            private Vector3 damagePosition;
            private DateTime dateTime;

            public Damage(int shooterActorNumber, int reporterActorNumber, Vector3 damagePosition, DateTime dateTime)
            {
                this.shooterActorNumber = shooterActorNumber;
                this.reporterActorNumber = reporterActorNumber;
                this.damagePosition = damagePosition;
                this.dateTime = dateTime;
            }

            public bool CloseTo(Damage damage)
            {
                return shooterActorNumber == damage.shooterActorNumber
                       && reporterActorNumber != damage.reporterActorNumber
                       && (dateTime - damage.dateTime).Duration() < TimeSpan.FromSeconds(2)
                       && damagePosition.AlmostEquals(damage.damagePosition, 0.5f);
            }

            public bool ToRemove(DateTime now)
            {
                return now - dateTime > TimeSpan.FromSeconds(2);
            }
        }

        public float GetBullets()
        {
            return _bullets;
        }

        public double GetShootTime()
        {
            return _shootTime;
        }

        public void SetStaminaGenerating(bool value)
        {
            _staminaGenerating = value;
        }

        public float GetStamina()
        {
            return _stamina;
        }

        public void DecreaseStamina(float delta)
        {
            _stamina = Math.Max(0, _stamina - delta);
            UpdateStaminaUI();
        }

        public int GetKills()
        {
            return _kills;
        }

        public int GetDeaths()
        {
            return _deaths;
        }

        public string GetName()
        {
            return _name;
        }
    }
}