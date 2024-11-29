using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using client_app.Models;

namespace client_app
{
    public partial class MainForm
    {
        private async void OnAITabSelected()
        {
            Debug.WriteLine($"Method: {nameof(OnAITabSelected)}");
            await Task.CompletedTask;

            var messageAllowed = !string.IsNullOrWhiteSpace(_config?.ChatGPTApiKey) && !string.IsNullOrWhiteSpace(_config?.ChatGPTApiUrl);
            aiSendButton.Enabled = messageAllowed;
            aiAssistantSendButton.Enabled = messageAllowed;

            if (!messageAllowed)
            {
                aiRequestTextbox.Text = "Please configure the ChatGPT API key and URI to use this feature.";
                aiRequestTextbox.ReadOnly = true;
            }
        }

        private async void AISendButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(AISendButton_Click)}");
            await Task.CompletedTask;

            var requestText = aiRequestTextbox.Text;
            if (!string.IsNullOrWhiteSpace(requestText))
            {
                var responseText = await _aiService.CompleteChatAsync(requestText);
                aiResponseTextbox.Text = responseText;
            }
            else
            {
                MessageBox.Show("Please enter a request.");
            }
        }

        private async void AIAssistantSendButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(AIAssistantSendButton_Click)}");
            await Task.CompletedTask;

            var requestText = aiRequestTextbox.Text;
            if (!string.IsNullOrWhiteSpace(requestText))
            {
                var responseText = await _aiService.CreateAssistantMessageAndRunAsync(requestText);
                aiResponseTextbox.Text = responseText;
            }
            else
            {
                MessageBox.Show("Please enter a request.");
            }
        }
    }
}
