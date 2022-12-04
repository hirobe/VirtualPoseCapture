// Copyright (c) 2022 Kazuya Hirobe
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;

namespace VirtualPoseCapture
{
    internal static class PoseViewHelper
    {
        public static bool IsQuaternionInvalid(Quaternion q)
        {
            if (q.x == 0 && q.y == 0 && q.z == 0 && q.w == 0) return true;
            return float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w);
        }

        public static Quaternion RotateVectors(Vector3 fromA, Vector3 fromB, Vector3 toA, Vector3 toB)
        {
            var q2 = Quaternion.FromToRotation(fromA, toA);
            var v1 = Quaternion.Inverse(q2) * toB;
            var fromBProj = Vector3.ProjectOnPlane(fromB, fromA);
            var v1Proj = Vector3.ProjectOnPlane(v1, fromA);
            var q1 = Quaternion.FromToRotation(fromBProj, v1Proj);
            return Quaternion.Normalize(q2 * q1);
        }

        public static Quaternion RemoveTwist(Quaternion q, Vector3 v)
        {
            var qv = new Vector3(q.x, q.y, q.z);
            var s = q.w;
            var dot = Vector3.Dot(qv, v);
            var qv2 = qv - dot * v;
            var s2 = s - dot * v.magnitude;
            return new Quaternion(qv2.x, qv2.y, qv2.z, s2);
        }
    }
}