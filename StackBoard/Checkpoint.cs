using System;

namespace StackBoard
{
    [Serializable]
    public class Checkpoint : Node
    {
        public string serializedModel { get; private set; }
        public string url { get; private set; }
        private Checkpoint() { }
        /// <summary>
        /// Create a new checkpoint for the serialized model.
        /// </summary>
        /// <param name="serializedModel">The model parameters serialized.</param>
        public Checkpoint(string description, string serializedModel) : base("Checkpoint", description, typeof(Checkpoint))
        {
            this.serializedModel = serializedModel;
            url = "";
        }
    }
}



