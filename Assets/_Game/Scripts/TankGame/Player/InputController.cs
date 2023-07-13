using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhotonGame.TankGame
{
    public class InputController : NetworkBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private LayerMask _mouseRayMask;

        [Networked]
        public NetworkButtons ButtonsPrevious { get; set; }

        public static bool fetchInput = true;

        private Player _player;
        private NetworkInputData _inputData = new NetworkInputData();

        private bool _primaryFire;
        private bool _secondaryFire;
        private Vector2 _aimDelta;
        
        private Vector2 _leftPos;
        private Vector2 _leftDown;
        private Vector2 _rightPos;
        private Vector2 _rightDown;
        private bool _leftTouchWasDown;
        private bool _rightTouchWasDown;

        private MobileInput _mobileInput;

        /// <summary>
        /// Hook up to the Fusion callbacks so we can handle the input polling
        /// </summary>
        public override void Spawned()
        {
            _mobileInput = FindObjectOfType<MobileInput>(true);
            _player = GetComponent<Player>();
            // It does not matter which InputController fills the input structure,
            // since the actual data will only be sent to the one that does have authority,
            // but let's make sure we give input control to
            // the gameobject that also has Input authority.
            if (Object.HasInputAuthority)
            {
                Runner.AddCallbacks(this);
            }

            Debug.Log("Spawned [" + this + "] IsClient=" + Runner.IsClient + " IsServer=" + Runner.IsServer + " HasInputAuth=" + Object.HasInputAuthority + " HasStateAuth=" + Object.HasStateAuthority);
        }


        private void Update()
        {
            if (Object.HasInputAuthority)
            {
                // simple button rpc
                if (Input.GetKeyDown(KeyCode.J))
                {
                    ChangeColor_Rpc(0);
                }

                if (Input.GetKeyDown(KeyCode.K))
                {
                    ChangeColor_Rpc(1);
                }

                if (Input.GetKeyDown(KeyCode.L))
                {
                    ChangeColor_Rpc(2);
                }

                if (Input.GetKeyDown(KeyCode.I))
                {
                    ChangeColor_Rpc(3);
                }
            }


            if (Input.mousePresent)
            {
                // mouse
                if (Input.GetMouseButton(0))
                    _primaryFire = true;

                if (Input.GetMouseButton(1))
                    _secondaryFire = true;

                // mouse aim
                Vector3 mousePos = Input.mousePosition;

                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(mousePos);

                Vector3 mouseCollisionPoint = Vector3.zero;
                // Raycast towards the mouse collider box in the world
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, _mouseRayMask))
                {
                    if (hit.collider != null)
                    {
                        mouseCollisionPoint = hit.point;
                    }
                }

                Vector3 aimDirection = mouseCollisionPoint - _player.turretPosition;
                _aimDelta = new Vector2(aimDirection.x, aimDirection.z);
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        private void ChangeColor_Rpc(int matId)
        {
            _player.InitVisual(matId);
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            if (_player != null 
                && _player.Object != null 
                && _player.state == Player.State.Active && fetchInput)
            {
                #region simple input
                var inputData = new NetworkInputData();

                if (Input.GetKey(KeyCode.W))
                    inputData.movementInput += Vector2.up;

                if (Input.GetKey(KeyCode.S))
                    inputData.movementInput += Vector2.down;

                if (Input.GetKey(KeyCode.A))
                    inputData.movementInput += Vector2.left;

                if (Input.GetKey(KeyCode.D))
                    inputData.movementInput += Vector2.right;

                // todo continus input
                //inputData.buttons.Set(InputButtons.Fire1, Input.GetKey(KeyCode.Mouse0));
                //inputData.buttons.Set(InputButtons.Fire2, Input.GetKey(KeyCode.Mouse1));

                inputData.buttons.Set(InputButtons.Ready, Input.GetKey(KeyCode.R));

                inputData.buttons.Set(InputButtons.Item1, Input.GetKey(KeyCode.Alpha1));
                inputData.buttons.Set(InputButtons.Item2, Input.GetKey(KeyCode.Alpha2));
                inputData.buttons.Set(InputButtons.Item3, Input.GetKey(KeyCode.Alpha3));
                inputData.buttons.Set(InputButtons.Item4, Input.GetKey(KeyCode.Alpha4));
                inputData.buttons.Set(InputButtons.Item5, Input.GetKey(KeyCode.Alpha5));

                _inputData.buttons = inputData.buttons;
                _inputData.movementInput = inputData.movementInput.normalized;
                #endregion

                // advance input
                if (_primaryFire)
                {
                    _primaryFire = false;
                    _inputData.IntButtons |= NetworkInputData.BUTTON_FIRE_PRIMARY;
                }

                if (_secondaryFire)
                {
                    _secondaryFire = false;
                    _inputData.IntButtons |= NetworkInputData.BUTTON_FIRE_SECONDARY;
                }

                _inputData.aimDirection = _aimDelta.normalized;
            }

            // Hand over the data to Fusion
            input.Set(_inputData);
            _inputData.IntButtons = 0;
        }

        public override void FixedUpdateNetwork()
        {
            if (GameManager.playState == GameManager.PlayState.TRANSITION)
                return;
            // Get our input struct and act accordingly. This method will only return data if we
            // have Input or State Authority - meaning on the controlling player or the server.
            Vector2 direction = default;
            if (GetInput(out NetworkInputData input))
            {
                #region advance input
                direction = input.movementInput.normalized;
                // We let the NetworkCharacterController do the actual work
                _player.SetDirections(direction, input.aimDirection.normalized);

                // fire
                if (input.IsDown(NetworkInputData.BUTTON_FIRE_PRIMARY))
                {
                    _player.shooter.FireWeapon(WeaponManager.WeaponType.PRIMARY);
                }

                if (input.IsDown(NetworkInputData.BUTTON_FIRE_SECONDARY))
                {
                    _player.shooter.FireWeapon(WeaponManager.WeaponType.SECONDARY);
                }
                #endregion

                // buttons
                #region simple input
                NetworkButtons buttons = input.buttons;
                var pressed = buttons.GetPressed(ButtonsPrevious);
                ButtonsPrevious = buttons;

                //Vector3 moveVector = input.movementInput.normalized;
                //networkCharacterController.Move(moveSpeed * moveVector * Runner.DeltaTime);

                // buttons
                if (pressed.IsSet(InputButtons.Ready))
                {
                    _player.ToggleReady();
                }

                // newInput3
                // items
                if (pressed.IsSet(InputButtons.Item1))
                {
                    _player.shooter.ActivateWeapon(WeaponManager.WeaponType.PRIMARY, 0);
                }
                if (pressed.IsSet(InputButtons.Item2))
                {
                    _player.shooter.ActivateWeapon(WeaponManager.WeaponType.PRIMARY, 1);
                }
                if (pressed.IsSet(InputButtons.Item3))
                {
                    _player.shooter.ActivateWeapon(WeaponManager.WeaponType.PRIMARY, 2);
                }

                // secondary, todo ref
                if (pressed.IsSet(InputButtons.Item4))
                {
                    _player.shooter.ActivateWeapon(WeaponManager.WeaponType.SECONDARY, 3);
                }
                if (pressed.IsSet(InputButtons.Item5))
                {
                    _player.shooter.ActivateWeapon(WeaponManager.WeaponType.SECONDARY, 4);
                }
                #endregion


            }

            _player.Move();
        }


        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }



        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    }

}