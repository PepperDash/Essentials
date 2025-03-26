namespace PepperDash.Core.Intersystem.Tokens
{
    /// <summary>
    /// Represents the base class for all XSig datatypes.
    /// </summary>
    public abstract class XSigToken
    {
        private readonly int _index;

        /// <summary>
        /// Constructs an XSigToken with the specified index.
        /// </summary>
        /// <param name="index">Index for the data.</param>
        protected XSigToken(int index)
        {
            _index = index;
        }

        /// <summary>
        /// XSig 1-based index.
        /// </summary>
        public int Index
        {
            get { return _index; }
        }

        /// <summary>
        /// XSigToken type.
        /// </summary>
        public abstract XSigTokenType TokenType { get; }

        /// <summary>
        /// Generates the XSig bytes for the corresponding token.
        /// </summary>
        /// <returns>XSig byte array.</returns>
        public abstract byte[] GetBytes();

        /// <summary>
        /// Returns a new token if necessary with an updated index based on the specified offset.
        /// </summary>
        /// <param name="offset">Offset to adjust the index with.</param>
        /// <returns>XSigToken</returns>
        public abstract XSigToken GetTokenWithOffset(int offset);
    }
}