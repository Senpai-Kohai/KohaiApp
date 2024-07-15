using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace client_app
{
    public class AIService
    {
        private readonly HttpClient _httpClient;
        private AppConfiguration _config;
        private string? currentThreadID;

        public AIService(HttpClient httpClient, AppConfiguration config)
        {
            _config = config;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string?> GetAICompletionResponseAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(_config.ChatGPTApiKey) || string.IsNullOrWhiteSpace(_config.ChatGPTApiUrl))
            {
                Debug.WriteLine($"Error communicating with ChatGPT Completion API endpoint: API key or url not configured.");

                return null;
            }

            var requestObject = new JObject(
                new JProperty("model", "gpt-4o"),
                new JProperty("messages", new JArray(
                    new JObject(
                        new JProperty("role", "user"),
                        new JProperty("content", prompt)
                    )
                ))
            );

            // To convert the JObject to a string if needed
            string requestString = requestObject.ToString();
            string requestUri = $"{_config.ChatGPTApiUrl}/chat/completions";

            Debug.WriteLine($"AI prompt Completion API request payload: {requestString}");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ChatGPTApiKey);
                requestMessage.Content = new StringContent(requestString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject responseObject = JObject.Parse(jsonResponse);

                    JArray? choices = responseObject["choices"] as JArray;
                    if (choices != null && choices.Count > 0)
                    {
                        string? text = choices[0]?["message"]?["content"]?.ToString();
                        return text;
                    }
                    else
                    {
                        return "No choices found in response.";
                    }
                }
                else
                {
                    Debug.WriteLine($"Error communicating with ChatGPT API endpoint at {requestUri}. Reason: {response.ReasonPhrase}");

                    return $"Error: {response.ReasonPhrase}";
                }
            }
        }

        public async Task<string?> GetAIAssistantResponseAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(_config.ChatGPTApiKey) || string.IsNullOrWhiteSpace(_config.ChatGPTApiUrl))
            {
                Debug.WriteLine($"Error communicating with ChatGPT Assistant API endpoint: API key or url not configured.");

                return null;
            }

            if (string.IsNullOrWhiteSpace(currentThreadID) && !await CreateNewThread())
            {
                Debug.WriteLine($"Could not create a new thread for the Assistant API to use for prompt: {prompt}");

                return null;
            }

            if (!await AddMessageToThread(prompt))
            {
                Debug.WriteLine($"Could not add a new message to thread for the Assistant API to use for prompt: {prompt}");

                return null;
            }

            string? response = await RunCurrentAssistantAIThread_AndWaitForResponse();
            if (string.IsNullOrWhiteSpace(response))
            {
                Debug.WriteLine($"Failed to get valid response from Assistant AI for prompt: {prompt}");

                return null;
            }

            return response;
        }

        private async Task<string?> RetrieveAssistantAIGuid()
        {
            if (string.IsNullOrWhiteSpace(_config.ChatGPTApiKey) || string.IsNullOrWhiteSpace(_config.ChatGPTApiUrl))
            {
                Debug.WriteLine($"Error communicating with ChatGPT Retrieve Assistant ID API endpoint: API key or url not configured.");

                return null;
            }

            string requestUri = $"{_config.ChatGPTApiUrl}/assistants";

            Debug.WriteLine($"AI prompt Retrieve Assistant ID API request (no payload)");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ChatGPTApiKey);
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");

                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject responseObject = JObject.Parse(jsonResponse);

                    string? assistantID = (string?)responseObject?["data"]?[0]?["id"];

                    return assistantID;
                }
                else
                {
                    Debug.WriteLine($"Error communicating with ChatGPT Retrieve Assistant ID API endpoint at {requestUri}. Reason: {response.ReasonPhrase}");
                }

                return null;
            }
        }

        private async Task<string?> RunCurrentAssistantAIThread()
        {
            if (string.IsNullOrWhiteSpace(currentThreadID))
            {
                Debug.WriteLine($"Current thread not set, and is unable to be run by the Assistant AI.");

                return null;
            }

            if (string.IsNullOrWhiteSpace(_config.ChatGPTApiKey) || string.IsNullOrWhiteSpace(_config.ChatGPTApiUrl))
            {
                Debug.WriteLine($"Error communicating with ChatGPT Run Thread Assistant API endpoint: API key or url not configured.");

                return null;
            }

            string? assistantID = _config.ChatGPTAssistantAI ?? await RetrieveAssistantAIGuid();
            if (string.IsNullOrWhiteSpace(assistantID))
            {
                Debug.WriteLine($"Could not determine Assistant AI identifier to use, either from ENV or retrieved via API request.");

                return null;
            }

            var requestObject = new JObject(
                new JProperty("assistant_id", assistantID)
            );

            string requestString = requestObject.ToString();
            string requestUri = $"{_config.ChatGPTApiUrl}/threads/{currentThreadID}/runs";

            Debug.WriteLine($"AI prompt Run Thread Assistant API request (no payload)");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ChatGPTApiKey);
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");
                requestMessage.Content = new StringContent(requestString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject responseObject = JObject.Parse(jsonResponse);

                    return (string?)responseObject?["id"];
                }
                else
                {
                    Debug.WriteLine($"Error communicating with ChatGPT Run Thread Assistant API endpoint at {requestUri}. Reason: {response.ReasonPhrase}");
                }

                return null;
            }
        }

        private async Task<string?> RunCurrentAssistantAIThread_AndWaitForResponse()
        {
            if (string.IsNullOrWhiteSpace(currentThreadID))
            {
                Debug.WriteLine($"Current thread not set, and required in order to wait for run response.");

                return null;
            }

            string? runID = await RunCurrentAssistantAIThread();
            if (string.IsNullOrWhiteSpace(runID))
            {
                Debug.WriteLine($"Failed to retrieve run ID from Run Thread Assistant API request.");

                return null;
            }

            TimeSpan attemptInterval = TimeSpan.FromMilliseconds(_config.ChatGPTRetryInterval);
            bool success = false;

            for (int i = 0; i < _config.ChatGPTRetryMaxAttempts; i++)
            {
                if (await PollForAssistantAIRunCompletionState(runID) == "completed")
                {
                    success = true;
                    break;
                }

                await Task.Delay(attemptInterval);
            }

            if (success)
                return await GetLastMessageFromCurrentThread();

            return null;
        }

        private async Task<string?> GetLastMessageFromCurrentThread()
        {
            if (string.IsNullOrWhiteSpace(currentThreadID))
            {
                Debug.WriteLine($"Failed to get last message from current thread- current thread not set.");

                return null;
            }

            string requestUri = $"{_config.ChatGPTApiUrl}/threads/{currentThreadID}/messages";

            Debug.WriteLine($"AI prompt Get Last Message Assistant API request (no payload)");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ChatGPTApiKey);
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");

                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        JObject responseObject = JObject.Parse(jsonResponse);

                        JArray? messageArray = (JArray?)responseObject["data"];
                        JObject? lastMessage = (JObject?)messageArray?.FirstOrDefault();
                        JObject? lastMessageContent = (JObject?)lastMessage?["content"]?.FirstOrDefault();
                        string? lastMessageStr = (string?)lastMessageContent?["text"]?["value"];

                        return lastMessageStr;
                    }
                    catch (Exception exc)
                    {
                        Debug.WriteLine($"An exception occurred parsing the last message in the current Assistant AI thread: {exc}");
                    }

                    return null;
                }
                else
                {
                    Debug.WriteLine($"Error communicating with ChatGPT Get Last Message Assistant API endpoint at {requestUri}. Reason: {response.ReasonPhrase}");
                }

                return null;
            }
        }

        private async Task<string?> PollForAssistantAIRunCompletionState(string runID)
        {
            if (string.IsNullOrWhiteSpace(currentThreadID))
            {
                Debug.WriteLine($"Error communicating with ChatGPT Get Run State Assistant API endpoint: current thread ID not set.");

                return null;
            }

            if (string.IsNullOrWhiteSpace(_config.ChatGPTApiKey) || string.IsNullOrWhiteSpace(_config.ChatGPTApiUrl))
            {
                Debug.WriteLine($"Error communicating with ChatGPT Get Run State Assistant API endpoint: API key or url not configured.");

                return null;
            }

            string requestUri = $"{_config.ChatGPTApiUrl}/threads/{currentThreadID}/runs/{runID}";

            Debug.WriteLine($"Polling for Assistant AI thread run completion...");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ChatGPTApiKey);
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");

                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject responseObject = JObject.Parse(jsonResponse);

                    return (string?)responseObject?["status"];
                }
                else
                {
                    Debug.WriteLine($"Error communicating with ChatGPT Run Thread Assistant API endpoint at {requestUri}. Reason: {response.ReasonPhrase}");
                }

                return null;
            }
        }

        private async Task<bool> AddMessageToThread(string message)
        {
            if (string.IsNullOrWhiteSpace(_config.ChatGPTApiKey) || string.IsNullOrWhiteSpace(_config.ChatGPTApiUrl))
            {
                Debug.WriteLine($"Error communicating with ChatGPT Add Message Assistant API endpoint: API key or url not configured.");

                return false;
            }

            var requestObject = new JObject(
                new JProperty("role", "user"),
                new JProperty("content", message)
            );

            // To convert the JObject to a string if needed
            string requestString = requestObject.ToString();
            string requestUri = $"{_config.ChatGPTApiUrl}/threads/{currentThreadID}/messages";

            Debug.WriteLine($"AI prompt Add Message Assistant API request payload: {requestString}");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ChatGPTApiKey);
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");
                requestMessage.Content = new StringContent(requestString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Error communicating with ChatGPT Add Message Assistant API endpoint at {requestUri}. Reason: {response.ReasonPhrase}");
                }

                return false;
            }
        }

        private async Task<bool> CreateNewThread()
        {
            if (string.IsNullOrWhiteSpace(_config.ChatGPTApiKey) || string.IsNullOrWhiteSpace(_config.ChatGPTApiUrl))
            {
                Debug.WriteLine($"Error communicating with ChatGPT Create Thread Assistant API endpoint: API key or url not configured.");

                return false;
            }

            string requestUri = $"{_config.ChatGPTApiUrl}/threads";

            Debug.WriteLine($"AI prompt Create Thread Assistant API request (no payload)");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ChatGPTApiKey);
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");
                //requestMessage.Headers.Add("Content-Type", "application/json");
                requestMessage.Content = new StringContent("{}", Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        JObject responseObject = JObject.Parse(jsonResponse);

                        currentThreadID = (string?)responseObject["id"];

                        return currentThreadID != null;
                    }
                    catch (Exception exc)
                    {
                        Debug.WriteLine($"An exception occurred parsing the Create Thread API response: {exc}");
                    }

                    return false;
                }
                else
                {
                    Debug.WriteLine($"Error communicating with ChatGPT Create Thread Assistant API endpoint at {_config.ChatGPTApiUrl}. Reason: {response.ReasonPhrase}");
                }

                return false;
            }
        }
    }
}
