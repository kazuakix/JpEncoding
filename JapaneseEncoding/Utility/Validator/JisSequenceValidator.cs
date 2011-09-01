using System;

namespace Japanese.Text.Utility.Validator
{
    public class JisSequenceValidator : SequenceValidator
    {
        enum EncodingState
        {
            NotSpecified,
            Kanji,
            Ascii,
            Romaji,
            Katakana,
        }

        public override bool Validate(byte[] bytes, int index, int count)
        {
            EncodingState es = EncodingState.Ascii;
            for (int i = index; i < (index + count); ++i)
            {
                // エスケープを検出
                if (bytes[i] == 0x1B)
                {
                    if (i + 2 <= (index + count))
                    {
                        byte firstLetter = bytes[i + 1], secondLetter = bytes[i + 2];
                        switch (firstLetter)
                        {
                            case 0x24: // $
                                switch (secondLetter)
                                {
                                    case 0x40: // @
                                    case 0x42: // B
                                        es = EncodingState.Kanji;
                                        i += 2;
                                        continue;
                                }
                                return false;
                            case 0x26: // &
                                if (secondLetter == 0x40) // @
                                {
                                    if (i + 5 <= (index + count))
                                    {
                                        if (bytes[i + 3] == 0x1B
                                         && bytes[i + 4] == 0x24
                                         && bytes[i + 5] == 0x42)
                                        {
                                            es = EncodingState.Kanji;
                                            i += 5;
                                            continue;
                                        }
                                    }
                                }
                                return false;
                            case 0x28: // (
                                switch (secondLetter)
                                {
                                    case 0x42:
                                        es = EncodingState.Ascii;
                                        i += 2;
                                        continue;
                                    case 0x4a:
                                        es = EncodingState.Romaji;
                                        i += 2;
                                        continue;
                                    case 0x49:
                                        es = EncodingState.Katakana;
                                        i += 2;
                                        continue;
                                }
                                return false;
                        }
                    }
                }

                // 文字をデコード
                switch (es)
                {
                    case EncodingState.NotSpecified:
                    case EncodingState.Ascii:
                    case EncodingState.Romaji:
                        break;
                    case EncodingState.Katakana:
                        if (bytes[i] < 0x21 || 0x5f < bytes[i])
                            return false;
                        break;
                    case EncodingState.Kanji:
                        if (0x21 <= bytes[i] && bytes[i] <= 0x7e)
                        {
                            if (i + 1 < (index + count))
                            {
                                if (bytes[i + 1] < 0x21 || 0x7e < bytes[i + 1])
                                {
                                    return false;
                                }
                                ++i;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                        break;
                }
            }
            return true;
        }

        public override bool EndsValidSequence(byte[] bytes, int index, int count)
        {
            EncodingState es = EncodingState.Ascii;
            for (int i = index; i < (index + count); ++i)
            {
                // エスケープを検出
                if (bytes[i] == 0x1B)
                {
                    if (i + 2 <= (index + count))
                    {
                        byte firstLetter = bytes[i + 1], secondLetter = bytes[i + 2];
                        switch (firstLetter)
                        {
                            case 0x24: // $
                                switch (secondLetter)
                                {
                                    case 0x40: // @
                                    case 0x42: // B
                                        es = EncodingState.Kanji;
                                        i += 2;
                                        continue;
                                }
                                break;
                            case 0x26: // &
                                if (secondLetter == 0x40) // @
                                {
                                    if (i + 5 <= (index + count))
                                    {
                                        if (bytes[i + 3] == 0x1B
                                         && bytes[i + 4] == 0x24
                                         && bytes[i + 5] == 0x42)
                                        {
                                            es = EncodingState.Kanji;
                                            i += 5;
                                            continue;
                                        }
                                    }
                                }
                                break;
                            case 0x28: // (
                                switch (secondLetter)
                                {
                                    case 0x42:
                                        es = EncodingState.Ascii;
                                        i += 2;
                                        continue;
                                    case 0x4a:
                                        es = EncodingState.Romaji;
                                        i += 2;
                                        continue;
                                    case 0x49:
                                        es = EncodingState.Katakana;
                                        i += 2;
                                        continue;
                                }
                                break;
                        }
                    }
                }

                // 文字をデコード
                switch (es)
                {
                    case EncodingState.NotSpecified:
                    case EncodingState.Ascii:
                    case EncodingState.Romaji:
                    case EncodingState.Katakana:
                        break;
                    case EncodingState.Kanji:
                        if (0x21 <= bytes[i] && bytes[i] <= 0x7e)
                        {
                            if (i + 1 == (index + count))
                            {
                                return false;
                            }
                        }
                        break;
                }
            }
            return true;
        }
    }
}
