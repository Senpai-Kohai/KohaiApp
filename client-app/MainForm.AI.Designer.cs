using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using client_app;
using Newtonsoft.Json;

namespace client_app
{
    public partial class MainForm
    {
        private TabPage aiTab;
        private Button sendButton;
        private TextBox requestTextbox;
        private TextBox responseTextbox;
        private Label responseLabel;
        private Label requestLabel;

        private void InitializeAIComponents()
        {
            _tabSelectedActions[aiTab] = OnAITabSelected;

            responseTextbox = new TextBox();
            responseLabel = new Label();
            requestLabel = new Label();
            requestTextbox = new TextBox();
            sendButton = new Button();

            // 
            // aiTab
            // 
            aiTab.Controls.Add(responseTextbox);
            aiTab.Controls.Add(responseLabel);
            aiTab.Controls.Add(requestLabel);
            aiTab.Controls.Add(requestTextbox);
            aiTab.Controls.Add(sendButton);
            aiTab.Location = new Point(4, 24);
            aiTab.Name = "chatTab";
            aiTab.Padding = new Padding(3);
            aiTab.Size = new Size(358, 394);
            aiTab.TabIndex = 1;
            aiTab.Text = "Chat";
            aiTab.UseVisualStyleBackColor = true;
            // 
            // responseTextbox
            // 
            responseTextbox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            responseTextbox.Location = new Point(6, 169);
            responseTextbox.Multiline = true;
            responseTextbox.Name = "responseTextbox";
            responseTextbox.PlaceholderText = "Please make a request.";
            responseTextbox.ReadOnly = true;
            responseTextbox.Size = new Size(344, 217);
            responseTextbox.TabIndex = 3;
            // 
            // responseLabel
            // 
            responseLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            responseLabel.AutoSize = true;
            responseLabel.Location = new Point(293, 151);
            responseLabel.Name = "responseLabel";
            responseLabel.Size = new Size(57, 15);
            responseLabel.TabIndex = 2;
            responseLabel.Text = "Response";
            // 
            // requestLabel
            // 
            requestLabel.AutoSize = true;
            requestLabel.Location = new Point(8, 3);
            requestLabel.Name = "requestLabel";
            requestLabel.Size = new Size(49, 15);
            requestLabel.TabIndex = 1;
            requestLabel.Text = "Request";
            // 
            // requestTextbox
            // 
            requestTextbox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            requestTextbox.Location = new Point(6, 21);
            requestTextbox.Multiline = true;
            requestTextbox.Name = "requestTextbox";
            requestTextbox.Size = new Size(344, 106);
            requestTextbox.TabIndex = 0;
            // 
            // sendButton
            // 
            sendButton.BackColor = Color.FromArgb(192, 255, 192);
            sendButton.Location = new Point(6, 133);
            sendButton.Name = "sendButton";
            sendButton.Size = new Size(100, 30);
            sendButton.TabIndex = 4;
            sendButton.Text = "Send";
            sendButton.UseVisualStyleBackColor = false;
            sendButton.Click += sendButton_Click;
        }
    }
}
