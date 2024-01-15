using System;

namespace StackBoard
{
    /// <summary>
    /// Deprecated
    /// </summary>
    [Serializable]
    public class Picture : Node
    {
        public string url { get; private set; }

        private Picture() { }

        public Picture(string name, string url, string description) : base(name, description, typeof(Picture)) {
            this.url = url;
        }
    }
}



