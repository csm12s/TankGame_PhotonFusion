using UnityEngine;

namespace PhotonGame.TankGame
{
    public enum PowerupType
    {
        DEFAULT = 0,
        MINIGUN = 1,
        GIGAVOLT = 2,
        GRENADES = 3,
        HEALTH = 4,
        EMPTY = 5
    }

    [CreateAssetMenu(fileName = "PE_", menuName = "ScriptableObjects/PowerupElement")]
    public class PowerupElement : ScriptableObject
    {
        public WeaponManager.WeaponType weaponInstallationType;
        public PowerupType powerupType;
        public Mesh powerupSpawnerMesh;
        public AudioClipData pickupSnd;
    }
}