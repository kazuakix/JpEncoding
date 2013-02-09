namespace Japanese.Text.Encoding
{
    public class EucjpEncoding : System.Text.Encoding
    {
        const char FallbackChar = '?';
        const byte FallbackByte = 0x3f;

        // 2バイトのeucjpをsjisに変換する
        private void eucjp2sjis(ref byte sjis, ref byte sjis2)
        {
            if((sjis & 0x01) != 0){
                sjis >>= 1;
                if(sjis < 0x6F)
                    sjis += 0x31;
                else
                    sjis += 0x71;
                if(sjis2 > 0xDF)
                    sjis2 -= 0x60;
                else
                    sjis2 -= 0x61;
            }else{
                sjis >>= 1;
                if(sjis < 0x6F)
                    sjis += 0x30;
                else
                    sjis += 0x70;
                sjis2 -= 0x02;
            }
        }

        // 2バイトのsjisをeucjpに変換する
        private void sjis2eucjp(ref byte euc1, ref byte euc2)
        {
            // こちらのコードを使っています
            // http://homepage3.nifty.com/aokura/src/ms2euc.html

            euc1 <<= 1;
            if (euc2 < 0x9f)
            {
                if (euc1 < 0x3f)    euc1 -= 0x61;
                else                euc1 += 0x1f;

                if (euc2 > 0x7E)    euc2 += 0x60;
                else                euc2 += 0x61;
            }
            else
            {
                if (euc1 < 0x3F)    euc1 -= 0x60;
                else                euc1 += 0x20;

                                    euc2 += 0x02;
            }
        }

        // sjisからeucjpに変換したときのバイト数を返す
        private int getCountSjis2Eucjp(byte[] sjis, int index, int count)
        {
            int n = 0;
            for (int i = 0; i < count; ++i)
            {
                byte c1 = sjis[index + i];
                if (c1 < 0x80 || (c1 > 0xA0 && c1 < 0xDF) || c1 == 0xff)
                {
                    // 半角カナへ切り替え
                    if (c1 > 0xA0 && c1 < 0xE0)
                        n++;
                    n++;
                    continue;
                }
                if (++i == count)
                {
                    n++;
                    break;
                }
                byte c2 = sjis[index + i];
                if (c1 < 0x40 || (c1 > 0x7E && c1 < 0x80) || c1 > 0xFC)
                {
                    n += 2;
                    continue;
                }
                sjis2eucjp(ref c1, ref c2);
                n += 2;
            }
            return n;
        }

        // sjisからeucjpに変換する
        private byte[] sjis2eucjp(byte[] sjis, int index, int count)
        {
            byte[] buffer = new byte[getCountSjis2Eucjp(sjis, index, count)];
            int n = 0;
            for (int i = 0; i < count; ++i)
            {
                byte c1 = sjis[index + i];
                if (c1 < 0x80 || (c1 > 0xA0 && c1 < 0xDF) || c1 == 0xff)
                {
                    // 半角カナへ切り替え
                    if (c1 > 0xA0 && c1 < 0xE0)
                        buffer[n++] = 0x8E;
                    buffer[n++] = c1;
                    continue;
                }
                if (++i == count)
                {
                    buffer[n++] = FallbackByte;
                    break;
                }
                byte c2 = sjis[index + i];
                if (c1 < 0x40 || (c1 > 0x7E && c1 < 0x80) || c1 > 0xFC)
                {
                    buffer[n++] = c1;
                    buffer[n++] = c2;
                    continue;
                }
                sjis2eucjp(ref c1, ref c2);
                buffer[n++] = c1;
                buffer[n++] = c2;
            }
            return buffer;
        }

        // eucjpからsjisに変換したときのバイト数を返す
        private int getCountEucjp2Sjis(byte[] eucjp, int index, int count)
        {
            int n = 0;
            for (int i = 0; i < count; ++i)
            {
                byte c1 = eucjp[index + i];
                if (c1 < 0x80)
                {
                    n++;
                    continue;
                }
                if (++i == count)
                {
                    n++;
                    break;
                }
                byte c2 = eucjp[index + i];
                if (c1 == 0x8E)
                {
                    n++;
                    continue;
                }
                n += 2;
            }
            return n;
        }

        // eucjpからsjisに変換する
        private byte[] eucjp2sjis(byte[] eucjp, int index, int count)
        {
            byte[] buffer = new byte[getCountEucjp2Sjis(eucjp, index, count)];
            int n = 0;
            for (int i = 0; i < count; ++i)
            {
                byte c1 = eucjp[index + i];
                if (c1 < 0x80)
                {
                    buffer[n++] = c1;
                    continue;
                }
                if (++i == count)
                {
                    buffer[n++] = FallbackByte;
                    break;
                }
                byte c2 = eucjp[index + i];
                if (c1 == 0x8E)
                {
                    buffer[n++] = c2;
                    continue;
                }
                eucjp2sjis(ref c1, ref c2);
                buffer[n++] = c1;
                buffer[n++] = c2;
            }
            return buffer;
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            SjisEncoding e = new SjisEncoding();
            byte[] sjis = e.GetBytes(chars, index, count);
            return getCountSjis2Eucjp(sjis, 0, sjis.Length);
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            SjisEncoding e = new SjisEncoding();
            byte[] sjis = e.GetBytes(chars, charIndex, charCount);
            byte[] euc = sjis2eucjp(sjis, 0, sjis.Length);
            for (int i = 0; i < euc.Length; ++i)
                bytes[i + byteIndex] = euc[i];
            return euc.Length;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            SjisEncoding e = new SjisEncoding();
            byte[] sjis = eucjp2sjis(bytes, index, count);
            return e.GetCharCount(sjis, 0, sjis.Length);
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            SjisEncoding e = new SjisEncoding();
            byte[] sjis = eucjp2sjis(bytes, byteIndex, byteCount);
            return e.GetChars(sjis, 0, sjis.Length, chars, charIndex);
        }

        public override int GetMaxByteCount(int charCount)
        {
            return charCount * 2;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }
    }
}