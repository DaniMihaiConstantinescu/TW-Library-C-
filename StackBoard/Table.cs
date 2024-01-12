using System.Collections.Generic;
namespace StackBoard
{
    public class Table : Node
    {
        public string[] collumns {  get; private set; }   
        public LinkedList<object[]> rows { get; private set; }

        private Table() { }

        public Table(string name, string description, params string[] labels) : base(name, description, typeof(Table))
        {
            if(labels == null || labels.Length == 0)
            {
                throw new System.ArgumentException("Labels argument is null or has no elements in it when creating a StackBoard Table");
            }
            this.collumns = labels;
            rows = new LinkedList<object[]>();
        }

        public void Append(params object[] rowElements)
        {
            if (collumns.Length != collumns.Length)
                throw new System.ArgumentException($"This StackBoard Table was created with {collumns.Length} collumns and received {rowElements.Length}.");

            rows.AddLast(rowElements);    
        }
    }


}



