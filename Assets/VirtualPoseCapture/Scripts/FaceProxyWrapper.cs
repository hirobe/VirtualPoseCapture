// Copyright (c) 2022 Kazuya Hirobe
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using UnityEngine;
using VRM;

namespace VirtualPoseCapture
{
    public class FaceProxyWrapper
    {
        private readonly VRMBlendShapeProxy _faceProxy;

        private readonly Dictionary<string, BlendShapePreset> _presets = new()
        {
            { "Unknown", BlendShapePreset.Unknown },
            { "Neutral", BlendShapePreset.Neutral },
            { "A", BlendShapePreset.A },
            { "I", BlendShapePreset.I },
            { "U", BlendShapePreset.U },
            { "E", BlendShapePreset.E },
            { "O", BlendShapePreset.O },
            { "Blink", BlendShapePreset.Blink },
            { "Joy", BlendShapePreset.Joy },
            { "Angry", BlendShapePreset.Angry },
            { "Sorrow", BlendShapePreset.Sorrow },
            { "Fun", BlendShapePreset.Fun },
            { "LookUp", BlendShapePreset.LookUp },
            { "LookDown", BlendShapePreset.LookDown },
            { "LookLeft", BlendShapePreset.LookLeft },
            { "LookRight", BlendShapePreset.LookRight },
            { "Blink_L", BlendShapePreset.Blink_L },
            { "Blink_R", BlendShapePreset.Blink_R }
        };


        public FaceProxyWrapper(GameObject gameObject)
        {
            _faceProxy = gameObject.GetComponent<VRMBlendShapeProxy>();
        }

        public void WriteData(BlendShapePreset preset, float value)
        {
            // TODO: Use BlendShapeKey.CreateFromPreset( https://vrm-c.github.io/UniVRM/ja/api/0_58_blendshape.html )
            _faceProxy.ImmediatelySetValue(preset, value);
        }

        public void WriteData(Dictionary<string, float> values)
        {
            foreach (var pair in values)
                if (_presets.ContainsKey(pair.Key))
                    _faceProxy.ImmediatelySetValue(_presets[pair.Key], pair.Value);
        }

        public Dictionary<string, float> ReadData()
        {
            var values = new Dictionary<string, float>();
            foreach (var pair in _faceProxy.GetValues())
            {
                if (pair.Value == 0.0f) continue;
                values[pair.Key.Name] = pair.Value;
            }

            return values;
        }
    }
}