using System;

namespace Japanese.Text.Utility.Validator
{
    public class Utf16SequenceValidator : SequenceValidator
    {
        public override bool Validate(byte[] bytes, int index, int count)
        {
            // UTF16で奇数サイズはおかしい
            if ((count & 1) != 0) return false;
            return true;
        }

        public override bool EndsValidSequence(byte[] bytes, int index, int count)
        {
            // UTF16で奇数サイズはおかしい
            if ((count & 1) != 0) return false;
            return true;
        }
    }
}
