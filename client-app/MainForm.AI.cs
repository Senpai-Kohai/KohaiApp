using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client_app
{
    public partial class MainForm
    {
        private void OnAITabSelected()
        {
            bool messageAllowed = !string.IsNullOrWhiteSpace(_config?.ChatGPTApiKey) && !string.IsNullOrWhiteSpace(_config?.ChatGPTApiUrl);
            sendButton.Enabled = messageAllowed;

            if (!messageAllowed)
            {
                requestTextbox.Text = "Please configure the ChatGPT API key and URI to use this feature.";
                requestTextbox.ReadOnly = true;
            }
        }

        private async void sendButton_Click(object sender, EventArgs e)
        {
            string requestText = requestTextbox.Text;
            if (!string.IsNullOrWhiteSpace(requestText))
            {
                string? responseText = await _aiService.GetAIResponse(requestText);
                responseTextbox.Text = responseText;
            }
            else
            {
                MessageBox.Show("Please enter a request.");
            }
        }
    }
}
