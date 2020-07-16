using Photon.Pun;
using UnityEngine;

namespace Game.Bullet
{
    public class BulletController : MonoBehaviour
    {
        private PlayerManager _shooter;
        private int _shooterActorNumber;

        public void SetShooter(PlayerManager shooter)
        {
            _shooter = shooter;
        }

        public void SetShooterActorNumber(int shooterActorNumber)
        {
            _shooterActorNumber = shooterActorNumber;
        }

        void OnCollisionEnter(Collision collision)
        {
            var explosion = Instantiate(Resources.Load("ShooterExplosion"), transform.position, transform.rotation);
            Destroy(gameObject);
            Destroy(explosion, 1f);
            if (!collision.collider.CompareTag("Player"))
            {
                return;
            }

            var targetGameObject = collision.gameObject;
            var targetPlayerManager = targetGameObject.GetComponent<PlayerManager>();
            if (GameManager.instance.networkEnabled)
            {
                var targetPhotonView = targetGameObject.GetPhotonView();
                if (PhotonNetwork.IsMasterClient)
                {
                    targetPlayerManager.GiveDamage(targetPhotonView.Owner.ActorNumber, _shooterActorNumber, PhotonNetwork.LocalPlayer.ActorNumber, collision.transform.position);
                }
                else
                {
                    targetPhotonView.RPC("GiveDamage", RpcTarget.MasterClient, targetPhotonView.Owner.ActorNumber, _shooterActorNumber, PhotonNetwork.LocalPlayer.ActorNumber, collision.transform.position);
                }
            }
            else
            {
                targetPlayerManager.SetHealth(targetPlayerManager.GetHealth() - _shooter.GetDamage());
            }
        }
    }
}