// Copyright (c) 2022 Kazuya Hirobe
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using RootMotion;
using RootMotion.FinalIK;
using UniRx;
using UnityEngine;
using VRM;
using Random = UnityEngine.Random;

namespace VirtualPoseCapture
{

    public enum BlazePose
    {
        // https://google.github.io/mediapipe/solutions/pose.html
        Nose = 0,
        LeftEar = 7,
        RightEar = 8,
        LeftUpperArm = 11,
        RightUpperArm = 12,
        LeftLowerArm = 13,
        RightLowerArm = 14,
        LeftHand = 15,
        RightHand = 16,
        LeftUpperLeg = 23,
        RightUpperLeg = 24,
        LeftLowerLeg = 25,
        RightLowerLeg = 26,
        LeftFoot = 27,
        RightFoot = 28
    }

    public enum BlazeFace
    {
        // https://github.com/google/mediapipe/issues/1615
        // top:1
        // bottom:152
        NoseTop = 1,
        NoseBottom = 2,
        Top = 10,
        Bottom = 152,
        Left = 454,
        Right = 234,
        LeftEyeTop = 386,
        LeftEyeBottom = 374,
        RightEyeTop = 159,
        RightEyeBottom = 145,
        RipTop = 0,
        RipBottom = 17,
        RipLeft = 291,
        RipRight = 61,
        MouseTop = 13,
        MouseBottom = 14
    }

    public enum BlaseHand
    {
        Wrist = 0,
        Thumb0 = 1,
        Thumb1 = 2,
        Thumb2 = 3,
        Thumb3 = 4,
        IndexFinger0 = 5,
        IndexFinger1 = 6,
        IndexFinger2 = 7,
        IndexFinger3 = 8,
        MiddleFinger0 = 9,
        MiddleFinger1 = 10,
        MiddleFinger2 = 11,
        MiddleFinger3 = 12,
        RingFinger0 = 13,
        RingFinger1 = 14,
        RingFinger2 = 15,
        RingFinger3 = 16,
        Pinky0 = 17,
        Pinky1 = 18,
        Pinky2 = 19,
        Pinky3 = 20
    }

    public class BoneValue
    {
        public Vector3 DefaultLocalVector;
        public Vector3 DefaultPosition;
        public Quaternion DefaultRotation;
        public Quaternion LastRotation;
        public Quaternion RotationGoal;
    }


    public class PoseView : MonoBehaviour
    {
        [SerializeField] private GraphDataHolder graphDataHolder;
        [SerializeField] private GameObject lookAtTraget;
        private Transform[] _boneTransforms;

        private int _blinkingFrameCount = -1;
        private Vector3 _boneCuArmsToCuLegsVector;
        private Camera _camera;

        private FaceProxyWrapper _faceProxyWrapper;

        private FullBodyBipedIK _finalIk;
        private LookAtIK _finalIKLookAt;
        [SerializeField] private BoneValue[] _handFingerValues;
        private FBBIKHeadEffector _headEffector;
        private GameObject _headEffectorTarget;

        private Vector3 _lastTrackedCuArmsToCuLegs;
        private Vector3 _lastTrackedLFootVector;
        private Vector3 _lastTrackedLHandVector;
        private Vector3 _lastTrackedLlArmVector;
        private Vector3 _lastTrackedLlLegVector;
        private Vector3 _lastTrackedLuLegVector;
        private Vector3 _lastTrackedRFootVector;
        private Vector3 _lastTrackedRHandVector;
        private Vector3 _lastTrackedRlArmVector;
        private Vector3 _lastTrackedRlLegVector;
        private Vector3 _lastTrackedRuLegVector;
        private GameObject _leftArmChainTarget;
        private GameObject _leftLegChainTarget;
        private float _lowerArmLength;
        private float _lowerLegLength;
        private float _luCenterToUperLegLength;

        private float _neckToUperArmLength;
        private float _nextBlinkWaitFrame = 100;
        private GameObject _rightArmChainTarget;
        private GameObject _rightLegChainTarget;
        private float _upperArmLength;
        private float _upperLegLength;
        private GameObject _vrmGameObject;
        private VrmLoader _vrmLoader;
        private VRMLookAtHead _vrmLookAtHead;

        private float _blessRadian;
        private int _closingFrames = 10;
        private FingerPoseView _fingerPoseView;
        private float _frontFromBoneCuLegsToRoot;

        private Quaternion _nextHeaderRotation;
        private Vector3 _nextLFoot;
        private Vector3 _nextLHand;

        private Quaternion _nextLHandLocalRotation;
        private Vector3 _nextLlArm;
        private Vector3 _nextLlLeg;

        private Vector3 _nextLuArm;
        private Vector3 _nextLuLeg;
        private Vector3 _nextRFoot;
        private Vector3 _nextRHand;
        private Quaternion _nextRHandLocalRotation;
        private Vector3 _nextRlArm;
        private Vector3 _nextRlLeg;

        private Vector3 _nextRoot;
        private Vector3 _nextRuArm;
        private Vector3 _nextRuLeg;

        private TrackingPacket _posePacket;

        private float _upFromCuLegsToRootRate = 0.5f;

        private bool _isFinalIKSetupCompleted;
        private async void Start()
        {
            _camera = Camera.main;

            _closingFrames = (int)(0.33f / Time.deltaTime);

            _vrmLoader = GetComponent<VrmLoader>();
            _vrmGameObject = await _vrmLoader.LoadVrm();
            _vrmGameObject.transform.SetParent(gameObject.transform, false);
            _vrmGameObject.transform.rotation = new Quaternion(0, 1, 0, 0);

            var animator = _vrmGameObject.GetComponent<Animator>();
            // Get the value before messing with animator to get the value of the T-bone state
            _boneTransforms = new Transform[(int)HumanBodyBones.LastBone];
            _handFingerValues = new BoneValue[(int)HumanBodyBones.LastBone];
            for (var i = 0; i < (int)HumanBodyBones.LastBone; i++)
            {
                var boneValue = new BoneValue();
                _handFingerValues[i] = boneValue;

                _boneTransforms[i] = animator.GetBoneTransform((HumanBodyBones)i);
                if (_boneTransforms[i] != null)
                {
                    _handFingerValues[i].DefaultLocalVector = _boneTransforms[i].localPosition;
                    _handFingerValues[i].DefaultPosition = _boneTransforms[i].position;
                    _handFingerValues[i].DefaultRotation = _boneTransforms[i].rotation;
                }
            }

            var overrideController = new AnimatorOverrideController
            {
                runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("IKAnimatorController")
            };
            animator.runtimeAnimatorController = overrideController;
            animator.applyRootMotion = false;

            Observable.TimerFrame(3).Subscribe(_ =>
            {
                _fingerPoseView = gameObject.AddComponent<FingerPoseView>();
                _fingerPoseView.boneTransforms = _boneTransforms;
                _fingerPoseView.boneValues = _handFingerValues;

                InitializeNextPositons();

                // root positon
                var luLeg = (int)HumanBodyBones.LeftUpperLeg;
                var ruLeg = (int)HumanBodyBones.RightUpperLeg;
                var luArm = (int)HumanBodyBones.LeftUpperArm;
                var ruArm = (int)HumanBodyBones.RightUpperArm;

                var llArm = (int)HumanBodyBones.LeftLowerArm;
                var lHand = (int)HumanBodyBones.LeftHand;
                var rlArm = (int)HumanBodyBones.RightLowerArm;
                var rHand = (int)HumanBodyBones.RightHand;
                var llLeg = (int)HumanBodyBones.LeftLowerLeg;
                var lFoot = (int)HumanBodyBones.LeftFoot;
                var rlLeg = (int)HumanBodyBones.RightLowerLeg;
                var rFoot = (int)HumanBodyBones.RightFoot;

                _upperArmLength = (_boneTransforms[luArm].position - _boneTransforms[llArm].position).magnitude;
                _lowerArmLength = (_boneTransforms[llArm].position - _boneTransforms[lHand].position).magnitude;
                _upperLegLength = (_boneTransforms[luLeg].position - _boneTransforms[llLeg].position).magnitude;
                _lowerLegLength = (_boneTransforms[llLeg].position - _boneTransforms[lFoot].position).magnitude;

                // make length to root 
                var root = _boneTransforms[(int)HumanBodyBones.Hips].transform.position;
                var boneCuArms = (_boneTransforms[luArm].position + _boneTransforms[ruArm].position) * 0.5f;
                var boneCuLegs = (_boneTransforms[luLeg].position + _boneTransforms[ruLeg].position) * 0.5f;
                _boneCuArmsToCuLegsVector = boneCuArms - boneCuLegs;
                var boneCuLegsToRoot = root - boneCuLegs;
                _upFromCuLegsToRootRate = Vector3.Dot(-_boneCuArmsToCuLegsVector, boneCuLegsToRoot) /
                                          _boneCuArmsToCuLegsVector.magnitude;
                _frontFromBoneCuLegsToRoot =
                    (root - (boneCuLegs - _boneCuArmsToCuLegsVector * _upFromCuLegsToRootRate)).magnitude;

                _neckToUperArmLength = (boneCuArms - _boneTransforms[luArm].position).magnitude;
                _luCenterToUperLegLength = (boneCuLegs - _boneTransforms[luLeg].position).magnitude;

                _lastTrackedCuArmsToCuLegs = boneCuLegs - boneCuArms;
                _lastTrackedLuLegVector = _boneTransforms[luLeg].position - boneCuLegs;
                _lastTrackedRuLegVector = _boneTransforms[ruLeg].position - boneCuLegs;
                _lastTrackedLlArmVector = _boneTransforms[llArm].position - _boneTransforms[luArm].position;
                _lastTrackedLHandVector = _boneTransforms[lHand].position - _boneTransforms[llArm].position;
                _lastTrackedRlArmVector = _boneTransforms[rlArm].position - _boneTransforms[ruArm].position;
                _lastTrackedRHandVector = _boneTransforms[rHand].position - _boneTransforms[rlArm].position;

                _lastTrackedLlLegVector = _boneTransforms[llLeg].position - _boneTransforms[luLeg].position;
                _lastTrackedLFootVector = _boneTransforms[lFoot].position - _boneTransforms[llLeg].position;
                _lastTrackedRlLegVector = _boneTransforms[rlLeg].position - _boneTransforms[ruLeg].position;
                _lastTrackedRFootVector = _boneTransforms[rFoot].position - _boneTransforms[rlLeg].position;

                SetupForFinalIK();
                SetupBindings();
            
                _isFinalIKSetupCompleted = true;
            }).AddTo(this);
        }

        private void Update()
        {
            if (_posePacket != null) UpdatePosePointPositions(_posePacket);

            SetIKPositions();

            UpdateBlessCount();

            UpdateBlink();

            if (_isFinalIKSetupCompleted) _vrmLookAtHead.LookWorldPosition();
        }

        private void OnDestroy()
        {
            _vrmLoader.DestroyVrm(_vrmGameObject);
        }

        private void SetupForFinalIK()
        {
            // add finalIk
            // http://www.root-motion.com/finalikdox/html/page8.html
            BipedReferences references = null;
            BipedReferences.AutoDetectReferences(ref references, _vrmGameObject.transform,
                BipedReferences.AutoDetectParams.Default);
            _finalIk = _vrmGameObject.AddComponent<FullBodyBipedIK>(); // Adding the component

            _vrmGameObject.AddComponent<ShoulderRotator>();
            _finalIk.SetReferences(references, null);

            _leftArmChainTarget = new GameObject();
            _leftArmChainTarget.transform.SetParent(_vrmGameObject.transform);
            _rightArmChainTarget = new GameObject();
            _rightArmChainTarget.transform.SetParent(_vrmGameObject.transform);

            _leftLegChainTarget = new GameObject();
            _leftLegChainTarget.transform.SetParent(_vrmGameObject.transform);
            _rightLegChainTarget = new GameObject();
            _rightLegChainTarget.transform.SetParent(_vrmGameObject.transform);

            // Creating the head effector GameObject
            _headEffectorTarget = new GameObject
            {
                name = _finalIk.gameObject.name + " Head Effector",
                transform =
                {
                    position = references.head.position,
                    rotation = references.head.rotation
                }
            };

            // Adding the FBBIKHeadEffector script
            var headEffector = _headEffectorTarget.AddComponent<FBBIKHeadEffector>();
            headEffector.ik = _finalIk;
            headEffector.bodyWeight = 0.0f;
            headEffector.thighWeight = 0.0f;
            _headEffector = headEffector;

            // Assigning bend bones (just realized I need to make a constructor for FBBIKHeadEffector.BendBone)
            var spine = new FBBIKHeadEffector.BendBone
            {
                transform = references.spine[0],
                weight = 0.5f
            };

            var chest = new FBBIKHeadEffector.BendBone
            {
                transform = references.spine[1],
                weight = 0.5f
            };

            headEffector.bendBones = new[]
            {
                spine,
                chest
            };

            // Set weights
            headEffector.bendWeight = 1.0f;
            headEffector.positionWeight = 0.0f;
            headEffector.rotationWeight = 1.0f;

            // eye 

            _vrmLookAtHead = _vrmGameObject.GetComponent<VRMLookAtHead>();
            _vrmLookAtHead.enabled = true;
            _vrmLookAtHead.Target = lookAtTraget.transform; // _boneTransforms[(int)HumanBodyBones.LeftHand];
            var vrmLookAtBone = _vrmGameObject.GetComponent<VRMLookAtBoneApplyer>();
            vrmLookAtBone.enabled = true;
            _vrmLookAtHead.UpdateType = UpdateType.None; // Ensure that the eye-tracking function is not called automatically.
            headEffector.OnPostHeadEffectorFK = _vrmLookAtHead.LookWorldPosition;

            _faceProxyWrapper = new FaceProxyWrapper(_vrmGameObject);
        }

        private void SetupBindings()
        {
            graphDataHolder.leftHandPacket.Where(x => x != null)
                .ObserveOnMainThread()
                .Subscribe(x =>
                {
                    _nextLHandLocalRotation =
                        Quaternion.Inverse(_boneTransforms[(int)HumanBodyBones.LeftLowerArm].rotation) *
                        _fingerPoseView.UpdateHandPointPositions(x, true);
                }).AddTo(this);
            graphDataHolder.rightHandPacket.Where(x => x != null)
                .ObserveOnMainThread()
                .Subscribe(x =>
                {
                    _nextRHandLocalRotation =
                        Quaternion.Inverse(_boneTransforms[(int)HumanBodyBones.RightLowerArm].rotation) *
                        _fingerPoseView.UpdateHandPointPositions(x, false);
                }).AddTo(this);
            graphDataHolder.worldPosePacket.Where(x => x != null)
                .ObserveOnMainThread()
                .Subscribe(x => { _posePacket = x; }).AddTo(this);
            graphDataHolder.facePacket.Where(x => x != null)
                .ObserveOnMainThread()
                .Subscribe(x => { UpdateFacePointPositions(x); }).AddTo(this);
        }

        private void UpdateFacePointPositions(TrackingPacket packet)
        {
            if (_camera == null) return;

            var lEar = packet.landmark[(int)BlazeFace.Left].Position();
            var rEar = packet.landmark[(int)BlazeFace.Right].Position();

            var top = packet.landmark[(int)BlazeFace.Top].Position();
            var bottom = packet.landmark[(int)BlazeFace.Bottom].Position();

            var toCamera = _camera.transform.position - _boneTransforms[(int)HumanBodyBones.Head].position;


            _nextHeaderRotation =
                PoseViewHelper.RotateVectors(Vector3.left, new Vector3(0, -toCamera.z, toCamera.y), lEar - rEar, top - bottom);

            _headEffector.bendWeight = 1.0f;
            _headEffector.positionWeight = 0.8f;
            _headEffector.rotationWeight = 0.8f;

            var noseBottom = packet.landmark[(int)BlazeFace.NoseBottom].Position();
            var ripTop = packet.landmark[(int)BlazeFace.RipTop].Position();
            var ripBottom = packet.landmark[(int)BlazeFace.RipBottom].Position();

            _faceProxyWrapper.WriteData(BlendShapePreset.A,
                Math.Min(Math.Max((noseBottom - ripBottom).magnitude / (noseBottom - ripTop).magnitude - 2.3f, 0f), 1.5f));
        }

        private void UpdateBlink()
        {
            if (!_isFinalIKSetupCompleted) return;
        
            if (_nextBlinkWaitFrame < 0 && _blinkingFrameCount < 0)
            {
                // start blink
                _blinkingFrameCount = _closingFrames * 2;
            }
            else if (_blinkingFrameCount >= _closingFrames) // closing
            {
                _faceProxyWrapper.WriteData(BlendShapePreset.Blink,
                    1.0f - (_blinkingFrameCount - _closingFrames) / (float)_closingFrames);

                _blinkingFrameCount -= 1;
            }
            else if (_blinkingFrameCount > 0) // opening
            {
                _faceProxyWrapper.WriteData(BlendShapePreset.Blink, _blinkingFrameCount / (float)_closingFrames);
                _blinkingFrameCount -= 1;
            }
            else if (_blinkingFrameCount == 0)
            {
                _faceProxyWrapper.WriteData(BlendShapePreset.Blink, 0);
                _blinkingFrameCount = -1;

                _nextBlinkWaitFrame = 5f / Time.deltaTime + Random.value * 5f / Time.deltaTime;
            }
            else
            {
                _nextBlinkWaitFrame -= 1;
            }
        }

        private void InitializeNextPositons()
        {
            _nextLuArm = _boneTransforms[(int)HumanBodyBones.LeftUpperArm].position;
            _nextRuArm = _boneTransforms[(int)HumanBodyBones.RightUpperArm].position;
            _nextLlArm = _boneTransforms[(int)HumanBodyBones.LeftLowerArm].position;
            _nextRlArm = _boneTransforms[(int)HumanBodyBones.RightLowerArm].position;
            _nextLHand = _boneTransforms[(int)HumanBodyBones.LeftHand].position;
            _nextRHand = _boneTransforms[(int)HumanBodyBones.RightHand].position;

            _nextLuLeg = _boneTransforms[(int)HumanBodyBones.LeftUpperLeg].position;
            _nextRuLeg = _boneTransforms[(int)HumanBodyBones.RightUpperLeg].position;
            _nextLlLeg = _boneTransforms[(int)HumanBodyBones.LeftLowerLeg].position;
            _nextRlLeg = _boneTransforms[(int)HumanBodyBones.RightLowerLeg].position;
            _nextLFoot = _boneTransforms[(int)HumanBodyBones.LeftFoot].position;
            _nextRFoot = _boneTransforms[(int)HumanBodyBones.RightFoot].position;

            _nextHeaderRotation = _boneTransforms[(int)HumanBodyBones.Head].rotation;

            _nextRoot = _boneTransforms[(int)HumanBodyBones.Hips].position;
        }

        private Vector3 BlessVector()
        {
            return Vector3.zero;
        }

        private void UpdateBlessCount()
        {
            _blessRadian += Mathf.PI * 2.0f / (5f * 120f);
            if (_blessRadian >= Mathf.PI * 2.0f) _blessRadian = 0;
        }

        private void SetIKPositions()
        {
            if (!_isFinalIKSetupCompleted) return;

            _finalIk.solver.bodyEffector.position = IKPositon(_nextRoot, HumanBodyBones.Hips);
            _finalIk.solver.bodyEffector.positionWeight = 0.5f;

            _finalIk.solver.leftShoulderEffector.position =
                IKPositon(_nextLuArm, HumanBodyBones.LeftUpperArm) + BlessVector();
            _finalIk.solver.leftShoulderEffector.positionWeight = 1.0f;
            _finalIk.solver.rightShoulderEffector.position =
                IKPositon(_nextRuArm, HumanBodyBones.RightUpperArm) + BlessVector();
            _finalIk.solver.rightShoulderEffector.positionWeight = 1.0f;

            _finalIk.solver.leftThighEffector.position =
                IKPositon(_nextLuLeg, HumanBodyBones.LeftUpperLeg) + BlessVector() * 0.5f;
            _finalIk.solver.leftThighEffector.positionWeight = 1.0f;
            _finalIk.solver.rightThighEffector.position =
                IKPositon(_nextRuLeg, HumanBodyBones.RightUpperLeg) + BlessVector() * 0.5f;
            _finalIk.solver.rightThighEffector.positionWeight = 1.0f;

            _leftArmChainTarget.transform.position =
                IKPositon(_nextLlArm, HumanBodyBones.LeftLowerArm) + BlessVector() * 0.5f;
            _finalIk.solver.leftArmChain.bendConstraint.bendGoal = _leftArmChainTarget.transform;
            _finalIk.solver.leftArmChain.bendConstraint.weight = 1.0f;
            _finalIk.solver.leftHandEffector.position = IKPositon(_nextLHand, HumanBodyBones.LeftHand);
            _finalIk.solver.leftHandEffector.positionWeight = 1.0f;
            _finalIk.solver.leftHandEffector.rotation =
                _boneTransforms[(int)HumanBodyBones.LeftLowerArm].rotation * _nextLHandLocalRotation;
            _finalIk.solver.leftHandEffector.rotationWeight = 1.0f;


            _rightArmChainTarget.transform.position =
                IKPositon(_nextRlArm, HumanBodyBones.RightLowerArm) + BlessVector() * 0.5f;
            _finalIk.solver.rightArmChain.bendConstraint.bendGoal = _rightArmChainTarget.transform;
            _finalIk.solver.rightArmChain.bendConstraint.weight = 1.0f;
            _finalIk.solver.rightHandEffector.position = IKPositon(_nextRHand, HumanBodyBones.RightHand);
            _finalIk.solver.rightHandEffector.positionWeight = 1.0f;
            _finalIk.solver.rightHandEffector.rotation = _boneTransforms[(int)HumanBodyBones.RightLowerArm].rotation *
                                                         _nextRHandLocalRotation;
            _finalIk.solver.rightHandEffector.rotationWeight = 1.0f;

            _leftLegChainTarget.transform.position = IKPositon(_nextLlLeg, HumanBodyBones.LeftLowerLeg);
            _finalIk.solver.leftLegChain.bendConstraint.bendGoal = _leftLegChainTarget.transform;
            _finalIk.solver.leftLegChain.bendConstraint.weight = 1.0f;
            _finalIk.solver.leftFootEffector.position = IKPositon(_nextLFoot, HumanBodyBones.LeftFoot);
            _finalIk.solver.leftFootEffector.positionWeight = 1.0f;

            _rightLegChainTarget.transform.position = IKPositon(_nextRlLeg, HumanBodyBones.RightLowerLeg);
            _finalIk.solver.rightLegChain.bendConstraint.bendGoal = _rightLegChainTarget.transform;
            _finalIk.solver.rightLegChain.bendConstraint.weight = 1.0f;
            _finalIk.solver.rightFootEffector.position = IKPositon(_nextRFoot, HumanBodyBones.RightFoot);
            _finalIk.solver.rightFootEffector.positionWeight = 1.0f;

            _headEffectorTarget.transform.rotation = IKRotation(_nextHeaderRotation, HumanBodyBones.Head);
        }


        private Vector3 IKPositon(Vector3 goalPositon, HumanBodyBones bone)
        {
            return goalPositon * 0.2f + _boneTransforms[(int)bone].position * 0.8f;
        }

        private Quaternion IKRotation(Quaternion goalRotation, HumanBodyBones bone)
        {
            return Quaternion.Lerp(goalRotation, _boneTransforms[(int)bone].rotation, 0.8f);
        }

        private void UpdatePosePointPositions(TrackingPacket packet)
        {
            var visiUpper = 0.6f;
            // center
            var luArm = packet.landmark[(int)BlazePose.LeftUpperArm].Position();
            var ruArm = packet.landmark[(int)BlazePose.RightUpperArm].Position();
            var cuArms = (luArm + ruArm) * 0.5f;
            var luArmVector = (luArm - cuArms).normalized * _neckToUperArmLength;
            var ruArmVector = (ruArm - cuArms).normalized * _neckToUperArmLength;
            _nextLuArm = packet.landmark[(int)BlazePose.LeftUpperArm].visibility > visiUpper
                ? cuArms + luArmVector * 1.0f
                : _nextLuArm;
            _nextRuArm = packet.landmark[(int)BlazePose.RightUpperArm].visibility > visiUpper
                ? cuArms + ruArmVector * 1.0f
                : _nextRuArm;

            // root and luLeg        
            var boneLuLegPos = _boneTransforms[(int)HumanBodyBones.LeftUpperLeg].position;
            var boneRuLegPos = _boneTransforms[(int)HumanBodyBones.RightUpperLeg].position;
            var luLeg = packet.landmark[(int)BlazePose.LeftUpperLeg].Position();
            var ruLeg = packet.landmark[(int)BlazePose.RightUpperLeg].Position();

            var isLRuLegsTracked = true;
            if (packet.landmark[(int)BlazePose.LeftUpperLeg].visibility < visiUpper ||
                packet.landmark[(int)BlazePose.RightUpperLeg].visibility < visiUpper)
            {
                // reuse last positions
                isLRuLegsTracked = false;
                _nextLuLeg = cuArms + _lastTrackedCuArmsToCuLegs + _lastTrackedLuLegVector;
                _nextRuLeg = cuArms + _lastTrackedCuArmsToCuLegs + _lastTrackedRuLegVector;
            }
            else
            {
                var cuLegs = (luLeg + ruLeg) * 0.5f;
                _lastTrackedLuLegVector = (luLeg - cuLegs).normalized * _luCenterToUperLegLength;
                _lastTrackedRuLegVector = (ruLeg - cuLegs).normalized * _luCenterToUperLegLength;
                _lastTrackedCuArmsToCuLegs = (cuLegs - cuArms).normalized * _boneCuArmsToCuLegsVector.magnitude;
                _nextLuLeg = cuArms + _lastTrackedCuArmsToCuLegs + _lastTrackedLuLegVector;
                _nextRuLeg = cuArms + _lastTrackedCuArmsToCuLegs + _lastTrackedRuLegVector;
            }

            // root
            _nextRoot = cuArms + _lastTrackedCuArmsToCuLegs * (1.0f - _upFromCuLegsToRootRate)
                               + Vector3.Cross(-_lastTrackedCuArmsToCuLegs, boneRuLegPos - boneLuLegPos).normalized *
                               _frontFromBoneCuLegsToRoot;

            // arm and hand
            if (packet.landmark[(int)BlazePose.LeftLowerArm].visibility > 0 &&
                packet.landmark[(int)BlazePose.LeftHand].visibility > 0)
            {
                var llArm = packet.landmark[(int)BlazePose.LeftLowerArm].Position();
                var lHand = packet.landmark[(int)BlazePose.LeftHand].Position();
                _lastTrackedLlArmVector = (llArm - luArm).normalized * _upperArmLength;
                _lastTrackedLHandVector = (lHand - llArm).normalized * _lowerArmLength;
            }

            _nextLlArm = _nextLuArm + _lastTrackedLlArmVector;
            _nextLHand = _nextLlArm + _lastTrackedLHandVector;

            if (packet.landmark[(int)BlazePose.RightLowerArm].visibility > 0 &&
                packet.landmark[(int)BlazePose.RightHand].visibility > 0)
            {
                var rlArm = packet.landmark[(int)BlazePose.RightLowerArm].Position();
                var rHand = packet.landmark[(int)BlazePose.RightHand].Position();
                _lastTrackedRlArmVector = (rlArm - ruArm).normalized * _upperArmLength;
                _lastTrackedRHandVector = (rHand - rlArm).normalized * _lowerArmLength;
            }

            _nextRlArm = _nextRuArm + _lastTrackedRlArmVector;
            _nextRHand = _nextRlArm + _lastTrackedRHandVector;

            // leg
            var llLeg = packet.landmark[(int)BlazePose.LeftLowerLeg].Position();
            var lFoot = packet.landmark[(int)BlazePose.LeftFoot].Position();
            if (isLRuLegsTracked &&
                packet.landmark[(int)BlazePose.LeftLowerLeg].visibility > 0)
                _lastTrackedLlLegVector = (llLeg - luLeg).normalized * _upperLegLength;
            _nextLlLeg = _nextLuLeg + _lastTrackedLlLegVector;
            if (isLRuLegsTracked &&
                packet.landmark[(int)BlazePose.LeftLowerLeg].visibility > 0 &&
                packet.landmark[(int)BlazePose.LeftFoot].visibility > 0)
                _lastTrackedLFootVector = (lFoot - llLeg).normalized * _lowerLegLength;
            _nextLFoot = _nextLlLeg + _lastTrackedLFootVector;

            var rlLeg = packet.landmark[(int)BlazePose.RightLowerLeg].Position();
            var rFoot = packet.landmark[(int)BlazePose.RightFoot].Position();
            if (isLRuLegsTracked &&
                packet.landmark[(int)BlazePose.RightLowerLeg].visibility > 0)
                _lastTrackedRlLegVector = (rlLeg - ruLeg).normalized * _upperLegLength;
            _nextRlLeg = _nextRuLeg + _lastTrackedRlLegVector;
            if (isLRuLegsTracked &&
                packet.landmark[(int)BlazePose.RightLowerLeg].visibility > 0 &&
                packet.landmark[(int)BlazePose.RightFoot].visibility > 0)
                _lastTrackedRFootVector = (rFoot - rlLeg).normalized * _lowerLegLength;
            _nextRFoot = _nextRlLeg + _lastTrackedRFootVector;
        }
    }
}