// Copyright (c) 2022 Kazuya Hirobe
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;

namespace VirtualPoseCapture
{
    public class FingerPoseView : MonoBehaviour
    {
        public Transform[] boneTransforms;

        public Vector3[] landmarkTempPositions = new Vector3[(int)BlaseHand.Pinky3 + 1];
        public Vector3[] landmarkTempLocalPositions = new Vector3[(int)BlaseHand.Pinky3 + 1];
        public BoneValue[] boneValues;

        private void LateUpdate()
        {
            for (var i = (int)HumanBodyBones.LeftThumbProximal; i <= (int)HumanBodyBones.RightLittleDistal; i++)
            {
                if (boneValues == null || boneValues[i] == null) continue;

                if (!PoseViewHelper.IsQuaternionInvalid(boneValues[i].RotationGoal) && !PoseViewHelper.IsQuaternionInvalid(boneValues[i].LastRotation))
                {
                    var r = Quaternion.Lerp(boneValues[i].LastRotation,
                        boneValues[i].RotationGoal, 0.75f);
                    if (!PoseViewHelper.IsQuaternionInvalid(r)) boneTransforms[i].rotation = r;
                }

                boneValues[i].LastRotation = boneTransforms[i].rotation;
            }
        }
    
        public Quaternion UpdateHandPointPositions(TrackingPacket packet, bool isLeft)
        {
            // https://qiita.com/mkt_/items/d6dc4ff3846d39b1522d
            var wristPosition = packet.landmark[(int)BlaseHand.Wrist].Position();
            var indexPosition = packet.landmark[(int)BlaseHand.IndexFinger0].Position();
            var pinkyPosition = packet.landmark[(int)BlaseHand.Pinky0].Position();
            var humanHand = isLeft ? (int)HumanBodyBones.LeftHand : (int)HumanBodyBones.RightHand;
            var humanIndex0 = isLeft ? (int)HumanBodyBones.LeftIndexProximal : (int)HumanBodyBones.RightIndexProximal;
            var humanLittle2 = isLeft ? (int)HumanBodyBones.LeftLittleDistal : (int)HumanBodyBones.RightLittleDistal;
            var humanThumb0 = isLeft ? (int)HumanBodyBones.LeftThumbProximal : (int)HumanBodyBones.RightThumbProximal;
        
            // 手首、人差し指の付け根、小指の付け根の３角形の向きが手のひらの向きとする
            Quaternion nextHandrotation = PoseViewHelper.RotateVectors(boneValues[humanIndex0].DefaultLocalVector,
                boneValues[humanLittle2].DefaultLocalVector,
                indexPosition - wristPosition,
                pinkyPosition - wristPosition);

            // landmark tempを作る。長さをVroidの指のながさに、手首の回転をVroidの手首に合わせたベクトルにする
            landmarkTempPositions[(int)BlaseHand.Wrist] = packet.landmark[(int)BlaseHand.Wrist].Position();
            landmarkTempLocalPositions[(int)BlaseHand.Wrist] = Vector3.zero;
            for (var i = (int)BlaseHand.Thumb0; i <= (int)BlaseHand.Pinky3; i++)
            {
                landmarkTempPositions[i] = packet.landmark[i].Position();

                if ((i - (int)BlaseHand.Thumb0) % 4 == 0)
                    landmarkTempLocalPositions[i] =
                        landmarkTempPositions[i] - landmarkTempPositions[(int)BlaseHand.Wrist];
                else
                    landmarkTempLocalPositions[i] = landmarkTempPositions[i] - landmarkTempPositions[i - 1];
            }

            for (var i = humanThumb0; i <= humanLittle2; i++)
            {
                // 親関節のインデックスを求める。
                var preIndex = (i - humanThumb0) % 3 == 0 ? humanHand : i - 1;

                // DefaultVectorを求める
                var defaultVector = (i - humanThumb0) % 3 == 2
                    ? boneValues[i].DefaultPosition - boneValues[preIndex].DefaultPosition // 指先はボーン座標がないので、その前の節で代用する
                    : boneValues[i + 1].DefaultPosition - boneValues[i].DefaultPosition;
                var defaultVector2 = Quaternion.Inverse(boneValues[i].DefaultRotation) * defaultVector;
                var defaultVector3 = Quaternion.Euler(0, 180, 0) * defaultVector;


                var blaseIndex = HumanBodyToBlaseHand(i);
                boneValues[i].RotationGoal =
                    Quaternion.FromToRotation(defaultVector3, landmarkTempLocalPositions[blaseIndex + 1]);

                // ねじれをとりたい
                boneValues[i].RotationGoal = PoseViewHelper.RemoveTwist(boneValues[i].RotationGoal, defaultVector3);
            }

            return nextHandrotation;
        }

        private int HumanBodyToBlaseHand(int humanBodyIndex)
        {
            return humanBodyIndex switch
            {
                // right
                >= (int)HumanBodyBones.RightLittleProximal => (int)BlaseHand.Pinky0 +
                                                              (humanBodyIndex - (int)HumanBodyBones.RightLittleProximal),
                >= (int)HumanBodyBones.RightRingProximal => (int)BlaseHand.RingFinger0 +
                                                            (humanBodyIndex - (int)HumanBodyBones.RightRingProximal),
                >= (int)HumanBodyBones.RightMiddleProximal => (int)BlaseHand.MiddleFinger0 +
                                                              (humanBodyIndex - (int)HumanBodyBones.RightMiddleProximal),
                >= (int)HumanBodyBones.RightIndexProximal => (int)BlaseHand.IndexFinger0 +
                                                             (humanBodyIndex - (int)HumanBodyBones.RightIndexProximal),
                >= (int)HumanBodyBones.RightThumbProximal => (int)BlaseHand.Thumb0 +
                                                             (humanBodyIndex - (int)HumanBodyBones.RightThumbProximal),
                // left
                >= (int)HumanBodyBones.LeftLittleProximal => (int)BlaseHand.Pinky0 +
                                                             (humanBodyIndex - (int)HumanBodyBones.LeftLittleProximal),
                >= (int)HumanBodyBones.LeftRingProximal => (int)BlaseHand.RingFinger0 +
                                                           (humanBodyIndex - (int)HumanBodyBones.LeftRingProximal),
                >= (int)HumanBodyBones.LeftMiddleProximal => (int)BlaseHand.MiddleFinger0 +
                                                             (humanBodyIndex - (int)HumanBodyBones.LeftMiddleProximal),
                >= (int)HumanBodyBones.LeftIndexProximal => (int)BlaseHand.IndexFinger0 +
                                                            (humanBodyIndex - (int)HumanBodyBones.LeftIndexProximal),
                >= (int)HumanBodyBones.LeftThumbProximal => (int)BlaseHand.Thumb0 +
                                                            (humanBodyIndex - (int)HumanBodyBones.LeftThumbProximal),
                _ => (int)BlaseHand.Wrist
            };
        }
    }
}