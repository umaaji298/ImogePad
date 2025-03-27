using System.Text;
using System.Text.RegularExpressions;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TestFixString : UdonSharpBehaviour
{
    void Start()
    {
        string sample = "<font color=\"#789922\">&gt;スレ被ってるのに勢いが凄い</font><br><font color=\"#789922\">&gt;http://img.2chan.net/b/res/1295501352.htm</font><br>ごめん確認してなかった";

        string outstr = FixString(sample);

        Debug.Log(outstr);
    }

    public string FixString(string text)
    {
        // StringBuilder を使用して文字列を操作
        StringBuilder sb = new StringBuilder(text);

        // URLタグ無効化
        sb.Replace("</a>", "");

        // 正規表現で <a.*> を削除
        sb = new StringBuilder(Regex.Replace(sb.ToString(), "<a.*>", ""));

        // font color 置き換え
        sb.Replace("\"", "");
        sb.Replace("</font>", "</color>");
        sb.Replace("<font color", "<color");

        // 実体参照系
        sb.Replace("&gt;", ">");

        return sb.ToString();
    }
}
