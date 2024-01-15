using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StackBoard
{
    [Serializable]
    public class Stack
    {
        public string userAPIKey { get; private set; }
        public int id { get; private set; }
        public string name { get; private set; }  
        public string color { get; private set; }
        public DateTime createdAt { get; private set; }
        public List<Node> nodes { get; private set; }

        // Parameterless constructor required for serialization.
        private Stack() { }

        private Stack(string userAPIKey, int id, string name)
        {
            StackBoardManager.runningStacks.AddLast(this);
            this.userAPIKey = userAPIKey;
            this.id = id;
            this.name = name;
            this.color = StackBoardManager.colorPallete[new Random().Next(StackBoardManager.colorPallete.Count)].ToString();
            this.createdAt = DateTime.Now;
            nodes = new List<Node>();
            
        }

        public static async Task<Stack> CreateAsync(string userAPIKey, string name)
        {
            int newId = await StackBoardManager.GetNewStackId();

            return new Stack(userAPIKey, newId, name);
        }

        public static Stack Find(string userAPIKey, int stackId)
        {
            return StackBoardManager.FindStack(userAPIKey, stackId);
        }

        public void Push(Node node)
        {
            nodes.Add(node);
        }
    }

}



