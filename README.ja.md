# VirtualPoseCapture

[[English](README.md)]

これは、MediaPipeを使って、カメラでポースのトラッキングを行い、VRMのアバターを動かすアプリケーションです。

https://user-images.githubusercontent.com/85206/205860844-c4a0da29-6bbb-4a68-bcf5-455469892883.mov

アバターの身体の各部位を以下の方法で動かします。
- 体全体: MediaPipe Holistic(Pose)
- 手の指: MediaPipe Holistic(Hand)
- 頭の向き: MediaPipe Holistic(Face)
- 口: MediaPipe Holistic(Face)。口の開閉（唇の上下動作）のみ
- 目: カメラに視線を自動的に向け、ランダムにまばたきを行う

## 使用しているソフトウェアおよびライブラリとバージョン

- Unity: 2021.3.4f1
- MediaPipe Unity Plugin: v0.10.1
- FinalIK: Version 2.2 ([Unity Asset Store](https://assetstore.unity.com/packages/tools/animation/final-ik-14290))
- UniVRM: 0.107.0
- VRM model: Vita ([here](https://vroid.pixiv.help/hc/en-us/articles/360014900113-AvatarSample-F))

FinalIKは、商用製品です。そのため、このリポジトリにはFinalIKは含まれません。このアプリケーションを動かすには、FinalIKをUnityAssetStoreで購入する必要があります。

アバターの3Dモデルとして、VroidStudioのサンプルキャラであるVitaをこのリポジトリに含みます。

## How to build

プロジェクトをUnity Editorで開くと、MediaPipe Unity PluginとFinakIKがプロジェクトに含まれないため、エラーが表示されます。それぞれ以下のボタンを押して強制的にUnity Editorを開いてください。
- Unity Package Manager Error
  `Failed to resolve packages: Tarball package [com.github.homuler.mediapipe] cannot be found at path ...`
  `Continue` ボタンを押す
- Enter Safe Mode?
  `Ignore` ボタンを押す

### MediaPipe および MediaPipe Unity Pluginのセットアップ

1. [MediaPipe Unity Pluginのリリースページ](https://github.com/homuler/MediaPipeUnityPlugin/releases/tag/v0.10.1)から、 `com.github.homuler.mediapipe-0.10.1.tgz` をダウンロードしてください
2. ダウンロードしたファイルを`PackageFiles`フォルダにおいてください。
3. シェルを開いて、プロジェクトのフォルダに移動し以下のコマンドを実行して下さい。このコマンドは、tgzから使用するモデルをAssets/StreamingAssetsにコピーします。
```
./setup_models.sh
```
4. Unity Editorのメニュー `Assets` -> `Reimport All`を選択して読み込み直してください。

### FinakIKのセットアップ

FinalIKは事前にUnityAssetStoreで購入済みである必要があります。

1. Unity Editorのメニュー `Window` -> `Package Manager`を選択してPackage Managerを開いてください。
2. FinalIKを選択し、Importボタンを押す。ファイル選択ダイアログで全てのファイルを選択した状態でImportボタンを押してください。

### アバターの変更

Asset/VRMフォルダに.vrmファイルを置き、
シーンのVRMオブジェクトのスクリプトVRMLoadのPAHTプロパティにパスを入力してください。

## How to use

Unityでこのプロジェクトを開き、シーン`Assets/Scenes/Start Scene`を開き、実行してください。

iPhoneのように複数のカメラがある場合は、画面下のカメラボタンで、使用するカメラを切り替えることができます。

## LICENSE

このアプリケーションは、UnityからMediaPipeを使うためにMediaPipe Unity Pluginを利用します。それだけではなく、カメラの制御や画面表示部品の多くに、MediaPipe Unity Pluginのサンプルコードを利用しています。

このアプリケーションで利用しているライブラリのライセンスを以下に記します。
素晴らしいソフトウェア・ライブラリを公開していただいた方々に、深く感謝を申し上げます。

- MediaPipe ([Apache Licence 2.0](https://github.com/google/mediapipe/blob/e6c19885c6d3c6f410c730952aeed2852790d306/LICENSE))
- MediaPipe Unity Plugin ([MIT](https://github.com/homuler/MediaPipeUnityPlugin/blob/master/LICENSE))
- FontAwesome ([LICENSE](https://github.com/FortAwesome/Font-Awesome/blob/7cbd7f9951be31f9d06b6ac97739a700320b9130/LICENSE.txt))
- UniVRM ([MIT](https://github.com/vrm-c/UniVRM/blob/master/LICENSE.txt))
- Vita ([CC0](https://vroid.pixiv.help/hc/en-us/articles/360014900113-AvatarSample-F))
- FinalIK ([UnityAssetStore](https://assetstore.unity.com/packages/tools/animation/final-ik-14290)) ([UnityAsetStore EULA](https://unity.com/legal/as-terms))
