using Fusion;
using UnityEngine;

namespace PhotonGame.TankGame
{
    /// <summary>
    /// Interface implemented by any gameobject that can be damaged.
    /// </summary>
    public interface IDamagable
    {
        void TakeDamage(Vector3 impulse, int damage, PlayerRef source);
    }
}