using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.Http;
using System.Text;

namespace client_app
{
    public partial class MainForm : Form
    {
        private List<TaskItem> tasks = new List<TaskItem>();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadTasks();
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            string taskDescription = taskTextBox.Text;
            if (!string.IsNullOrWhiteSpace(taskDescription))
            {
                var task = new TaskItem { Description = taskDescription, IsCompleted = false };
                tasks.Add(task);
                tasksListBox.Items.Add(task.Description);
                taskTextBox.Clear();
                SaveTasks();
            }
            else
            {
                MessageBox.Show("Please enter a task.");
            }
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            if (tasksListBox.SelectedIndex != -1)
            {
                int selectedIndex = tasksListBox.SelectedIndex;
                var task = tasks[selectedIndex];

                using (var editTaskForm = new EditTaskForm(task.Description ?? "", task.IsCompleted))
                {
                    if (editTaskForm.ShowDialog() == DialogResult.OK)
                    {
                        task.Description = editTaskForm.TaskDescription;
                        task.IsCompleted = editTaskForm.IsCompleted;
                        tasksListBox.Items[selectedIndex] = task.Description + (task.IsCompleted ? " (Completed)" : string.Empty);
                        SaveTasks();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a task to edit.");
            }
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            if (tasksListBox.SelectedIndex != -1)
            {
                tasks.RemoveAt(tasksListBox.SelectedIndex);
                tasksListBox.Items.RemoveAt(tasksListBox.SelectedIndex);
                SaveTasks();
            }
            else
            {
                MessageBox.Show("Please select a task to remove.");
            }
        }

        private void completeButton_Click(object sender, EventArgs e)
        {
            if (tasksListBox.SelectedIndex != -1)
            {
                var task = tasks[tasksListBox.SelectedIndex];
                task.IsCompleted = !task.IsCompleted;

                if (task.Description != null)
                {
                    if (task.IsCompleted)
                        tasksListBox.Items[tasksListBox.SelectedIndex] = task.Description + " (Completed)";
                    else
                        tasksListBox.Items[tasksListBox.SelectedIndex] = task.Description;

                    SaveTasks();
                }
            }
            else
            {
                MessageBox.Show("Please select a task to mark as completed.");
            }
        }

        private async void sendButton_Click(object sender, EventArgs e)
        {
            string requestText = requestTextbox.Text;
            if (!string.IsNullOrWhiteSpace(requestText))
            {
                string? responseText = await GetChatGPTResponse(requestText);
                responseTextbox.Text = responseText;
            }
            else
            {
                MessageBox.Show("Please enter a request.");
            }
        }

        private void TabControl_TabIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex != -1)
            {
                TabPage selectedTab = tabControl.TabPages[tabControl.SelectedIndex];
                string tabName = selectedTab.Name;

                if (string.Equals("chatTab", tabName, StringComparison.InvariantCultureIgnoreCase))
                {
                    bool messageAllowed = !string.IsNullOrWhiteSpace(Program.AppConfig?.ChatGPTApiKey) && !string.IsNullOrWhiteSpace(Program.AppConfig?.ChatGPTApiUrl);
                    sendButton.Enabled = messageAllowed;

                    if (!messageAllowed)
                    {
                        requestTextbox.Text = "Please configure the ChatGPT API key and URI to use this feature.";
                        requestTextbox.ReadOnly = true;
                    }
                }
            }
        }

        private async Task<string?> GetChatGPTResponse(string prompt)
        {
            var requestBody = new JObject
            {
                { "model", "gpt-3.5-turbo-16k" },
                { "messages", new JArray() {
                    new JObject {
                        { "role", "user" },
                        { "content", prompt }
                    }
                } },
                { "temperature", 0.7 }
            };

            string jsonBody = requestBody.ToString();
            Debug.WriteLine($"Request body: {jsonBody}");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, Program.AppConfig?.ChatGPTApiUrl))
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Program.AppConfig?.ChatGPTApiKey);
                requestMessage.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await Program.HttpClient.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Response JSON: {jsonResponse}");
                    JObject responseObj = JObject.Parse(jsonResponse);

                    JArray? responseChoices = responseObj["choices"] as JArray;
                    string? responseText = null;

                    if (responseChoices != null && responseChoices.Count > 0)
                    {
                        JObject? choiceMessage = (JObject?)responseChoices[0]?["message"];
                        if (choiceMessage != null)
                        {
                            responseText = (string?)choiceMessage["content"];
                        }
                    }

                    if (string.IsNullOrWhiteSpace(responseText))
                    {
                        Debug.WriteLine("A message could not be parsed from the response.");
                        return "A message could not be parsed from the response.";
                    }

                    return responseText;
                }
                else
                {
                    Debug.WriteLine($"Error: {response.ReasonPhrase}");
                    return $"Error: {response.ReasonPhrase}";
                }
            }
        }

        private void SaveTasks()
        {
            var json = JsonConvert.SerializeObject(tasks, Formatting.Indented);
            File.WriteAllText(Program.FilePath, json);
        }

        private void LoadTasks()
        {
            if (File.Exists(Program.FilePath))
            {
                var json = File.ReadAllText(Program.FilePath);
                tasks = JsonConvert.DeserializeObject<List<TaskItem>>(json) ?? new List<TaskItem>();
                foreach (var task in tasks)
                {
                    tasksListBox.Items.Add(task.Description + (task.IsCompleted ? " (Completed)" : string.Empty));
                }
            }
            else
            {
                tasks = new List<TaskItem>();
            }
        }
    }
}
