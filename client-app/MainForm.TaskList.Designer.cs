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
        private TabPage taskListTab;
        private TextBox taskTextBox;
        private Button addButton;
        private Button editButton;
        private ListBox tasksListBox;
        private Button removeButton;
        private Button completeButton;

        private void InitializeTaskListComponents()
        {
            _tabSelectedActions[taskListTab] = OnTaskListTabSelected;

            taskTextBox = new TextBox();
            addButton = new Button();
            editButton = new Button();
            tasksListBox = new ListBox();
            removeButton = new Button();
            completeButton = new Button();

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
        }
    }
}
