using UnityEngine;
using StackBoard;
using System;
using System.Diagnostics;
using UnityEditor;
using System.IO;
using System.Text;

namespace DeepUnity
{
    [DisallowMultipleComponent]
    public class StackboardStatistics : MonoBehaviour
    {     
        public string userKey = "API Key";
        public UpdateFrequency refreshFrequency = UpdateFrequency.High;
        [ReadOnly] public bool runBackend = false;

        private StackboardStatistics Instance;
        private Process backendProcess;
        private Stack stack;

        private string startedAt = " - ";
        private string finishedAt = " - ";
        private string trainingSessionTime = " - ";
        private string inferenceTime = " - ";
        private string policyUpdateTime = " - ";
        private string policyUpdateTimePerIteration = " - ";
        private int episodeCount = 0;
        private int stepCount = 0;
        private int iterations = 0;
        private int parallelAgents = 0;

        private float policyUpdateSecondsElapsed = 0f;
        private float inferenceSecondsElapsed = 0f; // Updated via deltaTime
        private bool collectAgentStuff = false;

        private Graph cumulativeReward = new Graph("Cumulative Reward", "This graph displays the cumulative reward", "episode", "cumulated reward");
        private Graph episodeLength = new Graph("Episode Length", "This graph displays the steps taken for each episode", "episode", "steps");
        private Graph actorLoss = new Graph("Actor Loss", "Mean loss of policy function on each iteration", "iteration", "loss");
        private Graph criticLoss = new Graph("Critic Loss", "Mean MSE of Value (for PPO) or Q (for SAC) function on each epoch. Also used for the discriminator loss in Heuristic Training", "iteration", "loss");
        private Graph entropy = new Graph("Entropy", "The mean standard deviation of the policy for continuous actions, or -probs * log probs for discrete actions", "iteration", "entropy");
        private Graph learningRate = new Graph("Learning Rate", "Learning rate decay on each iteration", "iteration", "learning rate");


        private async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
                EditorApplication.playModeStateChanged += CloseBackendAndMakeCheckpoints;

                StackBoardManager.Run(refreshFrequency);
                stack = await Stack.CreateAsync(userKey, GetComponent<Agent>().GetType().Name);
                stack.Push(GenerateTrainingConfigurationTable());
                stack.Push(GenerateTrainerSpecificConfigurationTable());
                stack.Push(cumulativeReward);
                stack.Push(episodeLength);
                stack.Push(actorLoss);
                stack.Push(criticLoss);
                stack.Push(entropy);
                stack.Push(learningRate);

                if(runBackend)
                {
                    string exeFileName = "stackboard.exe";
                    string[] exePaths = Directory.GetFiles(Application.dataPath, exeFileName, SearchOption.AllDirectories);

                    if (exePaths.Length > 0)
                    {
                        string exePath = exePaths[0].Replace(Application.dataPath, "Assets");
                        backendProcess = new Process();
                        backendProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        backendProcess.StartInfo.CreateNoWindow = true;
                        backendProcess.StartInfo.UseShellExecute = false;
                        backendProcess.StartInfo.FileName = exePath;
                        backendProcess.StartInfo.Arguments = "/c";
                        backendProcess.EnableRaisingEvents = true;
                        backendProcess.Start();
                        ConsoleMessage.Info("<b>StackBoard backend process is running in background!</b>");
                    }
                    else
                    {
                        ConsoleMessage.Error("StackBoard backend executable couldn't be found in the assets of the project!");
                        EditorApplication.isPlaying = false;
                    }
                }             
            }
        }
        private void FixedUpdate()
        {
            if (!collectAgentStuff)
            {
                try
                {
                    var ags = DeepUnityTrainer.Instance.parallelAgents;
                    foreach (var item in ags)
                    {
                        item.OnEpisodeEnd += UpdateAgentStuff;
                    }
                    collectAgentStuff = true;
                }
                catch { }
            }


            stepCount = DeepUnityTrainer.Instance.currentSteps;
            parallelAgents = DeepUnityTrainer.Instance.parallelAgents.Count;
            inferenceSecondsElapsed += Time.fixedDeltaTime;
            inferenceTime = $"{(int)(Math.Ceiling(inferenceSecondsElapsed * DeepUnityTrainer.Instance.parallelAgents.Count) / 3600)} hrs : {(int)(Math.Ceiling(inferenceSecondsElapsed * DeepUnityTrainer.Instance.parallelAgents.Count) % 3600 / 60)} min : {(int)(Math.Ceiling(inferenceSecondsElapsed * DeepUnityTrainer.Instance.parallelAgents.Count) % 60)} sec";


            if (DeepUnityTrainer.Instance.updateIterations > iterations)
                UpdateTrainerStuff();
        }

        private Message GenerateTrainingConfigurationTable()
        {
            StringBuilder text = new StringBuilder();

            Hyperparameters hp = GetComponent<Agent>().model.config;

            text.Append($"[Trainer - {hp.trainer}] ");
            text.Append($"[Max steps - {hp.maxSteps}] ");
            text.Append($"[Learning Rate - {hp.learningRate}] ");
            text.Append($"[Gamma - {hp.gamma}]");
            text.Append($"[Batch Size - {hp.batchSize}] ");
            text.Append($"[Buffer Size - {hp.bufferSize}] ");
            text.Append($"[Learning Rate Schedule - {hp.LRSchedule}] ");
            text.Append($"[Timescale - {hp.timescale}]");

            Message config = new Message("Training Configuration", text.ToString());
            return config;
        }
        private Message GenerateTrainerSpecificConfigurationTable()
        {
           

            Hyperparameters hp = GetComponent<Agent>().model.config;

            if (hp.trainer == TrainerType.PPO)
            {
                StringBuilder text = new StringBuilder();

                text.Append($"[Horizon - {hp.horizon}] ");
                text.Append($"[Num Epoch - {hp.numEpoch}] ");
                text.Append($"[Beta - {hp.beta}]");
                text.Append($"[Epsilon - {hp.epsilon}] ");
                text.Append($"[Lambda - {hp.lambda}] ");
                text.Append($"[Gradient Clipping by Norm - {hp.gradClipNorm}] ");
                text.Append($"[KL Divergence - {hp.KLDivergence}] ");

                if (hp.KLDivergence != KLType.Off)
                    text.Append($"[Target KL - {hp.targetKL}] ");

                text.Append($"[Advantage Normalization - {hp.normalizeAdvantages}]");

                Message mess = new Message("Proximal Policy Optimization Specific Configuration", text.ToString());
                return mess;
            }
            else if (hp.trainer == TrainerType.SAC)
            {
                StringBuilder text = new StringBuilder();

                text.Append($"[Update Every - {hp.updateEvery}] ");
                text.Append($"[Update After - {hp.updateAfter}] ");
                text.Append($"[Updates Num - {hp.updatesNum}] ");
                text.Append($"[Alpha - {hp.alpha}] ");
                text.Append($"[Tau - {hp.tau}]");

                Message mess = new Message("Soft Actor-Critic Specific Configuration", text.ToString());
                return mess;
            }
            else
                throw new Exception("Unhandled Trainer type");
           

        }
        
        private void UpdateAgentStuff(object sender, EventArgs e)
        {
            Agent ag = (Agent)sender;
            episodeCount++;
            episodeLength.Append(ag.EpisodeStepCount);
            cumulativeReward.Append(ag.EpsiodeCumulativeReward);
        }
        private void UpdateTrainerStuff()
        {
            iterations++;

            TimeSpan timeElapsed = DateTime.Now - DeepUnityTrainer.Instance.timeWhenTheTrainingStarted;
            trainingSessionTime = $"{(int)timeElapsed.TotalHours} hrs : {(int)timeElapsed.TotalMinutes % 60} min : {(int)timeElapsed.TotalSeconds % 60} sec";
            policyUpdateSecondsElapsed += (float)DeepUnityTrainer.Instance.updateClock.Elapsed.TotalSeconds;
            policyUpdateTime = $"{(int)(Math.Ceiling(policyUpdateSecondsElapsed) / 3600)} hrs : {(int)(Math.Ceiling(policyUpdateSecondsElapsed) % 3600 / 60)} min : {(int)(Math.Ceiling(policyUpdateSecondsElapsed) % 60)} sec";
            policyUpdateTimePerIteration = $"{(int)DeepUnityTrainer.Instance.updateClock.Elapsed.TotalHours} hrs : {(int)(DeepUnityTrainer.Instance.updateClock.Elapsed.TotalMinutes) % 60} min : {(int)(DeepUnityTrainer.Instance.updateClock.Elapsed.TotalSeconds) % 60}.{DeepUnityTrainer.Instance.updateClock.ElapsedMilliseconds % 1000} sec";

            actorLoss.Append(DeepUnityTrainer.Instance.actorLoss);
            criticLoss.Append(DeepUnityTrainer.Instance.criticLoss);
            entropy.Append(DeepUnityTrainer.Instance.entropy);
            learningRate.Append(DeepUnityTrainer.Instance.learningRate);
        }
        private void CloseBackendAndMakeCheckpoints(PlayModeStateChange state)
        {
            if(state == PlayModeStateChange.ExitingPlayMode)
            {
                var model = GetComponent<Agent>().model;
                if (model.muNetwork)
                    stack.Push(new Checkpoint("mu network", JsonUtility.ToJson(model.muNetwork)));
                if (model.sigmaNetwork)
                    stack.Push(new Checkpoint("sigma network", JsonUtility.ToJson(model.sigmaNetwork)));
                if (model.discreteNetwork)
                    stack.Push(new Checkpoint("discrete network", JsonUtility.ToJson(model.discreteNetwork)));
                if (model.vNetwork)
                    stack.Push(new Checkpoint("value network", JsonUtility.ToJson(model.vNetwork)));
                if (model.q1Network)
                    stack.Push(new Checkpoint("q1 network", JsonUtility.ToJson(model.q1Network)));
                if (model.q2Network)
                    stack.Push(new Checkpoint("q2 network", JsonUtility.ToJson(model.q2Network)));

                StackBoardManager.Stop();
                backendProcess?.Kill();
            }        
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(StackboardStatistics)), CanEditMultipleObjects]
    class CustomAgentStacboardStatisticsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            string[] dontDrawMe = new string[] { "m_Script" };

            StackboardStatistics script = target as     StackboardStatistics;
            if (script.userKey == "API Key")
                EditorGUILayout.HelpBox("Please introduce your API Key (check StackBoard settings)", MessageType.Warning);

            DrawPropertiesExcluding(serializedObject, dontDrawMe);

            if (script.runBackend == false)
            EditorGUILayout.HelpBox("Make sure StackBoard's backend is running.", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

}



