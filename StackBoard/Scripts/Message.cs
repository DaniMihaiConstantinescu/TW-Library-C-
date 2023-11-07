
using System;

namespace StackBoard
{
    [Serializable]
    public class Message : Node
    {
        public Message() { }
        public Message(string name, string description) : base(name, description) { }
    }
}



