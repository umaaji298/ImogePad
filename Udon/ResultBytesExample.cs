using System.Text;
using System.Text.RegularExpressions;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.StringLoading;
using VRC.SDK3.Data;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using TMPro;


namespace ikaikurauniFactory.imogePad
{
    public class ResultBytesExample : UdonSharpBehaviour
    {
        //[SerializeField]
        //private VRCUrl url;
        // URLを入力するInputField
        [SerializeField] private VRCUrlInputField _urlInputfield;
        [UdonSynced(UdonSyncMode.None)] private VRCUrl _url;

        [SerializeField]
        public ShiftJISToUTF8Dictionary dictionary; // 別ファイルの辞書クラスを参照

        [SerializeField]
        public TextMeshProUGUI textMeshPro;

        [SerializeField]
        public ImageDownloader _downloader;

        [SerializeField]
        public TMP_InputField _inputField;  //InputFieldオブジェクト

        // ダウンロードしたテクスチャの反映先
        [SerializeField] private RawImage _target;

        [SerializeField] public Texture2D _defaultImage;
        [SerializeField] public Texture2D _successImage;
        [SerializeField] public Texture2D _errorImage;

        void Start()
        {
            //VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);
        }

        public void OnImageURLChanged()
        {
            Debug.Log("Interact");

            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);

            _url = _urlInputfield.GetUrl();
            _urlInputfield.SetUrl(VRCUrl.Empty);

            RequestSerialization();
            DownloadString();
        }

        public override void OnDeserialization()
        {
            DownloadString();
        }

        public void DownloadString()
        {
            //TODO このタイミングで入力データをチェックする

            //タスク起動
            VRCStringDownloader.LoadUrl(_url, (IUdonEventReceiver)this);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            //UTF8
            //string resultAsUTF8 = result.Result;
            //Debug.Log($"UTF8: {resultAsUTF8}");

            //ASCII
            //byte[] resultAsBytes = result.ResultBytes;
            //string resultAsASCII = Encoding.ASCII.GetString(resultAsBytes);
            //Debug.Log($"ASCII: {resultAsASCII}");

            // テクスチャ反映
            ApplyTexture(_successImage);

            byte[] resultAsBytes = result.ResultBytes;
            string resultAsUTF8 = ConvertShiftJISToUTF8(resultAsBytes);
            Debug.Log($"SJIS: {resultAsUTF8}");

            //string testHtml = "<html><body><blockquote>これは引用文です。</blockquote><p>本文</p><blockquote>別の引用文。</blockquote></body></html>";

            ///
            string extractedURL = ExtractSurega(resultAsUTF8);
            //これは不可能
            _inputField.text = extractedURL;

            ////inputfield 有効化
            //inputField.interactable = true;
            //inputField.Select();

            Debug.Log("surega:\n" + extractedURL);


            string extractedText = ExtractBlockquote(resultAsUTF8);
            Debug.Log("抽出結果:\n" + extractedText);


            string fixText = FixString(extractedText);

            textMeshPro.text = fixText;
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            Debug.LogError($"Error loading string: {result.ErrorCode} - {result.Error}");
        }

        public string ConvertShiftJISToUTF8(byte[] sjisBytes)
        {
            StringBuilder utf8Result = new StringBuilder();

            int i = 0;
            while (i < sjisBytes.Length)
            {
                byte b1 = sjisBytes[i];

                // ASCII文字（0x00～0x7F）はそのまま
                if (b1 < 0x80)
                {
                    utf8Result.Append((char)b1);
                    i++;
                }
                // 半角カナ
                else if (b1 >= 0xA0 && b1 <= 0xDF)
                {
                    if (dictionary.sjisToUtf8.ContainsKey((int)b1))
                    {
                        utf8Result.Append(dictionary.sjisToUtf8[(int)b1]);
                    }
                    else
                    {
                        utf8Result.Append("[?]"); // 未登録の文字は代替
                    }
                    i++;
                }
                // Shift-JISの2バイト文字（0x81～0x9F または 0xE0～0xFC）
                else if ((b1 >= 0x81 && b1 <= 0x9F) || (b1 >= 0xE0 && b1 <= 0xFC))
                {
                    if (i + 1 < sjisBytes.Length)
                    {
                        byte b2 = sjisBytes[i + 1];
                        int sjisCode = (b1 << 8) | b2;

                        if (dictionary.sjisToUtf8.ContainsKey(sjisCode))
                        {
                            utf8Result.Append(dictionary.sjisToUtf8[sjisCode]);
                        }
                        else
                        {
                            utf8Result.Append("[?]"); // 未登録の文字は代替
                        }

                        i += 2;
                    }
                    else
                    {
                        utf8Result.Append("[?]"); // バイト列が途中で途切れた場合
                        i++;
                    }
                }
                else
                {
                    utf8Result.Append("[?]"); // その他の未知の文字
                    i++;
                }
            }

            return utf8Result.ToString();
        }

        public string ExtractSurega(string html)
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

            return result.Trim();
        }

        public string FixString(string text)
        {
            // TODO StringBuilderによる高速化

            //URLタグ無効化
            text = text.Replace("</a>", "");
            text = Regex.Replace(text, "<a.*>", "");

            //font color 置き換え
            text = text.Replace("\"", "");
            text = text.Replace("</font>", "</color>");
            text = text.Replace("font ", "");

            //実体参照系
            //これ以外のやつはとりあえず無視
            text = text.Replace("&gt;", ">");

            return text;
        }

        // ダウンロードしたテクスチャを割り当て
        private void ApplyTexture(Texture2D texture)
        {
            if (_target == null) return;

            // テクスチャ反映
            _target.texture = texture;

            // サイズ変更
            _target.rectTransform.sizeDelta = new Vector2(_target.texture.width, _target.texture.height);
        }
    }
}