using Fusion;
using UnityEngine;

namespace PhotonGame.TankGame
{
    public class WeaponManager : NetworkBehaviour
    {
        public enum WeaponType
        {
            PRIMARY,
            SECONDARY,
            BUFF
        };

        [SerializeField] private Weapon[] _weapons;
        [SerializeField] private Player _player;

        [Networked]
        public int currentPrimaryWeapon { get; set; }

        [Networked]
        public int currentSecondaryWeapon { get; set; }

        [Networked]
        public TickTimer primaryFireDelay { get; set; }

        [Networked]
        public TickTimer secondaryFireDelay { get; set; }

        [Networked]
        public int primaryAmmo { get; set; }

        [Networked]
        public int secondaryAmmo { get; set; }

        private int _activePrimaryWeapon;
        private int _activeSecondaryWeapon;

        public override void Render()
        {
            ShowAndHideWeapons();
        }

        private void ShowAndHideWeapons()
        {
            // Animates the scale of the weapon based on its active status
            for (int i = 0; i < _weapons.Length; i++)
            {
                _weapons[i].Show(i == currentPrimaryWeapon || i == currentSecondaryWeapon);
            }

            // Whenever the weapon visual is fully visible, set the weapon to be active - prevents shooting when changing weapon
            SetWeaponActive(currentPrimaryWeapon, ref _activePrimaryWeapon);
            SetWeaponActive(currentSecondaryWeapon, ref _activeSecondaryWeapon);
        }

        void SetWeaponActive(int selectedWeapon, ref int _activeWeapon)
        {
            if (_weapons[selectedWeapon].isShowing)
                _activeWeapon = selectedWeapon;
        }

        /// <summary>
        /// Activate a new weapon when picked up
        /// </summary>
        /// <param name="weaponType">Type of weapon that should be activated</param>
        /// <param name="weaponIndex">Index of weapon the _Weapons list for the player</param>
        public void ActivateWeapon(WeaponType weaponType, int weaponIndex)
        {
            int selectedWeapon = weaponType == WeaponType.PRIMARY ? currentPrimaryWeapon : currentSecondaryWeapon;
            int activeWeapon = weaponType == WeaponType.PRIMARY ? _activePrimaryWeapon : _activeSecondaryWeapon;

            if (!_player.isActivated || selectedWeapon != activeWeapon)
                return;

            // Fail safe, clamp the weapon index within weapons list bounds
            weaponIndex = Mathf.Clamp(weaponIndex, 0, _weapons.Length - 1);

            if (weaponType == WeaponType.PRIMARY)
            {
                currentPrimaryWeapon = weaponIndex;
                primaryAmmo = _weapons[weaponIndex].ammo;
            }
            else
            {
                currentSecondaryWeapon = weaponIndex;
                secondaryAmmo = _weapons[weaponIndex].ammo;
            }
        }

        /// <summary>
        /// Fire the current weapon. This is called from the Input Auth Client and on the Server in
        /// response to player input. Input Auth Client spawns a dummy shot that gets replaced by the networked shot
        /// whenever it arrives
        /// </summary>
        public void FireWeapon(WeaponType weaponType)
        {
            if (!IsWeaponFireAllowed(weaponType))
                return;

            int ammo = weaponType == WeaponType.PRIMARY ? primaryAmmo : secondaryAmmo;

            TickTimer tickTimer = weaponType == WeaponType.PRIMARY ? primaryFireDelay : secondaryFireDelay;
            if (tickTimer.ExpiredOrNotRunning(Runner) && ammo > 0)
            {
                int weaponIndex = weaponType == WeaponType.PRIMARY ? _activePrimaryWeapon : _activeSecondaryWeapon;
                Weapon weapon = _weapons[weaponIndex];

                weapon.Fire(Runner, Object.InputAuthority, _player.velocity);

                if (!weapon.infiniteAmmo)
                    ammo--;

                if (weaponType == WeaponType.PRIMARY)
                {
                    primaryFireDelay = TickTimer.CreateFromSeconds(Runner, weapon.delay);
                    primaryAmmo = ammo;
                }
                else
                {
                    secondaryFireDelay = TickTimer.CreateFromSeconds(Runner, weapon.delay);
                    secondaryAmmo = ammo;
                }

                if (/*Object.HasStateAuthority &&*/ ammo == 0)
                {
                    ResetWeapon(weaponType);
                }
            }
        }

        private bool IsWeaponFireAllowed(WeaponType weaponType)
        {
            if (!_player.isActivated)
                return false;

            // Has the selected weapon become fully visible yet? If not, don't allow shooting
            if (weaponType == WeaponType.PRIMARY && _activePrimaryWeapon != currentPrimaryWeapon)
                return false;
            else if (weaponType == WeaponType.SECONDARY && _activeSecondaryWeapon != currentSecondaryWeapon)
                return false;
            return true;
        }

        public void ResetAllWeapons()
        {
            ResetWeapon(WeaponType.PRIMARY);
            ResetWeapon(WeaponType.SECONDARY);
        }

        void ResetWeapon(WeaponType weaponType)
        {
            if (weaponType == WeaponType.PRIMARY)
            {
                ActivateWeapon(weaponType, 0);
            }
            else if (weaponType == WeaponType.SECONDARY)
            {
                ActivateWeapon(weaponType, 4);
            }
        }

        public void InstallWeapon(PowerupElement powerup)
        {
            int weaponIndex = GetWeaponIndex(powerup.powerupType);
            ActivateWeapon(powerup.weaponInstallationType, weaponIndex);
        }

        private int GetWeaponIndex(PowerupType powerupType)
        {
            for (int i = 0; i < _weapons.Length; i++)
            {
                if (_weapons[i].powerupType == powerupType)
                    return i;
            }

            Debug.LogError($"Weapon {powerupType} was not found in the weapon list, returning <color=red>0 </color>");
            return 0;
        }

        public int GetPrimaryWeaponCount()
        {
            // todo ref
            return 3;
        }

        //
    }
}