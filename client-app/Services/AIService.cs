using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.Assistants;
using System.Linq;
using System.ClientModel.Primitives;
using OpenAI;
using client_app.Services.Configuration;

namespace client_app.Services
{
    /// <summary>
    /// Responsible for wrapping the OpenAI SDK / client.
    /// </summary>
    public class AIService : ServiceBase<AIServiceConfiguration>
    {
        private OpenAI.Chat.ChatClient? _chatClient;
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
        private AssistantClient? _assistantClient;
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
        private OpenAI.Files.FileClient? _fileClient;

        private readonly ProjectService _projectService;
        private readonly OpenAIClient? _openAIClient;
        private readonly HttpClient? _httpClient;

        private string? _currentAssistantName;
        private string? _currentAssistantID;
        private string? _currentThreadID;

        public override bool ServiceRunning => _openAIClient != null && _httpClient != null;

        public AIService(HttpClient httpClient, ProjectService projectService)
        {
            _projectService = projectService;

            if (ServiceConfiguration.ChatGPTApiKey == null)
            {
                Debug.WriteLine($"Error starting AI service: API key not configured.");

                return;
            }

            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _openAIClient = new OpenAIClient(ServiceConfiguration.ChatGPTApiKey);
        }

        /// <summary>
        /// Send a message to the chat completion API. Doesn't use the Assistant AI / APIs, so it's much simpler.
        /// </summary>
        /// <param name="message">The message/prompt to send to the GPT.</param>
        /// <param name="user">The user to send the message as.</param>
        /// <param name="model">The model of GPT to use.</param>
        /// <returns>The message response from the GPT.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public string? CompleteChat(string message, string? user = null, string? model = null)
            => CompleteChatAsync(message, user, model).Result;

        /// <summary>
        /// Send a message to the chat completion API. Doesn't use the Assistant AI / APIs, so it's much simpler.
        /// </summary>
        /// <param name="message">The message/prompt to send to the GPT.</param>
        /// <param name="user">The user to send the message as.</param>
        /// <param name="model">The model of GPT to use.</param>
        /// <returns>The message response from the GPT.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<string?> CompleteChatAsync(string message, string? user = "user", string? model = "gpt-4o")
        {
            if (!ServiceRunning)
            {
                Debug.WriteLine($"Error communicating with ChatGPT API: AI service is not running.");

                return null;
            }

            if (_chatClient == null)
                _chatClient = _openAIClient?.GetChatClient(model) ?? throw new NullReferenceException("Chat client is null");

            try
            {
                var apiOptions = new OpenAI.Chat.ChatCompletionOptions()
                {
                    User = user ?? "user"
                };
                var result = await _chatClient.CompleteChatAsync(new[] { new OpenAI.Chat.AssistantChatMessage(message) }, apiOptions, Program.ShutdownTokenSource.Token);

                return result.Value.Content.FirstOrDefault()?.Text;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"Error communicating with ChatGPT completion API endpoint. Exception: {exc}");

                return null;
            }
        }

        /// <summary>
        /// Fetches an assistant ID by name.
        /// </summary>
        /// <param name="name">The name of the assistant to fetch.</param>
        /// <returns>The id of the assistant, if one found with name.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<string?> GetAssistantAsync(string? name = "Kohai")
        {
            if (!ServiceRunning)
            {
                Debug.WriteLine($"Error communicating with ChatGPT API: AI service is not running.");

                return null;
            }

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
            _assistantClient ??= _openAIClient?.GetAssistantClient() ?? throw new NullReferenceException("Chat client is null");
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.

            if (string.Equals(_currentAssistantName, name, StringComparison.OrdinalIgnoreCase))
                return _currentAssistantID;

            try
            {
                var apiOptions = new RequestOptions()
                {
                    CancellationToken = Program.ShutdownTokenSource.Token
                };
                var result = await _assistantClient.GetAssistantAsync(name, apiOptions);

                try
                {
                    var resultStr = result.GetRawResponse()?.Content?.ToString();
                    if (!string.IsNullOrWhiteSpace(resultStr))
                    {
                        var resultObj = JObject.Parse(resultStr);
                        var assistantID = (string?)resultObj["id"];

                        if (string.IsNullOrWhiteSpace(assistantID))
                        {
                            _currentAssistantName = name;
                            _currentAssistantID = assistantID;
                        }

                        return assistantID;
                    }

                    Debug.WriteLine($"Failed to determine Assistant ID from API response. Response: {resultStr}");
                }
                catch (Exception exc)
                {
                    Debug.WriteLine($"Error parsing GetAssistant response as JSON: {exc}");
                }

                return null;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"Error communicating with ChatGPT completion API endpoint. Exception: {exc}");

                return null;
            }
        }

        /// <summary>
        /// Create a new assistant, but only if one doesn't already exist with the same name.
        /// </summary>
        /// <param name="name">The name of the assistant to create.</param>
        /// <param name="model">The model of the GPT to use when creating the assistant.</param>
        /// <returns>The new assistant ID.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<string?> CreateAssistantAsync(string? name = "Kohai", string? model = "gpt-4o")
        {
            if (!ServiceRunning)
            {
                Debug.WriteLine($"Error communicating with ChatGPT API: AI service is not running.");

                return null;
            }

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
            _assistantClient ??= _openAIClient?.GetAssistantClient() ?? throw new NullReferenceException("Chat client is null");
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.

            if (string.Equals(_currentAssistantName, name, StringComparison.OrdinalIgnoreCase))
                return _currentAssistantID;

            try
            {
                var apiOptions = new AssistantCreationOptions()
                {
                    Name = name
                };
                var result = await _assistantClient.CreateAssistantAsync(model, apiOptions, Program.ShutdownTokenSource.Token);
                var newAssistantID = result.Value.Id;

                if (!string.IsNullOrWhiteSpace(newAssistantID))
                {
                    _currentAssistantID = newAssistantID;
                    _currentAssistantName = name;
                }


                return newAssistantID;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"Error communicating with ChatGPT completion API endpoint. Exception: {exc}");

                return null;
            }
        }

        /// <summary>
        /// Returns the ID of the currently loaded assistant AI thread.
        /// </summary>
        /// <param name="loadMostRecent">Automatically load the most recent thread for the project, if one not set.</param>
        /// <returns>The current thread's ID.</returns>
        public async Task<string?> GetCurrentThreadIDAsync(bool loadMostRecent = false)
        {
            await Task.CompletedTask;

            if (!string.IsNullOrWhiteSpace(_currentThreadID))
                return _currentThreadID;

            if (loadMostRecent)
            {
                var projectThreadID = _projectService.GetCurrentProject()?.ThreadID;
                if (!string.IsNullOrWhiteSpace(projectThreadID))
                    _currentThreadID = projectThreadID;
            }

            return _currentThreadID;
        }

        /// <summary>
        /// Creates a new assistant thread, for messages to be published to and processed by the assistant AI.
        /// </summary>
        /// <returns>The assistant thread ID created/fetched.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<string?> CreateAssistantThreadAsync()
        {
            if (!ServiceRunning)
            {
                Debug.WriteLine($"Error communicating with ChatGPT API: AI service is not running.");

                return null;
            }

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
            _assistantClient ??= _openAIClient?.GetAssistantClient() ?? throw new NullReferenceException("Chat client is null");
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.

            try
            {
                var apiOptions = new ThreadCreationOptions()
                {
                };
                // just for debugging purposes
                apiOptions.Metadata["owner"] = "Kohai";

                var result = await _assistantClient.CreateThreadAsync(apiOptions, Program.ShutdownTokenSource.Token);

                _currentThreadID = result.Value.Id;

                return _currentThreadID;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"Error communicating with ChatGPT completion API endpoint. Exception: {exc}");

                return null;
            }
        }

        /// <summary>
        /// Deletes an existant Assistant Thread.
        /// </summary>
        /// <param name="threadID">The ID of the thread to delete.</param>
        /// <returns>Whether the thread was successfully deleted.</returns>
        public async Task<bool> DeleteAssistantThreadAsync(string threadID)
        {
            if (!ServiceRunning)
            {
                Debug.WriteLine($"Error communicating with ChatGPT API: AI service is not running.");

                return false;
            }

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
            _assistantClient ??= _openAIClient?.GetAssistantClient() ?? throw new NullReferenceException("Chat client is null");
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.

            try
            {
                var result = await _assistantClient.DeleteThreadAsync(threadID, Program.ShutdownTokenSource.Token);

                return true;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"Error communicating with ChatGPT completion API endpoint. Exception: {exc}");

                return false;
            }
        }

        /// <summary>
        /// Publishes one or more messages to the assistant thread.
        /// </summary>
        /// <param name="messages">The messages to publish.</param>
        /// <param name="role">The role of the messages (assistant or user).</param>
        /// <returns>Whether the messages were successfully published to the assistant thread.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<bool> CreateAssistantMessageAsync(string message, MessageRole role = MessageRole.User)
            => await CreateAssistantMessageAsync(new[] { message }, role);

        /// <summary>
        /// Publishes one or more messages to the assistant thread.
        /// </summary>
        /// <param name="messages">The messages to publish.</param>
        /// <param name="role">The role of the messages (assistant or user).</param>
        /// <returns>Whether the messages were successfully published to the assistant thread.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<bool> CreateAssistantMessageAsync(IEnumerable<string> messages, MessageRole role = MessageRole.User)
        {
            if (!ServiceRunning)
            {
                Debug.WriteLine($"Error communicating with ChatGPT API: AI service is not running.");

                return false;
            }

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
            _assistantClient ??= _openAIClient?.GetAssistantClient() ?? throw new NullReferenceException("Chat client is null");
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.

            if (string.IsNullOrWhiteSpace(_currentThreadID))
            {
                Debug.WriteLine($"Error communicating with ChatGPT API: current thread not set.");

                return false;
            }

            return await CreateAssistantMessageAsync(_currentThreadID, messages, role);
        }

        /// <summary>
        /// Publishes one or more messages to the assistant thread.
        /// </summary>
        /// <param name="threadID">The thread ID to publish the messages under.</param>
        /// <param name="message">The message to publish.</param>
        /// <param name="role">The role of the messages (assistant or user).</param>
        /// <returns>Whether the messages were successfully published to the assistant thread.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<bool> CreateAssistantMessageAsync(string threadID, string message, MessageRole role = MessageRole.User)
            => await CreateAssistantMessageAsync(threadID, new[] { message }, role);

        /// <summary>
        /// Publishes one or more messages to the assistant thread.
        /// </summary>
        /// <param name="threadID">The thread ID to publish the messages under.</param>
        /// <param name="messages">The messages to publish.</param>
        /// <param name="role">The role of the messages (assistant or user).</param>
        /// <returns>Whether the messages were successfully published to the assistant thread.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<bool> CreateAssistantMessageAsync(string threadID, IEnumerable<string> messages, MessageRole role = MessageRole.User)
        {
            if (!ServiceRunning)
            {
                Debug.WriteLine($"Error communicating with ChatGPT API: AI service is not running.");

                return false;
            }

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
            _assistantClient ??= _openAIClient?.GetAssistantClient() ?? throw new NullReferenceException("Chat client is null");
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.

            try
            {
                var messageContents = new List<MessageContent>();
                foreach (var message in messages)
                    messageContents.Add(MessageContent.FromText(message));

                var apiOptions = new MessageCreationOptions()
                {
                };
                // just for debugging purposes
                apiOptions.Metadata["owner"] = "kohai";

                var result = await _assistantClient.CreateMessageAsync(threadID, role, messageContents, apiOptions, Program.ShutdownTokenSource.Token);

                return true;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"Error communicating with ChatGPT completion API endpoint. Exception: {exc}");

                return false;
            }
        }

        /// <summary>
        /// Sends a message to the current assistant AI thread, runs it, and waits for the response.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>The assistant AI response.</returns>
        public async Task<string?> CreateAssistantMessageAndRunAsync(string message, MessageRole role = MessageRole.User)
        {
            var assistantID = await GetAssistantAsync() ?? await CreateAssistantAsync();
            if (string.IsNullOrWhiteSpace(assistantID))
            {
                Debug.WriteLine($"Error: failed to get or create assistant.");

                return null;
            }

            var threadID = await GetCurrentThreadIDAsync(loadMostRecent: true) ?? await CreateAssistantThreadAsync();
            if (string.IsNullOrWhiteSpace(threadID))
            {
                Debug.WriteLine($"Error: failed to get or create thread for assistant with ID [{assistantID}].");

                return null;
            }

            if (!await CreateAssistantMessageAsync(threadID, message, role))
            {
                Debug.WriteLine($"Error: failed to publish message to assistant thread with ID [{threadID}].");

                return null;
            }

            var runID = await RunAssistantThreadAsync();
            if (string.IsNullOrWhiteSpace(runID))
            {
                Debug.WriteLine($"Error: null run ID from attempt to run assistant thread with ID [{threadID}].");

                return null;
            }

            if (!await PollAssistantThreadUntilCompletedAsync(runID))
            {
                Debug.WriteLine($"Error: Assistant thread run with id [{runID}] did not complete within the expiration time.");

                return null;
            }

            var lastMessage = await GetLastAssistantMessageAsync();
            if (string.IsNullOrWhiteSpace(lastMessage))
            {
                Debug.WriteLine($"Error: failed to get last assistant thread message for run with ID [{runID}].");
            }

            return lastMessage;
        }

        /// <summary>
        /// Gets the last message posted to the current assistant thread.
        /// </summary>
        /// <returns>The last message in the current thread.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<string?> GetLastAssistantMessageAsync()
        {
            if (!ServiceRunning)
            {
                Debug.WriteLine($"Error communicating with ChatGPT API: AI service is not running.");

                return null;
            }

            if (string.IsNullOrWhiteSpace(_currentThreadID))
            {
                Debug.WriteLine($"Error running assistant AI thread: no thread ID currently set.");

                return null;
            }

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
            _assistantClient ??= _openAIClient?.GetAssistantClient() ?? throw new NullReferenceException("Chat client is null");
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.

            return (await GetAssistantMessagesAsync(_currentThreadID, ListOrder.NewestFirst))?.FirstOrDefault();
        }

        /// <summary>
        /// Gets the messages in the current assistant thread.
        /// </summary>
        /// <param name="listOrder">The order the messages are returned in.</param>
        /// <returns>The messages in the current thread.</returns>
        public async Task<IEnumerable<string>?> GetAssistantMessagesAsync(ListOrder? listOrder = null)
        {
            if (string.IsNullOrWhiteSpace(_currentThreadID))
            {
                Debug.WriteLine($"Error running assistant AI thread: no thread ID currently set.");

                return null;
            }

            return await GetAssistantMessagesAsync(_currentThreadID, listOrder);
        }

        /// <summary>
        /// Gets the messages in a given thread, ordered as indicated.
        /// </summary>
        /// <param name="threadID">The ID of the thread to fetch messages for.</param>
        /// <param name="listOrder">The order the messages are returned in.</param>
        /// <returns>The messages in a given thread.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<IEnumerable<string>?> GetAssistantMessagesAsync(string threadID, ListOrder? listOrder)
        {
            if (!ServiceRunning)
            {
                Debug.WriteLine($"Error communicating with ChatGPT API: AI service is not running.");

                return null;
            }

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
            _assistantClient ??= _openAIClient?.GetAssistantClient() ?? throw new NullReferenceException("Chat client is null");
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.

            try
            {
                var results = _assistantClient.GetMessagesAsync(threadID, listOrder, Program.ShutdownTokenSource.Token);

                var messages = new List<string>();

                await foreach (var result in results)
                {
                    var newMessage = result?.Content?.FirstOrDefault()?.Text;

                    if (!string.IsNullOrWhiteSpace(newMessage))
                        messages.Add(newMessage);
                }

                return messages;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"Error communicating with ChatGPT completion API endpoint. Exception: {exc}");

                return null;
            }
        }

        /// <summary>
        /// Will execute running an existing thread, processing all of the messages that have been added to it.
        /// </summary>
        /// <returns>The ID of the run that has started, to monitor via polling.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<string?> RunAssistantThreadAsync()
        {
            if (string.IsNullOrWhiteSpace(_currentAssistantID))
            {
                Debug.WriteLine($"Error running assistant AI thread: no assistant ID currently set.");

                return null;
            }

            if (string.IsNullOrWhiteSpace(_currentThreadID))
            {
                Debug.WriteLine($"Error running assistant AI thread: no thread ID currently set.");

                return null;
            }

            return await RunAssistantThreadAsync(_currentAssistantID, _currentThreadID);
        }

        /// <summary>
        /// Will execute running an existing thread, processing all of the messages that have been added to it.
        /// </summary>
        /// <param name="assistantID">The ID of the assistant running the thread.</param>
        /// <param name="threadID">The ID of the thread to run.</param>
        /// <returns>The ID of the run that has started, to monitor via polling.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<string?> RunAssistantThreadAsync(string assistantID, string threadID)
        {
            if (!ServiceRunning)
            {
                Debug.WriteLine($"Error communicating with ChatGPT API: AI service is not running.");

                return null;
            }

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
            _assistantClient ??= _openAIClient?.GetAssistantClient() ?? throw new NullReferenceException("Chat client is null");
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.

            try
            {
                var apiOptions = new RunCreationOptions()
                {
                };
                // just for debugging purposes
                apiOptions.Metadata["owner"] = "Kohai";

                var result = await _assistantClient.CreateRunAsync(threadID, assistantID, apiOptions, Program.ShutdownTokenSource.Token);

                return result?.Value?.Id;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"Error communicating with ChatGPT completion API endpoint. Exception: {exc}");

                return null;
            }
        }

        /// <summary>
        /// Will poll the running thread until completed, up to a given timeout time.
        /// </summary>
        /// <param name="runID">The ID of the thread run.</param>
        /// <returns>The current run status of the thread.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<bool> PollAssistantThreadUntilCompletedAsync(string runID, TimeSpan? expirationTime = null)
        {
            if (string.IsNullOrWhiteSpace(_currentThreadID))
            {
                Debug.WriteLine($"Error running assistant AI thread: no thread ID currently set.");

                return false;
            }

            return await PollAssistantThreadUntilCompletedAsync(_currentThreadID, runID, expirationTime);
        }

        /// <summary>
        /// Will poll a running thread until completed, up to a given timeout time.
        /// </summary>
        /// <param name="runID">The ID of the thread run.</param>
        /// <param name="threadID">The ID of the thread to run.</param>
        /// <returns>The current run status of the thread.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<bool> PollAssistantThreadUntilCompletedAsync(string threadID, string runID, TimeSpan? expirationTime = null)
        {
            var timeoutTime = DateTimeOffset.UtcNow + (expirationTime ?? TimeSpan.FromSeconds(5));

            while (DateTimeOffset.UtcNow <= timeoutTime)
            {
                var runStatus = await PollAssistantThreadAsync(threadID, runID);

                if (string.Equals(runStatus, RunStatus.Completed.ToString(), StringComparison.OrdinalIgnoreCase))
                    return true;

                if (runStatus == null)
                    return false;

                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }

            return false;
        }

        /// <summary>
        /// Will poll a running thread, returning the current run status.
        /// </summary>
        /// <param name="runID">The ID of the thread run.</param>
        /// <param name="threadID">The ID of the thread to run.</param>
        /// <returns>The current run status of the thread.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<string?> PollAssistantThreadAsync(string threadID, string runID)
        {
            if (!ServiceRunning)
            {
                Debug.WriteLine($"Error communicating with ChatGPT API: AI service is not running.");

                return null;
            }

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
            _assistantClient ??= _openAIClient?.GetAssistantClient() ?? throw new NullReferenceException("Chat client is null");
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.

            try
            {
                var result = await _assistantClient.GetRunAsync(threadID, runID, Program.ShutdownTokenSource.Token);

                return result?.Value.Status.ToString();
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"Error communicating with ChatGPT completion API endpoint. Exception: {exc}");

                return null;
            }
        }


        // OLD NON-SDK METHODS
        public async Task<string?> GetAIAssistantResponseAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiKey) || string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiUrl))
            {
                Debug.WriteLine($"Error communicating with ChatGPT Assistant API endpoint: API key or url not configured.");

                return null;
            }

            if (string.IsNullOrWhiteSpace(_currentThreadID) && !await CreateNewThread())
            {
                Debug.WriteLine($"Could not create a new thread for the Assistant API to use for prompt: {prompt}");

                return null;
            }

            if (!await AddMessageToThread(prompt))
            {
                Debug.WriteLine($"Could not add a new message to thread for the Assistant API to use for prompt: {prompt}");

                return null;
            }

            var response = await RunCurrentAssistantAIThread_AndWaitForResponse();
            if (string.IsNullOrWhiteSpace(response))
            {
                Debug.WriteLine($"Failed to get valid response from Assistant AI for prompt: {prompt}");

                return null;
            }

            try
            {
                if (response.StartsWith('{'))
                {
                    var responseObj = JObject.Parse(response);

                    var functionName = responseObj["function"]?.ToString();
                    if (string.Equals("create_character_profile", functionName, StringComparison.OrdinalIgnoreCase))
                    {
                        var characterNickname = (string?)responseObj["nickname"];
                        var characterAge = (int?)responseObj["age"];
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

            var requestUri = $"{ServiceConfiguration.ChatGPTApiUrl}/assistants";

            Debug.WriteLine($"AI prompt Retrieve Assistant ID API request (no payload)");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceConfiguration.ChatGPTApiKey);
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");

                var response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(jsonResponse);

                    var assistantID = (string?)responseObject?["data"]?[0]?["id"];

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
            if (string.IsNullOrWhiteSpace(_currentThreadID))
            {
                Debug.WriteLine($"Current thread not set, and is unable to be run by the Assistant AI.");

                return null;
            }

            if (string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiKey) || string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiUrl))
            {
                Debug.WriteLine($"Error communicating with ChatGPT Run Thread Assistant API endpoint: API key or url not configured.");

                return null;
            }

            var assistantID = ServiceConfiguration.ChatGPTAssistantAI ?? await RetrieveAssistantAIGuid();
            if (string.IsNullOrWhiteSpace(assistantID))
            {
                Debug.WriteLine($"Could not determine Assistant AI identifier to use, either from ENV or retrieved via API request.");

                return null;
            }

            var requestObject = new JObject(
                new JProperty("assistant_id", assistantID)
            );

            var requestString = requestObject.ToString();
            var requestUri = $"{ServiceConfiguration.ChatGPTApiUrl}/threads/{_currentThreadID}/runs";

            Debug.WriteLine($"AI prompt Run Thread Assistant API request (no payload)");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceConfiguration.ChatGPTApiKey);
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");
                requestMessage.Content = new StringContent(requestString, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(jsonResponse);

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
            if (string.IsNullOrWhiteSpace(_currentThreadID))
            {
                Debug.WriteLine($"Current thread not set, and required in order to wait for run response.");

                return null;
            }

            var runID = await RunCurrentAssistantAIThread();
            if (string.IsNullOrWhiteSpace(runID))
            {
                Debug.WriteLine($"Failed to retrieve run ID from Run Thread Assistant API request.");

                return null;
            }

            var attemptInterval = TimeSpan.FromMilliseconds(ServiceConfiguration.ChatGPTRetryInterval);
            var success = false;

            for (var i = 0; i < ServiceConfiguration.ChatGPTRetryMaxAttempts; i++)
            {
                var responseObj = await GetAssistantAIRunState(runID);
                var runState = (string?)responseObj?["status"];

                Debug.WriteLine($"Current run state: [{runState}]");

                if (string.Equals("requires_action", runState, StringComparison.OrdinalIgnoreCase))
                {
                    var runStatePrintedLines = responseObj?.ToString(Formatting.Indented)?.Split('\n');
                    if (runStatePrintedLines == null)
                        continue;

                    foreach (var runStatePrintedLine in runStatePrintedLines)
                        Debug.WriteLine($"{runStatePrintedLine}");

                    var requiredActionsStr = (await GetFunctionCallsFromRequiredActions(runID, responseObj))?.ToString(Formatting.None);
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

            var confirmToolOutputs = new JArray();
            var functionCalls = new JArray();
            var requiredAction = (JObject?)responseObject["required_action"];

            if (requiredAction == null)
                return null;

            var submitToolOutputs = (JObject?)requiredAction["submit_tool_outputs"];
            if (submitToolOutputs == null)
                return null;

            var toolCalls = (JArray?)submitToolOutputs["tool_calls"];
            if (toolCalls == null)
                return null;

            foreach (var toolCallObj in toolCalls)
            {
                var toolCallID = (string?)toolCallObj["id"];
                var toolStringValue = (string?)toolCallObj["function"]?["arguments"];
                if (string.IsNullOrEmpty(toolStringValue))
                    continue;

                var newFunctionCall = JObject.Parse(toolStringValue);
                if (newFunctionCall != null)
                {
                    confirmToolOutputs.Add(new JObject() { ["tool_call_id"] = toolCallID, ["output"] = toolStringValue });
                    functionCalls.Add(newFunctionCall);
                }
            }

            if (confirmToolOutputs.Count == 0)
                return null;

            var submitUri = $"{ServiceConfiguration.ChatGPTApiUrl}/threads/{_currentThreadID}/runs/{runID}/submit_tool_outputs";
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, submitUri))
            {
                var messagePayload = new JObject()
                {
                    ["tool_outputs"] = confirmToolOutputs
                };

                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceConfiguration.ChatGPTApiKey);
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");
                requestMessage.Content = new StringContent(messagePayload.ToString(), Encoding.UTF8, "application/json");

                var submitResponse = await _httpClient.SendAsync(requestMessage);
                if (submitResponse.IsSuccessStatusCode)
                {
                    return functionCalls;
                }
                else
                {
                    var responseContent = await submitResponse.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Error submitting tool outputs: {submitResponse.ReasonPhrase}, Content: {responseContent}");
                }
            }

            return null;
        }

        private async Task<string?> GetLastMessageFromCurrentThread()
        {
            if (string.IsNullOrWhiteSpace(_currentThreadID))
            {
                Debug.WriteLine($"Failed to get last message from current thread- current thread not set.");

                return null;
            }

            var requestUri = $"{ServiceConfiguration.ChatGPTApiUrl}/threads/{_currentThreadID}/messages";

            Debug.WriteLine($"AI prompt Get Last Message Assistant API request (no payload)");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceConfiguration.ChatGPTApiKey);
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");

                var response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var responseObject = JObject.Parse(jsonResponse);

                        var messageArray = (JArray?)responseObject["data"];
                        var lastMessage = (JObject?)messageArray?.FirstOrDefault();
                        var lastMessageContent = (JObject?)lastMessage?["content"]?.FirstOrDefault();
                        var lastMessageStr = (string?)lastMessageContent?["text"]?["value"];

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
            if (string.IsNullOrWhiteSpace(_currentThreadID))
            {
                Debug.WriteLine($"Error communicating with ChatGPT Get Run State Assistant API endpoint: current thread ID not set.");

                return null;
            }

            if (string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiKey) || string.IsNullOrWhiteSpace(ServiceConfiguration.ChatGPTApiUrl))
            {
                Debug.WriteLine($"Error communicating with ChatGPT Get Run State Assistant API endpoint: API key or url not configured.");

                return null;
            }

            var requestUri = $"{ServiceConfiguration.ChatGPTApiUrl}/threads/{_currentThreadID}/runs/{runID}";

            Debug.WriteLine($"Polling for Assistant AI thread run completion...");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceConfiguration.ChatGPTApiKey);
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");

                var response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(jsonResponse);

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
            var requestString = requestObject.ToString();
            var requestUri = $"{ServiceConfiguration.ChatGPTApiUrl}/threads/{_currentThreadID}/messages";

            Debug.WriteLine($"AI prompt Add Message Assistant API request payload: {requestString}");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceConfiguration.ChatGPTApiKey);
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");
                requestMessage.Content = new StringContent(requestString, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(requestMessage);
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

            var requestUri = $"{ServiceConfiguration.ChatGPTApiUrl}/threads";

            Debug.WriteLine($"AI prompt Create Thread Assistant API request (no payload)");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ServiceConfiguration.ChatGPTApiKey);
                requestMessage.Headers.Add("OpenAI-Beta", "assistants=v2");
                //requestMessage.Headers.Add("Content-Type", "application/json");
                requestMessage.Content = new StringContent("{}", Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var responseObject = JObject.Parse(jsonResponse);

                        _currentThreadID = (string?)responseObject["id"];

                        return _currentThreadID != null;
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
