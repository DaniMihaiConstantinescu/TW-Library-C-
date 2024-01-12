using System;

namespace StackBoard
{
    [Serializable]
    public abstract class Node
    {
        public Type type { get; private set; }
        public string title {get; private set;}
        public string description { get; private set; }
        public DateTime createdAt { get; private set; }

        protected Node() { }
        protected Node(string title, string description, Type type) 
        {           
            this.title = title;
            this.description = description;
            this.type = type;
            createdAt = DateTime.Now;
        }
    }
}



