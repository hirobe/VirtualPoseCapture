#!/bin/sh

echo Copying models...

tar xf PackageFiles/com.github.homuler.mediapipe-0.10.1.tgz -O package/Runtime/Resources/face_detection_short_range.bytes > Assets/StreamingAssets/face_detection_short_range.bytes
tar xf PackageFiles/com.github.homuler.mediapipe-0.10.1.tgz -O package/Runtime/Resources/face_landmark.bytes > Assets/StreamingAssets/face_landmark.bytes
tar xf PackageFiles/com.github.homuler.mediapipe-0.10.1.tgz -O package/Runtime/Resources/face_landmark_with_attention.bytes > Assets/StreamingAssets/face_landmark_with_attention.bytes
tar xf PackageFiles/com.github.homuler.mediapipe-0.10.1.tgz -O package/Runtime/Resources/hand_landmark_full.bytes > Assets/StreamingAssets/hand_landmark_full.bytes
tar xf PackageFiles/com.github.homuler.mediapipe-0.10.1.tgz -O package/Runtime/Resources/hand_recrop.bytes > Assets/StreamingAssets/hand_recrop.bytes
tar xf PackageFiles/com.github.homuler.mediapipe-0.10.1.tgz -O package/Runtime/Resources/iris_landmark.bytes > Assets/StreamingAssets/iris_landmark.bytes
tar xf PackageFiles/com.github.homuler.mediapipe-0.10.1.tgz -O package/Runtime/Resources/palm_detection_full.bytes > Assets/StreamingAssets/palm_detection_full.bytes
tar xf PackageFiles/com.github.homuler.mediapipe-0.10.1.tgz -O package/Runtime/Resources/pose_detection.bytes > Assets/StreamingAssets/pose_detection.bytes
tar xf PackageFiles/com.github.homuler.mediapipe-0.10.1.tgz -O package/Runtime/Resources/pose_landmark_full.bytes > Assets/StreamingAssets/pose_landmark_full.bytes
tar xf PackageFiles/com.github.homuler.mediapipe-0.10.1.tgz -O package/Runtime/Resources/pose_landmark_heavy.bytes > Assets/StreamingAssets/pose_landmark_heavy.bytes
tar xf PackageFiles/com.github.homuler.mediapipe-0.10.1.tgz -O package/Runtime/Resources/pose_landmark_lite.bytes > Assets/StreamingAssets/pose_landmark_lite.bytes
tar xf PackageFiles/com.github.homuler.mediapipe-0.10.1.tgz -O package/Runtime/Resources/handedness.txt > Assets/StreamingAssets/handedness.txt

ls -l Assets/StreamingAssets
