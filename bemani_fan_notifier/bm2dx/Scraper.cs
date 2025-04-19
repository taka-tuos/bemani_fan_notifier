using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace bemani_fan_notifier.bm2dx
{
    /// <summary>
    /// Scraper (IIDX)
    /// 検証済み：30,31,32
    /// </summary>
    internal class Scraper : bemani_fan_notifier.IScraper
    {
        /// <summary>
        /// うｒｌ
        /// </summary>
        string url = "";

        /// <summary>
        /// ハッシュリスト
        /// </summary>
        List<string> hashes = [];

        /// <summary>
        /// nullのやつはjsonに出さない
        /// </summary>
        readonly JsonSerializerOptions options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="url">ベースURL(作品の数字で終わる)</param>
        public void Init(string url)
        {
            // 末尾のスラッシュは消しておく
            this.url = url.TrimEnd('/');

            // ハッシュリストを読んでおく
            hashes = Utils.ReadSHA256HashListFromURL(this.url);
        }

        /// <summary>
        /// スクレイピング実行
        /// </summary>
        /// <returns>適当にawaitでもして</returns>
        public async Task<bool> Execute()
        {
            // 作って
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            HttpClient client = new();

            // getする
            using HttpResponseMessage response = await client.GetAsync(url + "/info/index.html");

            // 転ばぬ先の
            try
            {
                // 500とかだったらここで例外が飛んで終了
                // AM5:00～AM7:00は絶対500が帰ってきます
                response.EnsureSuccessStatusCode();
                // なんでここまでAsyncなんすか？
                string body = await response.Content.ReadAsStringAsync();

                // ドカっとやる
                var parser = new AngleSharp.Html.Parser.HtmlParser();
                var doc = parser.ParseDocument(body);

                // ばらす
                var ulNodes = doc.QuerySelectorAll("ul[id='info-news']");
                var newsNodes = ulNodes[0].QuerySelectorAll(":scope > li");

                // 1ニュース毎に舐めていく
                foreach (var newselement in newsNodes)
                {
                    // Newsクラスを作って
                    News news = new(newselement);

                    // 解析してもらう
                    WebHook.Json json = news.Dump(out string hash);

                    // 名前とアイコンを設定
                    json.username = "eaChecker bm2dx";
                    // これ何用のURLなんやろ…
                    json.avatar_url = url + "/top_banner/myp_icon_PC.jpg";

                    // 尻アライズ！！
                    string json_string = JsonSerializer.Serialize(json, options);

                    // 今まで見たこと無いニュースか？
                    if (!hashes.Contains(hash))
                    {
                        Console.WriteLine($"New Hash: {hash}, hashes.Count={hashes.Count}");
                        
                        // ハッシュリストに足す
                        hashes.Add(hash);

                        // 標準出力に出す
                        //Console.WriteLine(json_string);

                        // WebHookを蹴る
                        WebHook.Kick(json_string);
                        
                        // 待つ
                        Thread.Sleep(500);
                    }
                }
            }
            catch
            {
                // どっかでエラーだったらfalse
                return false;
            }

            // ハッシュ一覧を書き戻す
            Utils.WriteSHA256HashListFromURL(url, hashes);

            return true;
        }
    }
}
