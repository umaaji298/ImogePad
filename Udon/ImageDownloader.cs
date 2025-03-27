
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Image;
using VRC.SDK3.Components;

namespace ikaikurauniFactory.imogePad
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ImageDownloader : UdonSharpBehaviour
    {
        [UdonSynced(UdonSyncMode.None)] private VRCUrl _threadImageUrl;
        [SerializeField] private VRCUrlInputField _threadImageUrlInputfield;

        //スレッド画像表示先
        [SerializeField] private RawImage _threadImage;

        // 画像をダウンロードしてくれるクラス
        private VRCImageDownloader _imageDownloader;

        // ダウンロード実行タスク
        private IVRCImageDownload _downloadTask;

        private UdonBehaviour _udon;

        private void Start()
        {
            // クラス作成
            _imageDownloader = new VRCImageDownloader();
            _udon = this.transform.GetComponent<UdonBehaviour>();
        }

        private void OnDestroy()
        {
            // ちゃんと破棄する
            _imageDownloader.Dispose();
        }

        // インタラクトしたら、ダウンロード開始
        public void OnImageURLChanged()
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);

            _threadImageUrl = _threadImageUrlInputfield.GetUrl();
            if (_threadImageUrl == null || _threadImageUrl == VRCUrl.Empty) return;

            _threadImageUrlInputfield.SetUrl(VRCUrl.Empty);

            RequestSerialization();
            DownloadImage();
        }

        public override void OnDeserialization()
        {
            Debug.Log("Image call");

            if (_threadImageUrl != VRCUrl.Empty)
            {
                DownloadImage();
            }
        }

        public void DownloadImage()
        {
            //TODO このタイミングで入力データをチェックする

            // ダウンロードするテクスチャの設定
            TextureInfo info = GetTextureInfo();

            // ダウンロード実行
            _downloadTask = _imageDownloader.DownloadImage(_threadImageUrl, null, _udon, info);
        }


        // ダウンロード成功時
        public override void OnImageLoadSuccess(IVRCImageDownload result)
        {
            Debug.Log("Success");

            base.OnImageLoadSuccess(result);

            // テクスチャ反映
            ApplyTexture(result.Result);
        }

        // ダウンロード失敗時
        public override void OnImageLoadError(IVRCImageDownload result)
        {
            Debug.Log("Error");

            base.OnImageLoadError(result);
        }

        // ダウンロードしたテクスチャを割り当て
        private void ApplyTexture(Texture2D texture)
        {
            if (_threadImage == null) return;

            // テクスチャ反映
            _threadImage.texture = texture;

            // サイズ変更
            _threadImage.rectTransform.sizeDelta = new Vector2(_threadImage.texture.width, _threadImage.texture.height);
        }

        // テクスチャの設定情報を取得
        private TextureInfo GetTextureInfo()
        {
            // Mipmapを指定
            TextureInfo info = new TextureInfo();
            //info.MaterialProperty = "_MainTex";
            info.GenerateMipMaps = true;

            return info;
        }
    }
}
