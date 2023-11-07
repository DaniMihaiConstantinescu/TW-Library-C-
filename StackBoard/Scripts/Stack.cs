using System;
using System.Collections.Generic;

namespace StackBoard
{
    [Serializable]
    public class Stack
    {
        public string userAPIKey { get; private set; }



        public string name { get; private set; }      
        public DateTime createdAt { get; private set; }
        public List<Node> nodes { get; private set; }

        // Parameterless constructor required for serialization.
        public Stack() { }

        private Stack(string userAPIKey, string name)
        {
            StackBoardManager.runningStacks.AddLast(this);
            this.userAPIKey = userAPIKey;
            this.name = name;
            this.createdAt = DateTime.Now;
            nodes = new List<Node>();
        }

        public static Stack Create(string userAPIKey, string name)
        {
            return new Stack(userAPIKey, name);
        }

        public static Stack Find(string userAPIKey, string name)
        {
            return StackBoardManager.FindStack(userAPIKey, name);
        }

        public void Push(Node node)
        {
            nodes.Add(node);
        }
    }

}



