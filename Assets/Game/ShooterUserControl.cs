using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace Game
{
    [RequireComponent(typeof(ShooterCharacter))]
    [RequireComponent(typeof(PlayerManager))]
    public class ShooterUserControl : MonoBehaviour
    {
        private ShooterCharacter m_Character;
        private PlayerManager _playerManager;
        private Transform m_Cam;
        private Vector3 m_CamForward;
        private Vector3 m_Move;

        private bool m_Jump;

        private float _shootLoadingTime = 0;
        private bool _shootLoading = false;
        private bool _active = true;

        private void OnEnable()
        {
            PauseMenuController.AddOnShowHideAction(OnShowHideMenu);
            if (Camera.main != null)
            {
                m_Cam = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning("Warning: no main camera found.", gameObject);
            }

            m_Character = GetComponent<ShooterCharacter>();
            _playerManager = GetComponent<PlayerManager>();
        }

        private void OnDisable()
        {
            PauseMenuController.RemoveOnShowHideAction(OnShowHideMenu);
        }

        private void OnShowHideMenu(bool show)
        {
            _active = !show;
        }

        private void Update()
        {
            if (!_active)
            {
                return;
            }

            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }

            PrepareShoot();
            // Fire();
        }


        private void FixedUpdate()
        {
            if (!_active)
            {
                return;
            }

            bool dancing = Input.GetKey(KeyCode.B);
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");
            bool crouch = Input.GetKey(KeyCode.C);

            if (m_Cam != null)
            {
                m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                m_Move = v * m_CamForward + h * m_Cam.right;
            }
            else
            {
                m_Move = v * Vector3.forward + h * Vector3.right;
            }
#if !MOBILE_INPUT
            if (Input.GetKey(KeyCode.LeftControl)) m_Move *= 0.5f;
#endif
            Vector3 move = m_Move.magnitude > 1f ? m_Move.normalized : m_Move;
            bool wantRun = Input.GetKey(KeyCode.LeftShift);
            bool run = CheckWillRun(move, wantRun);
            if (run)
            {
                _playerManager.DecreaseStamina(Time.fixedDeltaTime * 4);
            }

            m_Character.Move(m_Move, run, crouch, m_Jump, dancing);
            m_Jump = false;
        }

        private bool CheckWillRun(Vector3 move, bool wantRun)
        {
            _playerManager.SetStaminaGenerating(move.magnitude < 0.9 && !wantRun);
            return wantRun && _playerManager.GetStamina() > 0;
        }

        private void PrepareShoot()
        {
            if (_playerManager.GetBullets() <= 1)
            {
                return;
            }

            if (!_shootLoading && Input.GetAxis("Fire1") == 1)
            {
                _shootLoading = true;
            }

            if (_shootLoading)
            {
                _shootLoadingTime += Time.deltaTime;
            }

            if (_shootLoadingTime >= _playerManager.GetShootTime())
            {
                _shootLoading = false;
                _shootLoadingTime = 0;
                _playerManager.Shoot();
            }
        }

        // private void Fire()
        // {
        //     if (Input.GetAxis("Fire2") == 1)
        //     {
        //         _playerManager.Fire();
        //     }
        // }
    }
}