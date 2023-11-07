using System;

namespace StackBoard
{
    [Serializable]
    public class Checkpoint : Node
    {
        public string serializedModel { get; private set; }
        public float performanceIndex { get; private set; }

        public Checkpoint() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializedModel">The model parameters serialized.</param>
        /// <param name="performanceIndex">The loss or the acurracy of the model.</param>
        public Checkpoint(string serializedModel, float performanceIndex) : base("Checkpoint", $"Performance index: {performanceIndex.ToString()}")
        {
            this.serializedModel = serializedModel;
            this.performanceIndex = performanceIndex;
        }
    }
}



