# VirtualPoseCapture

[[日本語](README.ja.md)]

This is an application that uses MediaPipe to do pose tracking with the camera and move the avatar in VRM.

https://user-images.githubusercontent.com/85206/205860844-c4a0da29-6bbb-4a68-bcf5-455469892883.mov

Each part of the avatar's body is moved in the following ways
- Whole body: MediaPipe Holistic(Pose)
- Hand: MediaPipe Holistic(Hand)
- Head: MediaPipe Holistic(Face)
- Mouth: MediaPipe Holistic(Face). Only opening and closing of the mouth (up and down movement of the lips)
- Eyes: Automatically look at the camera and blink randomly

## Versions of software and libraries

- Unity: 2021.3.4f1
- MediaPipe Unity Plugin: v0.10.1
- FinalIK: Version 2.2 ([Unity Asset Store](https://assetstore.unity.com/packages/tools/animation/final-ik-14290))
- UniVRM: 0.107.0
- VRM model: Vita ([here](https://vroid.pixiv.help/hc/en-us/articles/360014900113-AvatarSample-F))
- uOSC: v2.1.0 ([here](https://github.com/hecomi/uOSC))
- UnityVMDRecorder ([here](https://github.com/hobosore/UnityVMDRecorder))

FinalIK is a commercial product. Therefore, FinalIK is not included in this repository. To run this application, FinalIK must be purchased from the UnityAssetStore.

Vita, an sample character from VroidStudio, is included in this repository as a 3D model for the avatar.

## How to build

When I open the project in the Unity Editor, I get an error message because the MediaPipe Unity Plugin and FinakIK are not included in the project. Please force the Unity Editor to open by pressing the following buttons respectively.
- Unity Package Manager Error
  `Failed to resolve packages: Tarball package [com.github.homuler.mediapipe] cannot be found at path ... `
  Press the `Continue` button.
- Press `Enter Safe Mode?
  Press the `Ignore` button.

### Setup MediaPipe and MediaPipe Unity Plugin.

1. from [the MediaPipe Unity Plugin release page](https://github.com/homuler/MediaPipeUnityPlugin/releases/tag/v0.10.1) Download `com.github.homuler.mediapipe-0.10.1.tgz`.
2. Place the downloaded files in the `PackageFiles` folder.
3. Open a shell, go to your project folder and execute the following command. This command will copy models from tgz to `Assets/StreamingAssets`.
```
./setup_models.sh
```
4. select the menu `Assets` -> `Reimport All` in the Unity Editor and reload it.

### Setting up FinakIK

FinalIK must have been purchased from the UnityAssetStore.

1. Open the Package Manager by selecting `Window` -> `Package Manager` from the Unity Editor menu.
2. Select FinalIK and press the Import button. Press the Import button with all files selected in the file selection dialog.

### Change the avatar

Place the .vrm file in the `Asset/VRM` folder
Enter the path to the PAHT property of the script VRMLoad of the VRM object in your scene.

## How to use

Open this project in Unity, open the scene `Assets/Scenes/Start Scene` and run it.

If you have multiple cameras like iPhone, you can use the camera button at the bottom of the screen to switch the camera to use.

## LICENSE

This application uses the MediaPipe Unity Plugin to use MediaPipe from Unity. Not only that, but it also uses the MediaPipe Unity Plugin sample code for many of the camera controls and screen display components.

Note that some files are distributed under other licenses.
We would like to express our deepest gratitude to those who have made their wonderful software libraries available to the public.

- MediaPipe ([Apache Licence 2.0](https://github.com/google/mediapipe/blob/e6c19885c6d3c6f410c730952aeed2852790d306/LICENSE))
- MediaPipe Unity Plugin ([MIT](https://github.com/homuler/MediaPipeUnityPlugin/blob/master/LICENSE))
- FontAwesome ([LICENSE](https://github.com/FortAwesome/Font-Awesome/blob/7cbd7f9951be31f9d06b6ac97739a700320b9130/LICENSE.txt))
- UniVRM ([MIT](https://github.com/vrm-c/UniVRM/blob/master/LICENSE.txt))
- Vita ([CC0](https://vroid.pixiv.help/hc/en-us/articles/360014900113-AvatarSample-F))
- FinalIK ([UnityAssetStore](https://assetstore.unity.com/packages/tools/animation/final-ik-14290)) ([UnityAsetStore EULA](https://unity.com/legal/as-terms))
- uOSC ([MIT](https://github.com/hecomi/uOSC/blob/master/LICENSE.md))
- UnityVMDRecorder ([MIT](https://github.com/hobosore/UnityVMDRecorder/blob/2019/LICENSE))
