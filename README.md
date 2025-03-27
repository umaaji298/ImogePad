# ImogePad
 VRChatでimgを見るPadです。  
 技術検証のために作成されました。


以下のワールドで実際に試すことができます。  
 https://vrchat.com/home/world/wrld_d6401ee0-7a5d-444b-9800-08f4c8aebf7e

## 使い方
- Padの一番上のTextBoxにimgスレURLを入れる
    - うまく行ったら成功画面が表示
    - した部分に簡易的にスレ内容が表示される
- (Option)スレ画を読み込む場合
    - スレ本文読み込み後に、2つ目のTextBoxにスレ画URLが表示されるのでコピーする
    - 3つ目のTextBoxにペーストする
    - うまくいったらスレ画が読み込まれる

# Unityへの追加方法

## 必須Package
UIにTextMeshProを利用しています。Unity内でTextMeshProを有効化してください。  
また、ワールド軽量化のためにFallbackフォントを活用します。  
こちらのプロジェクトからFallbackフォントをプロジェクトに追加する必要があります。  
https://github.com/Narazaka/tmp-fallback-fonts-jp

## Install
右側のReleaseからダウンロードしてください。  
imogePad_0.4.zip

zipを解答し、unitypackageをプロジェクトにインポートしてください。  
2.System -> ImogePad -> ImogePad のprefabをHierarchyに配置して使います。  

また  
0.Scenes > VRCDefaultWorldSceneを開くとpurfab配置済みのサンプルシーンがあります。

# License
「」　と　としあき　のみMIT Licenseの範囲内でお使いできます。  
それ以外の方、意味がわからない方は Not Freeライセンスとなります。
