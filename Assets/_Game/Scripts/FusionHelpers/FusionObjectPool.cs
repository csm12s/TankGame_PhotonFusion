using Fusion;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PhotonGame.FusionHelpers
{
    /// <summary>
    /// Pool of all free instances of a single type of NetworkObject's
    /// </summary>
    public class FusionObjectPool
    {
        private List<NetworkObject> _free = new List<NetworkObject>();

        public NetworkObject GetFromPool()
        {
            while (_free.Count > 0)
            {
                var t = _free[0];
                _free.RemoveAt(0);
                if (t)
                    return t;
            }
            return null;
        }

        public void Clear()
        {
            foreach (var pooled in _free)
            {
                if (pooled)
                {
                    Debug.Log($"Destroying pooled object: {pooled.gameObject.name}");
                    Object.Destroy(pooled.gameObject);
                }
            }

            _free = new List<NetworkObject>();
        }

        public void ReturnToPool(NetworkObject no)
        {
            _free.Add(no);
        }
    }
}