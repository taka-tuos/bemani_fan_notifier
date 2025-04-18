using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace bemani_fan_notifier
{
    /// <summary>
    /// 各種ユーティリティ関数群
    /// </summary>
    internal static class Utils
    {
        static readonly HashAlgorithm hashAlgorithm = SHA256.Create();

        /// <summary>
        /// SHA256を計算(byte[]版)
        /// </summary>
        /// <param name="value">計算対象</param>
        /// <returns></returns>
        public static ulong GetSHA256HashValue(string value)
        => BitConverter.ToUInt64(hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value)), 0);

        /// <summary>
        /// SHA256を計算(string版)
        /// </summary>
        /// <param name="value">計算対象</param>
        /// <returns></returns>
        public static string GetSHA256HashString(string value)
        => string.Join("", hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value)).Select(x => $"{x:x2}"));

        /// <summary>
        /// 保存したハッシュ一覧を読み出し
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>ハッシュ一覧</returns>
        public static List<string> ReadSHA256HashListFromURL(string url)
        {
            List<string> list = [];

            Directory.CreateDirectory("hashes");

            try
            {
                // ちゃんとやらないと窓で動いて🐧で動かないとかその逆がある
                using StreamReader sr = new(Path.Combine("hashes", $"{GetSHA256HashString(url)}.txt"));

                // ファイルが無かったら下のcatchへ飛ぶ
                while (!sr.EndOfStream)
                {
                    string? rawline = sr.ReadLine();

                    // 空行も飛ばしたい
                    if (!string.IsNullOrEmpty(rawline))
                    {
                        // トリム
                        string line = rawline.Trim();

                        // 64文字(256/4)
                        if(line.Length == 64)
                        {
                            list.Add(line);
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // 滅殺
            }

            return list;
        }

        /// <summary>
        /// ハッシュ一覧を保存
        /// </summary>
        /// <param name="url"></param>
        /// <param name="list"></param>
        public static void WriteSHA256HashListFromURL(string url, List<string> list)
        {
            Directory.CreateDirectory("hashes");

            // StreamWriterはなにも指定しない場合追記にはならない
            using StreamWriter sw = new(Path.Combine("hashes", $"{GetSHA256HashString(url)}.txt"));

            // こう、ガッと
            foreach (string line in list)
            {
                if (line.Length == 64)
                {
                    sw.WriteLine(line);
                }
            }
        }
    }
}
