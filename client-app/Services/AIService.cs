using System;
using System.Diagnostics;
using System.Text;
using OpenAI.Assistants;
using System.Linq;
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

            if (!ConfigurationUtils.ValidateConfiguration(ServiceConfiguration))
            {
                Debug.WriteLine($"Error starting AI service: required configuration not set.");

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

            _chatClient ??= _openAIClient?.GetChatClient(model) ?? throw new NullReferenceException("Chat client is null");

            try
            {
                var apiOptions = new OpenAI.Chat.ChatCompletionOptions()
                {
                    EndUserId = user ?? "user"
                };
                var result = await _chatClient.CompleteChatAsync([new OpenAI.Chat.AssistantChatMessage(message)], apiOptions, Program.ShutdownTokenSource.Token);

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

            if (string.IsNullOrWhiteSpace(name) || string.Equals(_currentAssistantName, name, StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine($"Returning existing Assistant AI [{_currentAssistantName}] with ID [{_currentAssistantID}].");

                return _currentAssistantID;
            }

            try
            {
                var requestOptions = new AssistantCollectionOptions()
                {
                    PageSizeLimit = 100,
                    Order = AssistantCollectionOrder.Descending
                };
                var collResult = _assistantClient.GetAssistantsAsync(requestOptions, Program.ShutdownTokenSource.Token);

                try
                {
                    var collEnumerator = collResult.GetAsyncEnumerator();
                    while (await collEnumerator.MoveNextAsync())
                    {
                        var assistant = collEnumerator.Current;
                        if (assistant == null || assistant.Name == null)
                            continue;

                        if (string.Equals(name, assistant.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            Debug.WriteLine($"Assistant [{assistant.Name}] located, ID: [{assistant.Id}]");

                            _currentAssistantName = assistant.Name;
                            _currentAssistantID = assistant.Id;

                            return _currentAssistantID;
                        }
                    }
                }
                catch (Exception exc)
                {
                    Debug.WriteLine($"Error handling GetAssistants response: {exc}");
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"Error communicating with ChatGPT completion API endpoint. Exception: {exc}");

                return null;
            }

            Debug.WriteLine($"Existing Assistant AI with name [{name}] could not be located.");

            return null;
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

            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.WriteLine($"A name is required when generating a new assistant.");

                return null;
            }

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
            _assistantClient ??= _openAIClient?.GetAssistantClient() ?? throw new NullReferenceException("Chat client is null");
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.

            if (string.Equals(_currentAssistantName, name, StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine($"Assistant AI with name [{_currentAssistantName}] exists already- returning ID [{_currentAssistantID}].");

                return _currentAssistantID;
            }

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
            => await CreateAssistantMessageAsync([message], role);

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
            => await CreateAssistantMessageAsync(threadID, [message], role);

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
                apiOptions.Metadata["user"] = "Senpai";

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

            Debug.WriteLine($"Assistant AI established with ID [{assistantID}].");

            var threadID = await GetCurrentThreadIDAsync(loadMostRecent: true) ?? await CreateAssistantThreadAsync();
            if (string.IsNullOrWhiteSpace(threadID))
            {
                Debug.WriteLine($"Error: failed to get or create thread for assistant with ID [{assistantID}].");

                return null;
            }

            Debug.WriteLine($"Thread created with ID [{threadID}].");

            if (!await CreateAssistantMessageAsync(threadID, message, role))
            {
                Debug.WriteLine($"Error: failed to publish message to assistant thread with ID [{threadID}].");

                return null;
            }

            Debug.WriteLine($"New message on thread created.");

            var runID = await RunAssistantThreadAsync();
            if (string.IsNullOrWhiteSpace(runID))
            {
                Debug.WriteLine($"Error: null run ID from attempt to run assistant thread with ID [{threadID}].");

                return null;
            }

            Debug.WriteLine($"Started AI thread processing- now running with ID [{runID}].");

            if (!await PollAssistantThreadUntilCompletedAsync(runID))
            {
                Debug.WriteLine($"Error: Assistant thread run with id [{runID}] did not complete within the expiration time.");

                return null;
            }

            Debug.WriteLine($"Thread processing run complete. Checking for new responses from Assistant AI.");

            var lastMessage = await GetLastAssistantMessageAsync();
            if (string.IsNullOrWhiteSpace(lastMessage))
            {
                Debug.WriteLine($"Error: failed to get last assistant thread message for run with ID [{runID}].");
            }

            Debug.WriteLine($"New Assistant AI response was received- returning to requester.");

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

            return (await GetAssistantMessagesAsync(_currentThreadID, MessageCollectionOrder.Descending))?.FirstOrDefault();
        }

        /// <summary>
        /// Gets the messages in the current assistant thread.
        /// </summary>
        /// <param name="listOrder">The order the messages are returned in.</param>
        /// <returns>The messages in the current thread.</returns>
        public async Task<IEnumerable<string>?> GetAssistantMessagesAsync(MessageCollectionOrder? listOrder = null)
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
        public async Task<IEnumerable<string>?> GetAssistantMessagesAsync(string threadID, MessageCollectionOrder? listOrder)
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
                var results = _assistantClient.GetMessagesAsync(threadID, new MessageCollectionOptions() { Order = listOrder }, Program.ShutdownTokenSource.Token);

                var messages = new List<string>();

                await foreach (var result in results)
                {
                    var newMessage = result?.Content?[0]?.Text;

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

            _assistantClient ??= _openAIClient?.GetAssistantClient() ?? throw new NullReferenceException("Chat client is null");

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
    }
}
