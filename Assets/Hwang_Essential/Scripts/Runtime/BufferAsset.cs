using System;

namespace UnityEngine
{
    public class BufferAsset : TextAsset
    {
        private string _text;
        public new string text
        {
            get { return _text; }
        }

        private byte[] _bytes;
        public new byte[] bytes
        {
            get { return _bytes; }
        }

        public BufferAsset()
        {
        }

        public BufferAsset(string text)
        {
            _text = text;
        }

        public BufferAsset(byte[] bytes)
        {
            _bytes = bytes;
        }
    }
}
