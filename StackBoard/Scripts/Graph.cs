using System;
using System.Collections.Generic;

namespace StackBoard
{
    [Serializable]
    public class Graph : Node
    {
        public string xLabel { get; private set; }
        public string yLabel { get; private set; }
        public LinkedList<KeyFrame> keyFrames { get; private set; }

        public Graph() { }
        public Graph(string name, string description, string xLabel, string yLabel) : base(name, description)
        {
            this.xLabel = xLabel;
            this.yLabel = yLabel;
            keyFrames = new LinkedList<KeyFrame>();
        }

        public void Append(float x, float y)
        {
            keyFrames.AddLast(new KeyFrame(x, y));
        }
        public void Append(KeyFrame k)
        {
            keyFrames.AddLast(k);
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



