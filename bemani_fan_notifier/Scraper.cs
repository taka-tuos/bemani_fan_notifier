using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bemani_fan_notifier
{
    /// <summary>
    /// Scraperインターフェイス
    /// Scraperは機種に対して1つなので、どの作品のサイトでもスクレイピングできる事を期待している
    /// </summary>
    internal interface IScraper
    {
        void Init(string url);
        Task<bool> Execute();
    }
}
