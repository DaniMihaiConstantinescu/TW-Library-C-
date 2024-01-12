using System;
using System.Collections.Generic;

namespace StackBoard
{
    [Serializable]
    public class Graph : Node
    {
        public int count { get; private set; }
        public string xLabel { get; private set; }
        public string yLabel { get; private set; }
        public LinkedList<KeyFrame> keyFrames { get; private set; }


        private Graph() { }
        public Graph(string name, string description, string xLabel, string yLabel) : base(name, description, typeof(Graph))
        {
            count = 0;
            this.xLabel = xLabel;
            this.yLabel = yLabel;
            keyFrames = new LinkedList<KeyFrame>();
        }

        /// <summary>
        /// Append a new value to the next step.
        /// </summary>
        /// <param name="value"></param>
        public void Append(float value)
        {
            keyFrames.AddLast(new KeyFrame(count++, value));
        }
        public class KeyFrame
        {
            public float x { get; set; }
            public float y { get; set;}

            public KeyFrame() { }
            public KeyFrame(float time, float value)
            {
                this.x = time;
                this.y = value;
            }
        }
    }


    
}



