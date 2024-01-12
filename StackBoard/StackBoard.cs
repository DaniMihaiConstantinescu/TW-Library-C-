using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Text;
using System;
using System.Threading.Tasks;
using System.Threading;

//INSTALATION NOTES:
// 1. StackBoard requires Newtonsoft.Json package to be installed.
//      For Unity, go to Window -> Package Manager -> Packages (Unity Registry) -> Install Newtonsoft Json
//      For .NET, go to Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution -> Browse Newtonsoft.Json and install

namespace StackBoard
{
    internal static class StackBoardManager
    {
        // Stackboard constant settings
        private readonly static string serverUrl = "http://127.0.0.1:5000";

        public readonly static LinkedList<Stack> runningStacks = new LinkedList<Stack>();
        public readonly static List<string> colorPallete = new()
        {
            "#A4B0BE",
            "#9E7F47",
            "#DE9E2C",
            "#495789",
            "#5ES442"
        };
        public static UpdateFrequency updateFrequency = UpdateFrequency.High;
        private readonly static Timer updateClock;

  

        static StackBoardManager()
        {
            updateClock = new Timer(Update, null, 0, UpdateFrequencyToMiliseconds(updateFrequency));

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += (UnityEditor.PlayModeStateChange state) =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                    updateClock.Dispose();
            };
            #endif
        }

        /// <summary>
        ///  This method gets an existing stack.
        /// </summary>
        /// <param name="userAPIKey"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Stack FindStack(string userAPIKey, int stackId)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverUrl + $"/stack/{userAPIKey}/{stackId}");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            Stack foundStack = (Stack)JsonConvert.DeserializeObject(reader.ReadToEnd());
            return foundStack;
        }

        /// <summary>
        /// Update the server's data. Update() method doesn't require to be awaited!
        /// </summary>
        /// <returns></returns>
        private static async void Update(object state)
        {        
            foreach (var stack in runningStacks)
            {
                string serializedStack = JsonConvert.SerializeObject(stack);

                UnityEngine.Debug.Log(serializedStack);

                // Now send it to the server
                await SendToServer(serializedStack);
            }
        }



        private static async Task SendToServer(string jsonPayload)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Define the server URL (replace with your actual server endpoint).
                    var requestUri = new Uri(serverUrl + "/stack/add");

                    // Create a JSON content with the serialized data.
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Send the HTTP POST request.
                    var response = await client.PostAsync(requestUri, content);

                    // Check the response status code.
                    if (response.IsSuccessStatusCode)
                    {
                        #if UNITY_EDITOR
                        UnityEngine.Debug.Log("Data sent to the server successfully.");
                        #endif
                        Console.WriteLine("Data sent to the server successfully.");
                    }
                    else
                    {
                        #if UNITY_EDITOR
                        UnityEngine.Debug.Log($"StackBoard: Failed to send data to the server. Status code: {response.StatusCode}");
                        #endif
                        Console.WriteLine($"StackBoard: Failed to send data to the server. Status code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                #if UNITY_EDITOR
                UnityEngine.Debug.Log($"Error while sending data to the server: {ex.Message}");
                #endif
                Console.WriteLine($"Error while sending data to the server: {ex.Message}");
            }
        }    
        private static int UpdateFrequencyToMiliseconds(UpdateFrequency frequency)
        {
            switch(frequency)
            {
                case UpdateFrequency.Low: return 60_000;
                case UpdateFrequency.Moderate: return 15_000;
                case UpdateFrequency.High: return 5_000;
                default: throw new ArgumentException("Unhandled update frequency type.");
            }
        }
    }

    public enum UpdateFrequency
    {
        /// <summary>
        /// Once every 1 minute (60.000ms)
        /// </summary>
        Low,
        /// <summary>
        /// Once every 15 seconds (15.0000ms)
        /// </summary>
        Moderate,
        /// <summary>
        /// Once every 5 seconds (5.000ms)
        /// </summary>
        High,
    }

}

