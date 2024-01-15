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
//      For .NET, go to Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution ->  add from Git link, enter: com.unity.nuget.newtonsoft-json and install

namespace StackBoard
{
    /// <summary>
    /// How StackBoard should be used: <br></br>
    ///     1. Call StackBoardManager.Run(refreshRate) to start the data transfer between this and Stackboard's backend. <br></br>
    ///     2. Create your nodes (Graph, Table, Message etc.) at your wish and add data into them whenever you want.  <br></br>
    ///     3. Usually at the beginning, create a Stack asynchronously by awaiting Stack.CreateAsync() (and when you are convinced the await was done) then push all nodes created into the Stack. <br></br>
    ///     4. Call StackBoardManager.Stop() to close the data transfer between this and Stackboard's backend. <br></br>
    /// </summary>
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
        private static Timer updateClock;

        static StackBoardManager()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += (UnityEditor.PlayModeStateChange state) =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                    Stop();
            };
            #endif
        }

        /// <summary>
        /// Start running data to the server. It is automatically started when application runs.
        /// </summary>
        /// <param name="updateFrequency">How often new data should be delivered to the server</param>
        public static void Run(UpdateFrequency refreshFrequency = UpdateFrequency.High)
        {
            updateClock = new Timer(Update, null, 0, ConvertFrequencyToMiliseconds(refreshFrequency));
#if UNITY_EDITOR
            UnityEngine.Debug.Log("<b><color=green>StackBoard is running</color></b>");
#endif
            Console.WriteLine("<b><color=green>StackBoard is running</color></b>");
            
        }
        /// <summary>
        /// Stop sending data to the server.
        /// </summary>
        public static void Stop()
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log("<b><color=red>StackBoard was stopped</color></b>");
#endif
            Console.WriteLine("<b><color=red>StackBoard was stopped</color></b>");
            Update(null);
            updateClock.Dispose();
        }

        /// <summary>
        ///  This method gets an existing stack. from db.
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
        public static async Task<int> GetNewStackId()
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync($"{serverUrl}/stack/getid");

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(jsonResponse);

                    int newId = result.id;
                    return newId;
                }
                else
                {
                    // Handle the error if needed
                    throw new Exception($"Failed to get new stack id. Status code: {response.StatusCode}");
                }
            }
        }
        /// <summary>
        /// Update the server's data. Update() method doesn't require to be awaited!
        /// </summary>
        /// <returns></returns>
        private static async void Update(object state)
        {
            // #if UNITY_EDITOR
            // if (UnityEditor.EditorApplication.isPlaying == false)
            //     Stop();
            // #endif

            foreach (var stack in runningStacks)
            {
                string serializedStack = JsonConvert.SerializeObject(stack);

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
                    var requestUri = new Uri(serverUrl + "/stack/add");
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(requestUri, content);

                    if (response.IsSuccessStatusCode)
                    {
                        // #if UNITY_EDITOR
                        // UnityEngine.Debug.Log("Data sent to the server successfully.");
                        // #endif
                        // Console.WriteLine("Data sent to the server successfully.");
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

        private static int ConvertFrequencyToMiliseconds(UpdateFrequency frequency)
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

