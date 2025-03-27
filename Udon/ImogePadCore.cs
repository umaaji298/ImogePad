using System.Text;
using System.Text.RegularExpressions;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.StringLoading;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using TMPro;

namespace ikaikurauniFactory.imogePad
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ImogePadCore : UdonSharpBehaviour
    {
        //imogeスレッドURL
        [UdonSynced(UdonSyncMode.None)] private VRCUrl _threadUrl;
        [Header("text"), SerializeField] private VRCUrlInputField _threadUrlInputfield;

        //binary->sjis encoder
        [SerializeField] private ShiftJISToUTF8Dictionary _sjisEncoder;

        //スレッド本文表示先
        [SerializeField] private TextMeshProUGUI _threadTextUGUI;

        //スレッド画像URL出力先(Userコピー用)
        [Header("image"), SerializeField] private TMP_InputField _thredImageUrlUGUI;

        //スレッド画像表示先
        [SerializeField] private RawImage _threadImage;

        //エラー表示など
        [Header("info"), SerializeField] public Texture2D _defaultImage;
        [SerializeField] public Texture2D _successImage;
        [SerializeField] public Texture2D _errorImage;

        //Sound
        [SerializeField] public AudioSource _audio;

        void Start()
        {
            //
        }

        public void OnThreadURLChanged()
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);

            _threadUrl = _threadUrlInputfield.GetUrl();
            if (_threadUrl == null || _threadUrl == VRCUrl.Empty) return;

            _threadUrlInputfield.SetUrl(VRCUrl.Empty);

            RequestSerialization();
            DownloadThread();
        }

        public override void OnDeserialization()
        {
            Debug.Log("Core call");

            if (_threadUrl != VRCUrl.Empty)
            {
                DownloadThread();
            }
        }

        public void DownloadThread()
        {
            //TODO 入力データをチェックするならこのタイミング

            //タスク起動
            //https://creators.vrchat.com/worlds/udon/string-loading/
            //memo ImageDownloaderと違いDisposeとかはいらんの？
            VRCStringDownloader.LoadUrl(_threadUrl, (IUdonEventReceiver)this);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            byte[] resultAsBytes = result.ResultBytes;

            // Info テクスチャ反映
            ApplyTexture(_successImage);

            // Info audio再生
            _audio.PlayOneShot(_audio.clip);

            //Sjis変換
            //memo 非常に重いのでどこかで遅延実行必要かも
            string resultAsUTF8 = _sjisEncoder.Convert(resultAsBytes);
            //Debug.Log($"SJIS: {resultAsUTF8}");

            //スレ画URL抽出
            string extractedImageURL = ExtractImageUrl(resultAsUTF8);
            _thredImageUrlUGUI.text = extractedImageURL;
            //Debug.Log("surega:\n" + extractedImageURL);

            //スレ本文抽出
            string extractedText = ExtractBlockquote(resultAsUTF8);
            Debug.Log("抽出結果:\n" + extractedText);

            //TextMeshPro用に整形
            string fixText = FixString(extractedText);
            _threadTextUGUI.text = fixText;
            Debug.Log("結果:\n" + fixText);
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            // Info テクスチャ反映
            ApplyTexture(_errorImage);
            Debug.LogError($"Error loading string: {result.ErrorCode} - {result.Error}");
        }

        private string ExtractImageUrl(string html)
        {
            string result = "";

            // 正規表現パターン
            string pattern = "<img src=\"([^\"]+)\"";
            Match match = Regex.Match(html, pattern);
            if (match.Success)
            {
                result = match.Groups[1].Value;
                Debug.Log("抽出された画像URL: " + result);
            }
            else
            {
                Debug.Log("画像URLが見つかりませんでした。");
            }

            //URL全体を補完
            result = "https://img.2chan.net" + result;

            return result;
        }

        public string ExtractBlockquote(string html)
        {
            string result = "";

            // <blockquote>タグの中身を抽出する正規表現
            Regex regex = new Regex(@"<blockquote[^>]*>(.*?)<\/blockquote>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            MatchCollection matches = regex.Matches(html);

            // UdonSharpでは foreach が使えないため for ループを使う
            int count = matches.Count;
            for (int i = 0; i < count; i++)
            {
                result += matches[i].Groups[1].Value + "\n ---- \n";
            }

            result += "(スレ取得完了)";

            return result.Trim();
        }

        public string FixString(string text)
        {
            //RegexpでURLタグを無効化
            string t = Regex.Replace(text, "<a href=\".*target=\"_blank\">", "");

            // StringBuilder を使用して文字列を操作
            StringBuilder sb = new StringBuilder(t);

            // URLタグ無効化
            sb.Replace("</a>", "");

            // font color 置き換え
            sb.Replace("<font color=\"", "<color=");
            sb.Replace("\">", ">");
            sb.Replace("</font>", "</color>");

            //// 実体参照系
            sb.Replace("&gt;", ">");

            return sb.ToString();
        }

        private void ApplyTexture(Texture2D texture)
        {
            if (_threadImage == null) return;

            // テクスチャ反映
            _threadImage.texture = texture;

            // サイズ変更
            _threadImage.rectTransform.sizeDelta = new Vector2(200,200);
        }
    }
}
