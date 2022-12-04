// Copyright (c) 2022 Kazuya Hirobe
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using System.Linq;
using Mediapipe;
using UniRx;
using UnityEngine;

namespace VirtualPoseCapture
{
    public struct MyLandmark
    {
        public float x;
        public float y;
        public float z;
        public float visibility;

        public Vector3 Position()
        {
            return new Vector3(this.x, this.y, this.z);
        }
    }
    public class TrackingPacket
    {
        public static partial class Types {
            public enum Type {
                Unknown = 0,
                Lefthand = 1,
                Righthand = 2,
                Face = 3,
                Pose = 4,
            }

        }
    
        public Types.Type type;

        public MyLandmark[] landmark;
    }

    public class GraphDataHolder : MonoBehaviour
    {
        private static readonly int[] BlaseFaceMirroredArray = new int[468];
        private readonly bool _isMirrored = true;

        private readonly MyLandmark[] _faceMyLandmarks = new MyLandmark[468];
        public readonly BehaviorSubject<TrackingPacket> facePacket = new(null);
        private readonly MyLandmark[] _worldPoseMyLandmarks = new MyLandmark[33];
        public readonly BehaviorSubject<TrackingPacket> worldPosePacket = new(null);
        private readonly MyLandmark[] _leftHandMyLandmarks = new MyLandmark[21];
        public readonly BehaviorSubject<TrackingPacket> leftHandPacket = new(null);
        private readonly MyLandmark[] _rightHandMyLandmarks = new MyLandmark[21];
        public readonly BehaviorSubject<TrackingPacket> rightHandPacket = new(null);

        private int BlasePoseMirrored(int index)
        {
            if (index == 0) return 0;
            if (index < 4) return index + 3;
            if (index < 7) return index - 3;
            if (index % 2 == 0) return index - 1;
            return index + 1;
        }

        static GraphDataHolder()
        {
            InitBlaseFaceMirroredMap();
        }
    
        private static void InitBlaseFaceMirroredMap()
        {
            var blaseFaceMirroredMap = new Dictionary<int, int>();

            void FlipOnCenter(int[] array)
            {
                for (var i = 0; i < array.Length; i++) blaseFaceMirroredMap[array[i]] = array[array.Length - i - 1];
            }

            void MergeTwoArray(int[] array1, int[] array2)
            {
                for (var i = 0; i < array1.Length; i++)
                {
                    blaseFaceMirroredMap[array1[i]] = array2[i];
                    blaseFaceMirroredMap[array2[i]] = array1[i];
                }
            }

            // https://github.com/tensorflow/tfjs-models/blob/838611c02f51159afdd77469ce67f0e26b7bbb23/face-landmarks-detection/src/mediapipe-facemesh/keypoints.ts
            FlipOnCenter(new[]
            {
                338, 297, 332, 284, 251, 389, 356, 454, 323, 361, 288,
                397, 365, 379, 378, 400, 377, 152, 148, 176, 149, 150, 136,
                172, 58, 132, 93, 234, 127, 162, 21, 54, 103, 67, 109
            });

            FlipOnCenter(new[] { 61, 185, 40, 39, 37, 0, 267, 269, 270, 409, 291 });
            FlipOnCenter(new[] { 146, 91, 181, 84, 17, 314, 405, 321, 375, 291 });
            FlipOnCenter(new[] { 78, 191, 80, 81, 82, 13, 312, 311, 310, 415, 308 });
            FlipOnCenter(new[] { 78, 95, 88, 178, 87, 14, 317, 402, 318, 324, 308 });

            MergeTwoArray(new[]
            {
                246, 161, 160, 159, 158, 157, 173,
                33, 7, 163, 144, 145, 153, 154, 155, 133,
                247, 30, 29, 27, 28, 56, 190,
                130, 25, 110, 24, 23, 22, 26, 112, 243,
                226, 31, 228, 229, 230, 231, 232, 233, 244,
                143, 111, 117, 118, 119, 120, 121, 128, 245,

                156, 70, 63, 105, 66, 107, 55, 193,
                35, 124, 46, 53, 52, 65,
                //473, 474, 475, 476, 477,
                98, 205
            }, new[]
            {
                466, 388, 387, 386, 385, 384, 398,
                263, 249, 390, 373, 374, 380, 381, 382, 362,
                467, 260, 259, 257, 258, 286, 414,
                359, 255, 339, 254, 253, 252, 256, 341, 463,
                342, 445, 444, 443, 442, 441, 413,
                446, 261, 448, 449, 450, 451, 452, 453, 464,
                372, 340, 346, 347, 348, 349, 350, 357, 465,

                383, 300, 293, 334, 296, 336, 285, 417,
                265, 353, 276, 283, 282, 295,
                //468, 469, 470, 471, 472,
                327, 425
            });
            for (var i = 0; i < 468; i++)
                BlaseFaceMirroredArray[i] = blaseFaceMirroredMap.ContainsKey(i) ? blaseFaceMirroredMap[i] : i;
        }

        private void SetMyLandmarkFrom(NormalizedLandmark landmark, out MyLandmark myLandmark)
        {
            myLandmark.x = _isMirrored ? -landmark.X : landmark.X;
            myLandmark.y = -landmark.Y;
            myLandmark.z = landmark.Z;
            myLandmark.visibility = landmark.HasVisibility ? landmark.Visibility : 0f;
        }

        private void SetMyLandmarkFrom(Landmark landmark, out MyLandmark myLandmark)
        {
            myLandmark.x = _isMirrored ? -landmark.X : landmark.X;
            myLandmark.y = -landmark.Y;
            myLandmark.z = landmark.Z;
            myLandmark.visibility = landmark.HasVisibility ? landmark.Visibility : 0f;
        }
    
        public void AcceptWorldPose(LandmarkList landmarkList)
        {
            if (landmarkList?.Landmark == null) return;
            foreach (var i in Enumerable.Range(0, landmarkList.Landmark.Count))
            {
                var index = _isMirrored ? BlasePoseMirrored(i) : i;
                SetMyLandmarkFrom(landmarkList.Landmark[index], out _worldPoseMyLandmarks[i]);
            }

            var packet = new TrackingPacket
            {
                type = TrackingPacket.Types.Type.Pose,
                landmark = _worldPoseMyLandmarks
            };
            worldPosePacket.OnNext(packet);
        }

        public void AcceptFace(NormalizedLandmarkList landmarkList)
        {
            if (landmarkList?.Landmark == null) return;
        
            foreach (var i in Enumerable.Range(0, landmarkList.Landmark.Count))
            {
                var index = _isMirrored ? BlaseFaceMirroredArray[i] : i;
                SetMyLandmarkFrom(landmarkList.Landmark[index], out _faceMyLandmarks[i]);
            }

            var packet = new TrackingPacket
            {
                type = TrackingPacket.Types.Type.Pose,
                landmark = _faceMyLandmarks
            };
            facePacket.OnNext(packet);
        }

        public void AcceptHand(NormalizedLandmarkList landmarkList, bool isLeft)
        {
            if (landmarkList?.Landmark == null) return;
            isLeft = _isMirrored ? !isLeft : isLeft;
            foreach (var i in Enumerable.Range(0, landmarkList.Landmark.Count))
                if (isLeft)
                    SetMyLandmarkFrom(landmarkList.Landmark[i], out _leftHandMyLandmarks[i]);
                else
                    SetMyLandmarkFrom(landmarkList.Landmark[i], out _rightHandMyLandmarks[i]);

            var packet = new TrackingPacket
            {
                type = TrackingPacket.Types.Type.Pose,
                landmark = isLeft ? _leftHandMyLandmarks : _rightHandMyLandmarks
            };
            if (isLeft) leftHandPacket.OnNext(packet);
            else rightHandPacket.OnNext(packet);
        }
    }
}