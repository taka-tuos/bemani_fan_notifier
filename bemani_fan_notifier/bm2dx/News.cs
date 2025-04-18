using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace bemani_fan_notifier.bm2dx
{
    /// <summary>
    /// ニュースを解析していい感じにする
    /// </summary>
    /// <param name="element"></param>
    internal class News(AngleSharp.Dom.IElement element)
    {
        /// <summary>
        /// li class="i_05"とかの部分
        /// </summary>
        readonly AngleSharp.Dom.IElement element = element;

        /// <summary>
        /// 最終JSON
        /// </summary>
        WebHook.Json json = new();

        /// <summary>
        /// embed
        /// </summary>
        WebHook.Embed embed = new();

        public WebHook.Json Dump(out string hash)
        {
            // news-mainを取る
            var wrapper = element.Children[0];

            // いくつめのliかで処理が変わる
            int index = 0;
            
            // OuterHtmlで厳密にできるらしい(temp=1.20のgpt-4.1談)、生なのか再シリアライズしたやつなのかは未確認
            hash = Utils.GetSHA256HashString(wrapper.OuterHtml);

            // 初期化ァ
            json.embeds = [];

            // OTHERとかEVENTとかNEW SONGとか
            string? newsclass = element.GetAttribute("class");

            if (newsclass != null)
            {
                embed.color = newsclass switch
                {
                    "i_01" => 0x0084ff,  // NEWSONG
                    "i_02" => 0x26ca00,  // RANKING
                    "i_03" => 0xff7800,  // EVENT
                    "i_04" => 0xff00ae,  // SHOP
                    _ => (int?)0xff0000, // OTHER
                };
            }
            else
            {
                // nullだったらOTHER色に
                embed.color = 0xff0000;
            }

            // news-main内の各liを舐めていく
            foreach (var child in wrapper.Children)
            {
                if(child is AngleSharp.Html.Dom.IHtmlListItemElement)
                {
                    // 0-base
                    DumpListItem(child, index++);
                }
            }

            json.embeds.Add(embed);

            return json;
        }

        /// <summary>
        /// 相対パスを直す
        /// </summary>
        /// <param name="href"></param>
        /// <returns></returns>
        private static string FormatHref(string href)
        {
            return href.Replace("about://", "https://p.eagate.573.jp");
        }

        /// <summary>
        /// 各liの処理
        /// </summary>
        /// <param name="element"></param>
        /// <param name="idx"></param>
        public void DumpListItem(AngleSharp.Dom.IElement element, int idx)
        {
            var children = element.ChildNodes;

            bool first = true;

            // 各要素を舐めていく
            foreach (var child in children)
            {
                // スペースたくさんつけてインデントしてきよるので、行頭のだけは滅する
                string rawcontent = first ? child.TextContent.TrimStart() : child.TextContent;

                // 加工した現在の要素が乗ってくる
                string content = "";

                // テキスト　そのまま
                if (child.NodeName == "#text")
                {
                    content = rawcontent;
                }
                // <br/>は改行に
                else if (child.NodeName == "BR")
                {
                    content = "\n";
                    first = true;
                }
                // リンクはMarkdownにする
                else if (child.NodeName == "A")
                {
                    string href = FormatHref(((AngleSharp.Html.Dom.IHtmlAnchorElement)child).Href);
                    content = $"[{rawcontent}]({href})";
                }
                // li内のpは赤字(コナミ税が必要ですetc)なので太字にしておく
                else if (child.NodeName == "P")
                {
                    content = "\n\n**" + rawcontent + "**";
                }

                // idx=0, 日付
                if (idx == 0)
                {
                    // フッタに設定
                    // timestampにすると時刻をよしなにしないといけないのでめんどくさい(日付しか入ってない)
                    embed.footer = new WebHook.Footer()
                    {
                        text = content.Replace('/', '-')
                    };
                }
                // idx=1, ニュースタイトル
                else if (idx == 1)
                {
                    // タイトルを設定
                    embed.title = rawcontent;
                    // aタグだったらリンクも設定
                    // rawcontentはAタグの場合でも囲まれた中身がちゃんと出てくる
                    if(child.NodeName == "A")
                    {
                        embed.url = FormatHref(((AngleSharp.Html.Dom.IHtmlAnchorElement)child).Href);
                    }
                }
                // idx=2, 本文
                else
                {
                    // そのまま本文エリアに突っ込んでいく
                    embed.description += content;
                }
            }
        }
    }
}
