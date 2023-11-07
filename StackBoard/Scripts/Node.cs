using System;

namespace StackBoard
{
    [Serializable]
    public abstract class Node
    {
        public string title {get; private set;}
        public string description { get; private set; }
        public DateTime createdAt { get; private set; }

        protected Node() { }
        protected Node(string title, string description) 
        { 
            
            this.title = title;
            this.description = description;
            createdAt = DateTime.Now;
        }
    }
}



