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
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage taskListTab;
        private System.Windows.Forms.TabPage chatTab;
        private System.Windows.Forms.TextBox taskTextBox;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button editButton;
        private System.Windows.Forms.ListBox tasksListBox;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.Button completeButton;
        private System.Windows.Forms.Button sendButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            tabControl = new TabControl();
            taskListTab = new TabPage();
            taskTextBox = new TextBox();
            addButton = new Button();
            editButton = new Button();
            tasksListBox = new ListBox();
            removeButton = new Button();
            completeButton = new Button();
            chatTab = new TabPage();
            responseTextbox = new TextBox();
            responseLabel = new Label();
            requestLabel = new Label();
            requestTextbox = new TextBox();
            sendButton = new Button();
            tabControl.SuspendLayout();
            taskListTab.SuspendLayout();
            chatTab.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl.Controls.Add(taskListTab);
            tabControl.Controls.Add(chatTab);
            tabControl.Location = new Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(366, 422);
            tabControl.TabIndex = 0;
            tabControl.SelectedIndexChanged += TabControl_TabIndexChanged;
            // 
            // taskListTab
            // 
            taskListTab.Controls.Add(taskTextBox);
            taskListTab.Controls.Add(addButton);
            taskListTab.Controls.Add(editButton);
            taskListTab.Controls.Add(tasksListBox);
            taskListTab.Controls.Add(removeButton);
            taskListTab.Controls.Add(completeButton);
            taskListTab.Location = new Point(4, 24);
            taskListTab.Name = "taskListTab";
            taskListTab.Padding = new Padding(3);
            taskListTab.Size = new Size(358, 394);
            taskListTab.TabIndex = 0;
            taskListTab.Text = "Task List";
            taskListTab.UseVisualStyleBackColor = true;
            // 
            // taskTextBox
            // 
            taskTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            taskTextBox.Location = new Point(7, 14);
            taskTextBox.Name = "taskTextBox";
            taskTextBox.Size = new Size(263, 23);
            taskTextBox.TabIndex = 0;
            // 
            // addButton
            // 
            addButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            addButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            addButton.Location = new Point(280, 13);
            addButton.Name = "addButton";
            addButton.Size = new Size(72, 22);
            addButton.TabIndex = 1;
            addButton.Text = "Add";
            addButton.UseVisualStyleBackColor = true;
            addButton.Click += addButton_Click;
            // 
            // editButton
            // 
            editButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            editButton.Location = new Point(280, 41);
            editButton.Name = "editButton";
            editButton.Size = new Size(72, 23);
            editButton.TabIndex = 5;
            editButton.Text = "Edit";
            editButton.UseVisualStyleBackColor = true;
            editButton.Click += editButton_Click;
            // 
            // tasksListBox
            // 
            tasksListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tasksListBox.FormattingEnabled = true;
            tasksListBox.ItemHeight = 15;
            tasksListBox.Location = new Point(7, 47);
            tasksListBox.Name = "tasksListBox";
            tasksListBox.Size = new Size(263, 334);
            tasksListBox.TabIndex = 2;
            // 
            // removeButton
            // 
            removeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            removeButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            removeButton.Location = new Point(280, 70);
            removeButton.Name = "removeButton";
            removeButton.Size = new Size(72, 22);
            removeButton.TabIndex = 3;
            removeButton.Text = "Remove";
            removeButton.UseVisualStyleBackColor = true;
            removeButton.Click += removeButton_Click;
            // 
            // completeButton
            // 
            completeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            completeButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            completeButton.Location = new Point(280, 98);
            completeButton.Name = "completeButton";
            completeButton.Size = new Size(72, 22);
            completeButton.TabIndex = 4;
            completeButton.Text = "Complete";
            completeButton.UseVisualStyleBackColor = true;
            completeButton.Click += completeButton_Click;
            // 
            // chatTab
            // 
            chatTab.Controls.Add(responseTextbox);
            chatTab.Controls.Add(responseLabel);
            chatTab.Controls.Add(requestLabel);
            chatTab.Controls.Add(requestTextbox);
            chatTab.Controls.Add(sendButton);
            chatTab.Location = new Point(4, 24);
            chatTab.Name = "chatTab";
            chatTab.Padding = new Padding(3);
            chatTab.Size = new Size(358, 394);
            chatTab.TabIndex = 1;
            chatTab.Text = "Chat";
            chatTab.UseVisualStyleBackColor = true;
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
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(366, 422);
            Controls.Add(tabControl);
            Name = "MainForm";
            Text = "Task List and Chat";
            Load += MainForm_Load;
            tabControl.ResumeLayout(false);
            taskListTab.ResumeLayout(false);
            taskListTab.PerformLayout();
            chatTab.ResumeLayout(false);
            chatTab.PerformLayout();
            ResumeLayout(false);
        }

        private TextBox requestTextbox;
        private TextBox responseTextbox;
        private Label responseLabel;
        private Label requestLabel;
    }
}
