using System;

namespace Japanese.Text.Utility.Validator
{
    /// <summary>
    /// byte[]を任意のエンコードとして正しいかをチェックするクラス
    /// </summary>
    public abstract class SequenceValidator
    {
        /// <summary>
        /// byte[]は任意のエンコードとして正しいか調べる
        /// </summary>
        /// <param name="bytes">検査する配列</param>
        /// <returns>任意のエンコードとして正しい場合はtrue</returns>
        public virtual bool Validate(byte[] bytes)
        {
            return Validate(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// byte[]の指定した範囲が任意のエンコードとして正しいか調べる
        /// </summary>
        /// <param name="bytes">検査する配列</param>
        /// <param name="index">検査を始めるインデックス</param>
        /// <param name="count">検査する文字数</param>
        /// <returns>任意のエンコードとして正しい場合はtrue</returns>
        public abstract bool Validate(byte[] bytes, int index, int count);

        /// <summary>
        /// byte[]の末尾が任意のエンコードシーケンスの切れ目で終わっているかを調べる
        /// </summary>
        /// <param name="bytes">検査する配列</param>
        /// <returns>末尾がシーケンスの切れ目で終わっている場合はtrue</returns>
        public virtual bool EndsValidSequence(byte[] bytes)
        {
            return EndsValidSequence(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// byte[]の指定した範囲の末尾が任意のエンコードシーケンスの切れ目で終わっているかを調べる
        /// </summary>
        /// <param name="bytes">検査する配列</param>
        /// <param name="index">検査を始めるインデックス</param>
        /// <param name="count">検査する文字数</param>
        /// <returns>末尾がシーケンスの切れ目で終わっている場合はtrue</returns>
        public abstract bool EndsValidSequence(byte[] bytes, int index, int count);
    }
}
