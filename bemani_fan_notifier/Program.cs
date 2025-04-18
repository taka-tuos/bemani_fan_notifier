using System.Net;

namespace bemani_fan_notifier
{
    internal class Program
    {
        /// <summary>
        /// Scraperのリスト
        /// </summary>
        static readonly KeyValuePair<string, Func<IScraper>>[] scrapers = [
                new( "bm2dx", () => new bm2dx.Scraper())
        ];

        static async Task Main(string[] _)
        {
            // 転ばぬ先のtry-catch
            try
            {
                // using句便利だよね
                using StreamReader sr = new("config.txt");

                // 1行づつ
                while (!sr.EndOfStream)
                {
                    // bool TryReadLine(out string line)みたいなのないの？？？
                    string? rawline = sr.ReadLine();

                    if(!string.IsNullOrEmpty(rawline))
                    {
                        // 割って整形して…
                        string[] tok = rawline.Split(',').Select(x => x.Trim()).ToArray();

                        // bm2dx,<URL> みたいな形式
                        if (tok.Length == 2)
                        {
                            // ぶん回す
                            foreach (var kvp in scrapers)
                            {
                                // 一致するScraperがあれば
                                if (kvp.Key == tok[0])
                                {
                                    // インスタンスを作って
                                    IScraper scraper = kvp.Value();

                                    // やる
                                    scraper.Init(tok[1]);
                                    await scraper.Execute();

                                    // 複数のScraperはないハズなので…
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // イカ娘以外にいいネタないんかな
            }
        }
    }
}
