using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client_app
{
    public partial class MainForm
    {
        private async void OnAITabSelected()
        {
            Debug.WriteLine($"Method: {nameof(OnAITabSelected)}");
            await Task.CompletedTask;

            bool messageAllowed = !string.IsNullOrWhiteSpace(_config?.ChatGPTApiKey) && !string.IsNullOrWhiteSpace(_config?.ChatGPTApiUrl);
            aiSendButton.Enabled = messageAllowed;
            aiAssistantSendButton.Enabled = messageAllowed;

            if (!messageAllowed)
            {
                aiRequestTextbox.Text = "Please configure the ChatGPT API key and URI to use this feature.";
                aiRequestTextbox.ReadOnly = true;
            }
        }

        private async void aiSendButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(aiSendButton_Click)}");
            await Task.CompletedTask;

            string requestText = aiRequestTextbox.Text;
            if (!string.IsNullOrWhiteSpace(requestText))
            {
                string? responseText = await _aiService.GetAICompletionResponseAsync(requestText);
                aiResponseTextbox.Text = responseText;
            }
            else
            {
                MessageBox.Show("Please enter a request.");
            }
        }

        private async void aiAssistantSendButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(aiAssistantSendButton_Click)}");
            await Task.CompletedTask;

            string requestText = aiRequestTextbox.Text;
            if (!string.IsNullOrWhiteSpace(requestText))
            {
                string? responseText = await _aiService.GetAIAssistantResponseAsync(requestText);
                aiResponseTextbox.Text = responseText;
            }
            else
            {
                MessageBox.Show("Please enter a request.");
            }
        }
    }
}
