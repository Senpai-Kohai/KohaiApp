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
    /// <summary>
    /// Responsible for wrapping the OpenAI SDK / client.
    /// </summary>
    public class AIService : ServiceBase<AIServiceConfiguration>
    {
        private OpenAI.Chat.ChatClient? _chatClient;
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        private OpenAI.Assistants.AssistantClient? _assistantClient;
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        private OpenAI.Files.FileClient? _fileClient;

        private readonly OpenAI.OpenAIClient? _openAIClient;
        private readonly HttpClient? _httpClient;
        private string? currentThreadID;

        public bool ServiceEnabled => _openAIClient != null && _httpClient != null;

        public AIService(HttpClient httpClient)
        {
            if (ServiceConfiguration.ChatGPTApiKey == null)
            {
                Debug.WriteLine($"Error starting AI service: API key not configured.");

                return;
            }

            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _openAIClient = new OpenAI.OpenAIClient(ServiceConfiguration.ChatGPTApiKey);
        }

        public async Task<string?> New_GetAICompletionResponseAsync(string prompt)
        {
            if (!ServiceEnabled)
            {
                Debug.WriteLine($"Error communicating with ChatGPT Completion API endpoint: AI service is not running.");

                return null;
            }

            if (_chatClient == null)
                _chatClient = _openAIClient?.GetChatClient("gpt-4o") ?? throw new NullReferenceException("Chat client is null");

            try
            {
                var completionOptions = new OpenAI.Chat.ChatCompletionOptions()
                {
                    User = "user"
                };
                var result = await _chatClient.CompleteChatAsync(new[] { new OpenAI.Chat.AssistantChatMessage(prompt) }, completionOptions, Program.ShutdownTokenSource.Token);

                return result.Value.Content.ToString();
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"Error communicating with ChatGPT completion API endpoint. Exception: {exc}");

                return $"Error: {exc}";
            }
        }

        public async Task<string?> GetAICompletionResponseAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiKey) || string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiUrl))
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
            var requestString = requestObject.ToString();
            var requestUri = $"{ServiceConfiguration.ChatGPTApiUrl}/chat/completions";

            Debug.WriteLine($"AI prompt Completion API request payload: {requestString}");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceConfiguration.ChatGPTApiKey);
                requestMessage.Content = new StringContent(requestString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject responseObject = JObject.Parse(jsonResponse);

                    JArray? choices = responseObject["choices"] as JArray;
                    if (choices != null && choices.Count > 0)
                    {
                        var text = choices[0]?["message"]?["content"]?.ToString();
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
            if (string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiKey) || string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiUrl))
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

            try
            {
                if (response.StartsWith('{'))
                {
                    JObject responseObj = JObject.Parse(response);

                    string? functionName = responseObj["function"]?.ToString();
                    if (string.Equals("create_character_profile", functionName, StringComparison.OrdinalIgnoreCase))
                    {
                        string? characterNickname = (string?)responseObj["nickname"];
                        int? characterAge = (int?)responseObj["age"];
                        response = $"A new character profile was created!\n  Name: {characterNickname}\n  Age: {characterAge}";
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred parsing an Assistant AI response as a function payload: {exc}");
            }

            return response;
        }

        private async Task<string?> RetrieveAssistantAIGuid()
        {
            if (string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiKey) || string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiUrl))
            {
                Debug.WriteLine($"Error communicating with ChatGPT Retrieve Assistant ID API endpoint: API key or url not configured.");

                return null;
            }

            string requestUri = $"{ServiceConfiguration.ChatGPTApiUrl}/assistants";

            Debug.WriteLine($"AI prompt Retrieve Assistant ID API request (no payload)");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceConfiguration.ChatGPTApiKey);
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

            if (string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiKey) || string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiUrl))
            {
                Debug.WriteLine($"Error communicating with ChatGPT Run Thread Assistant API endpoint: API key or url not configured.");

                return null;
            }

            string? assistantID = ServiceConfiguration.ChatGPTAssistantAI ?? await RetrieveAssistantAIGuid();
            if (string.IsNullOrWhiteSpace(assistantID))
            {
                Debug.WriteLine($"Could not determine Assistant AI identifier to use, either from ENV or retrieved via API request.");

                return null;
            }

            var requestObject = new JObject(
                new JProperty("assistant_id", assistantID)
            );

            string requestString = requestObject.ToString();
            string requestUri = $"{ServiceConfiguration.ChatGPTApiUrl}/threads/{currentThreadID}/runs";

            Debug.WriteLine($"AI prompt Run Thread Assistant API request (no payload)");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceConfiguration.ChatGPTApiKey);
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

            TimeSpan attemptInterval = TimeSpan.FromMilliseconds(ServiceConfiguration.ChatGPTRetryInterval);
            bool success = false;

            for (int i = 0; i < ServiceConfiguration.ChatGPTRetryMaxAttempts; i++)
            {
                JObject? responseObj = await GetAssistantAIRunState(runID);
                string? runState = (string?)responseObj?["status"];

                Debug.WriteLine($"Current run state: [{runState}]");

                if (string.Equals("requires_action", runState, StringComparison.OrdinalIgnoreCase))
                {
                    var runStatePrintedLines = responseObj?.ToString(Formatting.Indented)?.Split('\n');
                    if (runStatePrintedLines == null)
                        continue;

                    foreach (var runStatePrintedLine in runStatePrintedLines)
                        Debug.WriteLine($"{runStatePrintedLine}");

                    string? requiredActionsStr = (await GetFunctionCallsFromRequiredActions(runID, responseObj))?.ToString(Formatting.None);
                    if (!string.IsNullOrWhiteSpace(requiredActionsStr))
                        return requiredActionsStr;

                    break;
                }

                if (string.Equals("completed", runState, StringComparison.OrdinalIgnoreCase))
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

        private async Task<JArray?> GetFunctionCallsFromRequiredActions(string runID, JObject responseObject)
        {
            if (string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiKey) || string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiUrl))
            {
                Debug.WriteLine($"Error communicating with ChatGPT Run Thread Assistant API endpoint: API key or url not configured.");

                return null;
            }

            JArray confirmToolOutputs = new JArray();
            JArray functionCalls = new JArray();
            JObject? requiredAction = (JObject?)responseObject["required_action"];

            if (requiredAction == null)
                return null;

            JObject? submitToolOutputs = (JObject?)requiredAction["submit_tool_outputs"];
            if (submitToolOutputs == null)
                return null;

            JArray? toolCalls = (JArray?)submitToolOutputs["tool_calls"];
            if (toolCalls == null)
                return null;

            foreach (var toolCallObj in toolCalls)
            {
                string? toolCallID = (string?)toolCallObj["id"];
                string? toolStringValue = (string?)toolCallObj["function"]?["arguments"];
                if (string.IsNullOrEmpty(toolStringValue))
                    continue;

                JObject? newFunctionCall = JObject.Parse(toolStringValue);
                if (newFunctionCall != null)
                {
                    confirmToolOutputs.Add(new JObject() { ["tool_call_id"] = toolCallID, ["output"] = toolStringValue });
                    functionCalls.Add(newFunctionCall);
                }
            }

            if (confirmToolOutputs.Count == 0)
                return null;

            string submitUri = $"{ServiceConfiguration.ChatGPTApiUrl}/threads/{currentThreadID}/runs/{runID}/submit_tool_outputs";
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, submitUri))
            {
                JObject messagePayload = new JObject()
                {
                    ["tool_outputs"] = confirmToolOutputs
                };

                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceConfiguration.ChatGPTApiKey);
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");
                requestMessage.Content = new StringContent(messagePayload.ToString(), Encoding.UTF8, "application/json");

                HttpResponseMessage submitResponse = await _httpClient.SendAsync(requestMessage);
                if (submitResponse.IsSuccessStatusCode)
                {
                    return functionCalls;
                }
                else
                {
                    string responseContent = await submitResponse.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Error submitting tool outputs: {submitResponse.ReasonPhrase}, Content: {responseContent}");
                }
            }

            return null;
        }

        private async Task<string?> GetLastMessageFromCurrentThread()
        {
            if (string.IsNullOrWhiteSpace(currentThreadID))
            {
                Debug.WriteLine($"Failed to get last message from current thread- current thread not set.");

                return null;
            }

            string requestUri = $"{ServiceConfiguration.ChatGPTApiUrl}/threads/{currentThreadID}/messages";

            Debug.WriteLine($"AI prompt Get Last Message Assistant API request (no payload)");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceConfiguration.ChatGPTApiKey);
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

        private async Task<JObject?> GetAssistantAIRunState(string runID)
        {
            if (string.IsNullOrWhiteSpace(currentThreadID))
            {
                Debug.WriteLine($"Error communicating with ChatGPT Get Run State Assistant API endpoint: current thread ID not set.");

                return null;
            }

            if (string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiKey) || string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiUrl))
            {
                Debug.WriteLine($"Error communicating with ChatGPT Get Run State Assistant API endpoint: API key or url not configured.");

                return null;
            }

            string requestUri = $"{ServiceConfiguration.ChatGPTApiUrl}/threads/{currentThreadID}/runs/{runID}";

            Debug.WriteLine($"Polling for Assistant AI thread run completion...");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceConfiguration.ChatGPTApiKey);
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");

                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject responseObject = JObject.Parse(jsonResponse);

                    return responseObject;
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
            if (string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiKey) || string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiUrl))
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
            string requestUri = $"{ServiceConfiguration.ChatGPTApiUrl}/threads/{currentThreadID}/messages";

            Debug.WriteLine($"AI prompt Add Message Assistant API request payload: {requestString}");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceConfiguration.ChatGPTApiKey);
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
            if (string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiKey) || string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiUrl))
            {
                Debug.WriteLine($"Error communicating with ChatGPT Create Thread Assistant API endpoint: API key or url not configured.");

                return false;
            }

            string requestUri = $"{ServiceConfiguration.ChatGPTApiUrl}/threads";

            Debug.WriteLine($"AI prompt Create Thread Assistant API request (no payload)");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceConfiguration.ChatGPTApiKey);
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
                    Debug.WriteLine($"Error communicating with ChatGPT Create Thread Assistant API endpoint at {ServiceConfiguration.ChatGPTApiUrl}. Reason: {response.ReasonPhrase}");
                }

                return false;
            }
        }
    }
}
