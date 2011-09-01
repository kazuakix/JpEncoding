using System;

namespace Japanese.Text.Utility.Validator
{
    public class EucjpSequenceValidator : SequenceValidator
    {
        public override bool Validate(byte[] bytes, int index, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                byte b = bytes[index + i];
                if (0xA1 <= b && b <= 0xFE)
                {
                    //漢字
                    //0xA1A1～0xFEFE (第1バイト・第2バイトとも0xA1～0xFE) 
                    if (i + 1 < count)
                    {
                        b = bytes[index + (++i)];
                        if (b < 0xA1 || 0xFE < b)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (b == 0x8E)
                {
                    //半角カタカナ
                    //0x8EA1～0x8EDF 
                    if (i + 1 < count)
                    {
                        b = bytes[index + (++i)];
                        if (b < 0xA1 || 0xDF < b)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (b == 0x8F)
                {
                    //補助漢字
                    //0x8FA1A1～0x8FFEFE (第2バイト・第3バイトとも0xA1～0xFE)
                    if (i + 2 < count)
                    {
                        b = bytes[index + (++i)];
                        if (b < 0xA1 || 0xFE < b)
                        {
                            return false;
                        }
                        b = bytes[index + (++i)];
                        if (b < 0xA1 || 0xFE < b)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override bool EndsValidSequence(byte[] bytes, int index, int count)
        {
            if (count == 0) return true;
            byte b = bytes[index + count - 1];
            if (0xA1 <= b && b <= 0xFE)
            {
                // 1文字戻る
                if (count > 1)
                {
                    byte b2 = bytes[index + count - 2];
                    if (b2 == 0x8E && b <= 0xDF) return true;
                    if (0xA1 <= b2 && b2 <= 0xFE)
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }
    }
}
