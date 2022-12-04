// Copyright (c) 2022 Kazuya Hirobe
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using VRM;
using VRMShaders;

namespace VirtualPoseCapture
{
    public sealed class VrmLoader : MonoBehaviour
    {
        [SerializeField] private string vrmPath;

        public async Task<GameObject> LoadVrm()
        {
            var path = Path.Combine(Application.streamingAssetsPath, vrmPath);
            var instance = await VrmUtility.LoadAsync(path, new RuntimeOnlyAwaitCaller());
            instance.ShowMeshes();
            return instance.Root;
        }

        public void DestroyVrm(GameObject vrmGameObject)
        {
            Destroy(vrmGameObject);
        }
    }
}