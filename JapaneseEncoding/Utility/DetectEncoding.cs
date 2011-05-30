using System;

namespace Japanese.Text.Utility
{
    public class EncodingDetecter
    {
        /// <summary>
        /// 判別したエンコード
        /// </summary>
        public enum DetectedEncoding
        {
            Unknown,
            ShiftJis,
            Jis,
            EucJp,
            Utf8,
            Utf16
        }

        #region Private Methods
        /// <summary>
        /// 統計からエンコーディングを推定します
        /// </summary>
        /// <param name="sjis">ShiftJISと読める文字数</param>
        /// <param name="eucjp">EUCJPと読める文字数</param>
        /// <param name="utf8">UTF8と読める文字数</param>
        /// <param name="currentPrediction">優先するエンコーディング</param>
        /// <returns>推定したエンコーディング</returns>
        private DetectedEncoding guess(uint sjis, uint eucjp, uint utf8, DetectedEncoding currentPrediction)
        {
            if (sjis > max(eucjp, utf8)) return DetectedEncoding.ShiftJis;
            if (eucjp > max(sjis, utf8)) return DetectedEncoding.EucJp;
            if (utf8 > max(sjis, eucjp)) return DetectedEncoding.Utf8;
            return currentPrediction == DetectedEncoding.Unknown ? DetectedEncoding.Utf8 : currentPrediction;
        }

        /// <summary>
        /// 2引数のうち大きい方を返す
        /// </summary>
        private uint max(uint p1, uint p2)
        {
            return p1 > p2 ? p1 : p2;
        }
        #endregion

        #region Properties
        /// <summary>
        /// JIS用Encodingクラス
        /// </summary>
        public Type JisEncoding { get; set; }
        /// <summary>
        /// ShiftJIS用Encodingクラス
        /// </summary>
        public Type SjisEncoding { get; set; }
        /// <summary>
        /// EUCJP用Encodingクラス
        /// </summary>
        public Type EucjpEncoding { get; set; }
        /// <summary>
        /// UTF-8用Encodingクラス
        /// </summary>
        public Type Utf8Encoding { get; set; }
        /// <summary>
        /// UTF-16用Encodingクラス
        /// </summary>
        public Type Utf16Encoding { get; set; }
        /// <summary>
        /// フォールバック用Encodingクラス
        /// </summary>
        public Type DefaultEncoding { get; set; }
        #endregion

        public EncodingDetecter()
        {
            JisEncoding = typeof(Encoding.JisEncoding);
            SjisEncoding = typeof(Encoding.SjisEncoding);
            EucjpEncoding = typeof(Encoding.EucjpEncoding);
            Utf8Encoding = typeof(System.Text.UTF8Encoding);
            Utf16Encoding = typeof(System.Text.UnicodeEncoding);
            DefaultEncoding = Utf8Encoding;
        }

        /// <summary>
        /// バイトシーケンスからエンコーディングを推定します
        /// </summary>
        /// <param name="bytes">推定に使用するバイトシーケンス</param>
        /// <param name="index">処理の開始位置</param>
        /// <param name="count">処理するバイト数</param>
        /// <returns>推定したエンコーディングを示すDetectedEncoding定数</returns>
        public DetectedEncoding DetectEncoding(byte[] bytes, int index, int count)
        {
            DetectedEncoding prediction = DetectedEncoding.Unknown, subprediction = DetectedEncoding.ShiftJis;
            uint utf8 = 0, sjis = 0, euc = 0;
            //const unsigned char* end = ptr + length;
            // UTF-16
            if (count > 2 && (bytes[index] == 0xff && bytes[index + 1] == 0xfe) || (bytes[index] == 0xfe && bytes[index + 1] == 0xff))
                return DetectedEncoding.Utf16;
            for (int i = 0; i < count; ++i)
            {
                // ISO2022JP
                if (bytes[i + index] == 0x1B)
                    return DetectedEncoding.Jis;

                // UTF-8/N
                if (bytes[i + index] >= 0xC2 && bytes[i + index] <= 0xFC)
                {
                    int u = i;
                    byte c = bytes[i + index];
                    uint n = 0;
                    bool b = true;
                    for (n = 0; ((c <<= 1) & 0x80) != 0; ++n) ; ++i;
                    for (uint j = 0; j < n && i != count; ++j, ++i)
                    {
                        if (bytes[i + index] < 0x80 || bytes[i + index] > 0xBF)
                        {
                            i = u;
                            b = false;
                            break;
                        }
                    }

                    if (i == count)
                        return prediction == DetectedEncoding.Unknown ? subprediction : guess(sjis, euc, utf8, prediction);

                    if (b)
                    {
                        prediction = DetectedEncoding.Utf8;
                        ++utf8;
                        --i;
                        continue;
                    }
                }

                // EUC半角カナ
                if (bytes[i + index] == 0x8E)
                {
                    // 終端に達した
                    if (++i == count)
                        return prediction == DetectedEncoding.Unknown ? subprediction : prediction;

                    // 2byte目の判別
                    if (bytes[i + index] >= 0xA1 && bytes[i + index] <= 0xDF)
                    {
                        if ((prediction == DetectedEncoding.EucJp) || (prediction == DetectedEncoding.EucJp))
                        {
                            prediction = DetectedEncoding.EucJp;
                            ++euc;
                        }
                    }
                    else
                    {
                        if (bytes[i + index] >= 0x80 && bytes[i + index] <= 0xA0)
                            return DetectedEncoding.ShiftJis;
                    }
                    --i;
                    continue;
                }

                // EUC補助漢字
                if (bytes[i + index] == 0x8F)
                {
                    int u = i;
                    for (int j = 0; j < 2 && i != count; ++j, ++i)
                    {
                        if (bytes[i + index] >= 0xA1 && bytes[i + index] <= 0xFE)
                        {
                            if ((prediction == DetectedEncoding.EucJp) || (prediction == DetectedEncoding.Unknown))
                            {
                                prediction = DetectedEncoding.EucJp;
                                ++euc;
                            }
                        }
                        else
                        {
                            if (bytes[i + index] <= 0x80 && bytes[i + index] >= 0xA0)
                                return DetectedEncoding.ShiftJis;
                        }
                    }
                    if (i == count)
                        return prediction == DetectedEncoding.Unknown ? subprediction : guess(sjis, euc, utf8, prediction);

                    i = u;
                    continue;
                }

                // SJIS
                if (bytes[i + index] >= 0x80 && bytes[i + index] <= 0xA0)
                    return DetectedEncoding.ShiftJis;

                // SJIS半角カナ
                if (bytes[i + index] >= 0xA1 && bytes[i + index] <= 0xDF && prediction == DetectedEncoding.Unknown)
                {
                    int u = i;
                    if (++i == count)
                        return prediction == DetectedEncoding.Unknown ? subprediction : prediction;
                    if ((bytes[u + index] == 0xA4 || bytes[u + index] == 0xA5) && (bytes[i + index] >= 0xA1 && bytes[i + index] <= 0xF6))
                    {
                        subprediction = DetectedEncoding.EucJp;
                        continue;
                    }
                    if (bytes[i + index] == 0xFD || bytes[i + index] == 0xFE)
                        return DetectedEncoding.EucJp;
                    if (bytes[i + index] >= 0xE0 && bytes[i + index] <= 0xFE)
                    {
                        prediction = DetectedEncoding.EucJp;
                        ++euc;
                    }
                    else
                    {
                        prediction = DetectedEncoding.ShiftJis;
                        ++sjis;
                    }
                    --i;
                    continue;
                }

                // EUC
                if (bytes[i + index] >= 0xA1 && bytes[i + index] <= 0xFE)
                {
                    if (bytes[i + index] == 0xFD || bytes[i + index] == 0xFE)
                        return DetectedEncoding.EucJp;
                    prediction = DetectedEncoding.EucJp;
                    ++euc;
                }
            }

            return prediction == DetectedEncoding.Unknown ? subprediction : guess(sjis, euc, utf8, prediction);
        }

        /// <summary>
        /// バイトシーケンスからエンコーディングを推定します
        /// </summary>
        /// <param name="bytes">推定に使用するバイトシーケンス</param>
        /// <returns>推定したエンコーディングを示すDetectedEncoding定数</returns>
        public DetectedEncoding DetectEncoding(byte[] bytes)
        {
            return DetectEncoding(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// バイトシーケンスからエンコーディングを推定して適切なSystem.Text.Encodingクラスを生成します
        /// </summary>
        /// <param name="bytes">推定に使用するバイトシーケンス</param>
        /// <param name="index">処理の開始位置</param>
        /// <param name="count">処理するバイト数</param>
        /// <returns>バイトシーケンスから推定されたSystem.Text.Encodingクラス</returns>
        public System.Text.Encoding GetEncoding(byte[] bytes, int index, int count)
        {
            DetectedEncoding encoding = DetectEncoding(bytes, index, count);
            switch (encoding)
            {
                default:
                case DetectedEncoding.Unknown:
                    break;
                case DetectedEncoding.Utf8:
                    return Activator.CreateInstance(Utf8Encoding) as System.Text.Encoding;
                case DetectedEncoding.Jis:
                    return Activator.CreateInstance(JisEncoding) as System.Text.Encoding;
                case DetectedEncoding.ShiftJis:
                    return Activator.CreateInstance(SjisEncoding) as System.Text.Encoding;
                case DetectedEncoding.EucJp:
                    return Activator.CreateInstance(EucjpEncoding) as System.Text.Encoding;
                case DetectedEncoding.Utf16:
                    return Activator.CreateInstance(Utf16Encoding) as System.Text.Encoding;
            }
            return Activator.CreateInstance(DefaultEncoding) as System.Text.Encoding;
        }

        /// <summary>
        /// バイトシーケンスからエンコーディングを推定して適切なSystem.Text.Encodingクラスを生成します
        /// </summary>
        /// <param name="bytes">推定に使用するバイトシーケンス</param>
        /// <returns>バイトシーケンスから推定されたSystem.Text.Encodingクラス</returns>
        public System.Text.Encoding GetEncoding(byte[] bytes)
        {
            return GetEncoding(bytes, 0, bytes.Length);
        }
    }
}