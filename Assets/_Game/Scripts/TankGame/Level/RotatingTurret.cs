using Fusion;
using UnityEngine;

namespace PhotonGame.TankGame
{
    // Note that because we're placing the beams and their endpoints in Render() on *all* clients based
    // on the interpolated transform, we must make sure that the interpolation is done before we update.
    // Two ways we can do that: Make sure the NetworkTranform runs first, or do it in LateUpdate which
    // is always after Render for all NetworkBehaviours.
    [OrderAfter(typeof(NetworkTransform))]
    public class RotatingTurret : NetworkBehaviour
    {
        [SerializeField] private LaserBeam[] _laserBeams;
        [SerializeField] private float _rpm;

        public override void Spawned()
        {
            for (int i = 0; i < _laserBeams.Length; i++)
            {
                _laserBeams[i].Init();
            }
        }

        // Rotates the turret on state authority or in hosted mode even on proxies
        // (since it is rotating at a constant speed, it is safe to predict even on proxies)
        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority || Runner.GameMode != GameMode.Shared) // Only predict on proxies in hosted mode
                transform.Rotate(0, _rpm * Runner.DeltaTime, 0);
        }

        // Update the endpoint in Render so that it updates at the same frequency as the interpolated rotation
        public override void Render()
        {
            for (int i = 0; i < _laserBeams.Length; i++)
            {
                _laserBeams[i].UpdateLaserBeam();
            }
        }
    }
}