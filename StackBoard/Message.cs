
using System;

namespace StackBoard
{
    [Serializable]
    public class Message : Node
    {
        private Message() { }
        public Message(string name, string description) : base(name, description, typeof(Message)) { }
    }
}



