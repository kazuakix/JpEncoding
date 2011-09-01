using System;

namespace Japanese.Text.Utility.Validator
{
    public class Utf8SequenceValidator : SequenceValidator
    {
        public override bool Validate(byte[] bytes, int index, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                byte b = bytes[index + i];
                // UTF8 1byte
                if (b <= 0x7f) continue;
                // UTF8 2byte
                if (0xc2 <= b && b <= 0xbf)
                {
                    if (i + 1 < count) continue;
                    return false;
                }
                // UTF8 3byte
                if (0xe0 <= b && b <= 0xef)
                {
                    if (i + 2 < count) continue;
                    return false;
                }
                // UTF8 4byte
                if (0xf0 <= b && b <= 0xf7)
                {
                    if (i + 3 < count) continue;
                    return false;
                }
                // UTF8 5byte以上はエラー
                if (0xf8 <= b) return false;
            }
            return true;
        }

        public override bool EndsValidSequence(byte[] bytes, int index, int count)
        {
            if (count == 0) return true;
            byte b = bytes[index + count - 1];
            // UTF8 1byte
            if (b < 0x80) return true;
            // UTF8 2~byte
            if (b < 0x80 || 0xbf < b) return false;
            if (count < 2) return false;
            b = bytes[index + count - 2];
            if (0xc2 <= b && b <= 0xbf) return true;
            // UTF8 3~byte
            if (count < 3) return false;
            byte b2 = bytes[index + count - 3];
            if ((0xa0 <= b && b <= 0xbf) && (0xe0 <= b2 && b2 <= 0xef)) return true;
            if (b < 0x80 || 0xbf < b) return false;
            // UTF8 4byte
            if (count < 4) return false;
            b = b2;
            b2 = bytes[index + count - 4];
            if ((0x90 <= b && b <= 0xbf) && (0xf0 <= b2 && b2 <= 0xf7)) return true;
            return false;
        }
    }
}
