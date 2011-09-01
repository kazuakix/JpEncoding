using System;

namespace Japanese.Text.Utility.Validator
{
    public class SjisSequenceValidator : SequenceValidator
    {
        public override bool Validate(byte[] bytes, int index, int count)
        {
            bool isValid = true;
            for (int i = 0; i < count; i++)
            {
                byte b = bytes[index + i];
                if ((0x81 <= b && b <= 0x9F) || (0xE0 <= b && b <= 0xEF))
                {
                    if (i + 1 < count)
                    {
                        // Shift_JIS マルチバイト
                        b = bytes[index + (++i)];
                        if ((0x40 <= b && b < 0x7f) || (0x7f < b && b <= 0xfc))
                        {
                            // Nothing
                        }
                        else
                        {
                            isValid = false;
                            break;
                        }
                    }
                    else
                    {
                        isValid = false;
                        break;
                    }
                }
            }
            return isValid;
        }

        public override bool EndsValidSequence(byte[] bytes, int index, int count)
        {
            if (count == 0) return true;
            byte b = bytes[index + count - 1];
            if ((0x81 <= b && b <= 0x9F) || (0xE0 <= b && b <= 0xEF))
            {
                if (count > 1)
                {
                    b = bytes[index + count - 2];
                    if ((0x81 <= b && b <= 0x9F) || (0xE0 <= b && b <= 0xEF))
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
