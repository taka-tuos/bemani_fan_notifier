using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace bemani_fan_notifier
{
    /// <summary>
    /// WebHook用ユーティリティ
    /// </summary>
    internal class WebHook
    {
        /// <summary>
        /// JSON全体の構造体
        /// </summary>
        public struct Json
        {
            public string? username { get; set; }
            public string? avatar_url { get; set; }
            public List<Embed>? embeds { get; set; }
        }

        /// <summary>
        /// embed部のクラス
        /// </summary>
        public struct Embed
        {
            public string? title { get; set; }
            public string? description { get; set; }
            public string? url { get; set; }
            public int? color { get; set; }
            public string? timestamp { get; set; }
            public Footer? footer { get; set; }
        }

        /// <summary>
        /// embedのフッタ
        /// </summary>
        public struct Footer
        {
            public string? text { get; set; }
            public string? icon_url { get; set; }
        }

        /// <summary>
        /// WebHookをぜんぶ蹴る
        /// </summary>
        /// <param name="json"></param>
        public static async void Kick(string json)
        {
            try
            {
                using StreamReader sr = new("webhooks.txt");

                // 舐める
                while (!sr.EndOfStream)
                {
                    string? rawline = sr.ReadLine();

                    if (!string.IsNullOrEmpty(rawline))
                    {
                        // Trimって便利だよねえ
                        string url = rawline.Trim();

                        // URLかどうかだけは見る
                        if (url.StartsWith("https://"))
                        {
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                            HttpClient client = new();

                            // jsonをPOST
                            using HttpResponseMessage response = await client.PostAsync(url, new StringContent(json, Encoding.UTF8));
                            response.EnsureSuccessStatusCode();
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // 例外爆殺！イカ娘
            }
            catch
            {
                // 例外爆殺！イカ娘 もっと!
            }
        }
    }
}
