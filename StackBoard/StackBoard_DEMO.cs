using DeepUnity;
using UnityEngine;
using StackBoard;

namespace DeepUnityTutorials
{
    public class StackBoard_DEMO : MonoBehaviour
    {
        NeuralNetwork net;
        Optimizer optim;
        Stack stack;
        Graph graph = new Graph("Map noise to 1", "stackboard demo", "epoch", "loss");

        public string apikey = "VGtdLS3LSxdTAn2eib20LJ4AnHL2";
        public float learning_rate = 1e-3f;
        public int batch_size = 64;
        public int epochs = 100;
        [ReadOnly] public int current_epoch = 0;


        private async void Awake()
        {
            StackBoardManager.Run();
            stack = await Stack.CreateAsync(apikey, "StackBoard DEMO");
            stack.Push(graph);
            stack.Push(new Message("Details", $"lr: {optim.gamma}, batch_size: {batch_size}"));
        }

        private void Start()
        {
            net = new NeuralNetwork(
               new Dense(1, 32, device: Device.GPU),
               new ReLU(),
               new Dense(32, 32, device: Device.GPU),
               new ReLU(),
               new Dense(32, 1, device: Device.GPU)
               );
            optim = new Adam(net.Parameters(), learning_rate);
        }

        private void Update()
        {
            if(current_epoch < epochs)
            {
                Tensor input = Tensor.RandomNormal(batch_size, 1) * 0.1f;
                Tensor prediction = net.Forward(input);
                Loss mse = Loss.MSE(prediction, Tensor.Fill(1, batch_size, 1));
                net.Backward(mse.Gradient);
                optim.Step();
                graph.Append(mse.Item); 
                Debug.Log(mse.Item);
            }
            else if(current_epoch == epochs)
            {
                Debug.Log("Training session ended");
                string serializedModel = JsonUtility.ToJson(net);
                stack.Push(new Checkpoint($"The serialized model after {epochs} epochs", serializedModel));
                StackBoardManager.Stop();
            }
            current_epoch++;
        }
        
    }
}



