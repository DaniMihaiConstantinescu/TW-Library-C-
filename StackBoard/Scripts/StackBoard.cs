using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Text;
using System;
using System.Threading.Tasks;

namespace StackBoard
{
    public static class StackBoardManager
    {
        private readonly static string serverUrl = "localhost:8008";
        public readonly static LinkedList<Stack> runningStacks =  new LinkedList<Stack>();

        /// <summary>
        ///  This method gets an existing stack.
        /// </summary>
        /// <param name="userAPIKey"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Stack FindStack(string userAPIKey, string name)
        {
            string httpAddr = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(httpAddr);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            Stack foundStack = (Stack)JsonConvert.DeserializeObject(reader.ReadToEnd());
            return foundStack;
        }

        /// <summary>
        /// This method sends to the backend the overwritten stacks
        /// </summary>
        public static void Update()
        {
            foreach (var stack in runningStacks)
            {
                string serializedStack = JsonConvert.SerializeObject(stack);

                // UnityEngine.Debug.Log(serializedStack);

                // Now send it to server
                SendToServer(serverUrl, serializedStack);
            }
        }


        private static async Task SendToServer(string svURL, string jsonPayload)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Define the server URL (replace with your actual server endpoint).
                    var requestUri = new Uri(svURL);

                    // Create a JSON content with the serialized data.
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Send the HTTP POST request.
                    var response = await client.PostAsync(requestUri, content);

                    // Check the response status code.
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Data sent to the server successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to send data to the server. Status code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while sending data to the server: {ex.Message}");
            }
        }
    }

 

}

