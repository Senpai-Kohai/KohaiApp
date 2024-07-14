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
        private TabControl tabControl;

        // menus
        private MenuStrip menuStrip;
        private ToolStripMenuItem projectsMenu;
        private ToolStripMenuItem createProject_MenuItem;
        private ToolStripMenuItem editProject_MenuItem;
        private ToolStripMenuItem loadProject_MenuItem;
        private ToolStripMenuItem recentProjects_MenuItem;

        // projects
        private TabPage projectTab;
        private TextBox projectNameTextBox;
        private TextBox projectAuthorTextBox;
        private TextBox projectDescriptionTextBox;
        private Button projectSaveButton;

        // ai
        private TabPage aiTab;
        private Button aiSendButton;
        private TextBox aiRequestTextbox;
        private TextBox aiResponseTextbox;
        private Label aiResponseLabel;
        private Label aiRequestLabel;

        // tasks
        private TabPage taskListTab;
        private TextBox taskTextBox;
        private Button taskAddButton;
        private Button taskEditButton;
        private ListBox tasksListBox;
        private Button taskRemoveButton;
        private Button taskCompleteButton;

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
            menuStrip = new MenuStrip();
            projectsMenu = new ToolStripMenuItem();
            createProject_MenuItem = new ToolStripMenuItem();
            editProject_MenuItem = new ToolStripMenuItem();
            loadProject_MenuItem = new ToolStripMenuItem();
            recentProjects_MenuItem = new ToolStripMenuItem();
            tabControl = new TabControl();
            projectTab = new TabPage();
            aiTab = new TabPage();
            taskListTab = new TabPage();
            menuStrip.SuspendLayout();
            tabControl.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new ToolStripItem[] { projectsMenu });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(366, 24);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "menuStrip";
            // 
            // projectsMenu
            // 
            projectsMenu.DropDownItems.AddRange(new ToolStripItem[] { createProject_MenuItem, editProject_MenuItem, loadProject_MenuItem, recentProjects_MenuItem });
            projectsMenu.Name = "projectsMenu";
            projectsMenu.Size = new Size(61, 20);
            projectsMenu.Text = "Projects";
            // 
            // createProject_MenuItem
            // 
            createProject_MenuItem.Name = "createProject_MenuItem";
            createProject_MenuItem.Size = new Size(110, 22);
            createProject_MenuItem.Text = "Create";
            createProject_MenuItem.Click += CreateProject_MenuItem_Click;
            // 
            // editProject_MenuItem
            // 
            editProject_MenuItem.Name = "editProject_MenuItem";
            editProject_MenuItem.Size = new Size(110, 22);
            editProject_MenuItem.Text = "Edit";
            editProject_MenuItem.Click += EditProject_MenuItem_Click;
            // 
            // loadProject_MenuItem
            // 
            loadProject_MenuItem.Name = "loadProject_MenuItem";
            loadProject_MenuItem.Size = new Size(110, 22);
            loadProject_MenuItem.Text = "Load";
            loadProject_MenuItem.Click += LoadProject_MenuItem_Click;
            // 
            // recentProjects_MenuItem
            // 
            recentProjects_MenuItem.Name = "recentProjects_MenuItem";
            recentProjects_MenuItem.Size = new Size(110, 22);
            recentProjects_MenuItem.Text = "Recent";

            InitializeProjectComponents();
            InitializeAIComponents();
            InitializeTaskListComponents();

            // 
            // tabControl
            // 
            tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl.Controls.Add(projectTab);
            tabControl.Controls.Add(aiTab);
            tabControl.Controls.Add(taskListTab);
            tabControl.Location = new Point(0, 27);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(366, 392);
            tabControl.TabIndex = 0;
            tabControl.SelectedIndexChanged += TabControl_TabIndexChanged;
            // 
            // projectTab
            // 
            projectTab.Location = new Point(4, 24);
            projectTab.Name = "projectTab";
            projectTab.Size = new Size(358, 364);
            projectTab.TabIndex = 0;
            // 
            // aiTab
            // 
            aiTab.Location = new Point(4, 24);
            aiTab.Name = "aiTab";
            aiTab.Size = new Size(358, 394);
            aiTab.TabIndex = 1;
            // 
            // taskListTab
            // 
            taskListTab.Location = new Point(4, 24);
            taskListTab.Name = "taskListTab";
            taskListTab.Size = new Size(358, 394);
            taskListTab.TabIndex = 2;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(366, 422);
            Controls.Add(menuStrip);
            Controls.Add(tabControl);
            MainMenuStrip = menuStrip;
            Name = "MainForm";
            Text = "Helper App (No Project Loaded)";
            Load += MainForm_Load;
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            tabControl.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private void InitializeProjectComponents()
        {
            _tabSelectedActions[projectTab] = OnProjectTabSelected;

            projectNameTextBox = new TextBox();
            projectAuthorTextBox = new TextBox();
            projectDescriptionTextBox = new TextBox();
            projectSaveButton = new Button();

            // Project Tab
            projectTab.Controls.Add(projectNameTextBox);
            projectTab.Controls.Add(projectAuthorTextBox);
            projectTab.Controls.Add(projectDescriptionTextBox);
            projectTab.Controls.Add(projectSaveButton);
            projectTab.Location = new System.Drawing.Point(4, 22);
            projectTab.Name = "projectTab";
            projectTab.Padding = new Padding(3);
            projectTab.Size = new System.Drawing.Size(792, 424);
            projectTab.TabIndex = 2;
            projectTab.Text = "Project";
            projectTab.UseVisualStyleBackColor = true;

            // Project Name TextBox
            projectNameTextBox.Location = new System.Drawing.Point(20, 20);
            projectNameTextBox.Name = "projectNameTextBox";
            projectNameTextBox.Size = new System.Drawing.Size(300, 20);
            projectNameTextBox.TabIndex = 0;
            projectNameTextBox.PlaceholderText = "Project Name";

            // Project Author TextBox
            projectAuthorTextBox.Location = new System.Drawing.Point(20, 60);
            projectAuthorTextBox.Name = "projectAuthorTextBox";
            projectAuthorTextBox.Size = new System.Drawing.Size(300, 20);
            projectAuthorTextBox.TabIndex = 1;
            projectAuthorTextBox.PlaceholderText = "Author";

            // Project Description TextBox
            projectDescriptionTextBox.Location = new System.Drawing.Point(20, 100);
            projectDescriptionTextBox.Name = "projectDescriptionTextBox";
            projectDescriptionTextBox.Size = new System.Drawing.Size(300, 20);
            projectDescriptionTextBox.TabIndex = 2;
            projectDescriptionTextBox.PlaceholderText = "Description";

            // Save Project Button
            projectSaveButton.Location = new System.Drawing.Point(20, 140);
            projectSaveButton.Name = "saveProjectButton";
            projectSaveButton.Size = new System.Drawing.Size(75, 23);
            projectSaveButton.TabIndex = 3;
            projectSaveButton.Text = "Save";
            projectSaveButton.UseVisualStyleBackColor = true;
            projectSaveButton.Click += new EventHandler(SaveProject_Button_Click);
        }

        private void InitializeTaskListComponents()
        {
            _tabSelectedActions[taskListTab] = OnTaskListTabSelected;

            taskTextBox = new TextBox();
            taskAddButton = new Button();
            taskEditButton = new Button();
            tasksListBox = new ListBox();
            taskRemoveButton = new Button();
            taskCompleteButton = new Button();

            // 
            // taskListTab
            // 
            taskListTab.Controls.Add(taskTextBox);
            taskListTab.Controls.Add(taskAddButton);
            taskListTab.Controls.Add(taskEditButton);
            taskListTab.Controls.Add(tasksListBox);
            taskListTab.Controls.Add(taskRemoveButton);
            taskListTab.Controls.Add(taskCompleteButton);
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
            taskAddButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            taskAddButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            taskAddButton.Location = new Point(280, 13);
            taskAddButton.Name = "addButton";
            taskAddButton.Size = new Size(72, 22);
            taskAddButton.TabIndex = 1;
            taskAddButton.Text = "Add";
            taskAddButton.UseVisualStyleBackColor = true;
            taskAddButton.Click += taskAddButton_Click;
            // 
            // editButton
            // 
            taskEditButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            taskEditButton.Location = new Point(280, 41);
            taskEditButton.Name = "editButton";
            taskEditButton.Size = new Size(72, 23);
            taskEditButton.TabIndex = 5;
            taskEditButton.Text = "Edit";
            taskEditButton.UseVisualStyleBackColor = true;
            taskEditButton.Click += taskEditButton_Click;
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
            taskRemoveButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            taskRemoveButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            taskRemoveButton.Location = new Point(280, 70);
            taskRemoveButton.Name = "removeButton";
            taskRemoveButton.Size = new Size(72, 22);
            taskRemoveButton.TabIndex = 3;
            taskRemoveButton.Text = "Remove";
            taskRemoveButton.UseVisualStyleBackColor = true;
            taskRemoveButton.Click += taskRemoveButton_Click;
            // 
            // completeButton
            // 
            taskCompleteButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            taskCompleteButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            taskCompleteButton.Location = new Point(280, 98);
            taskCompleteButton.Name = "completeButton";
            taskCompleteButton.Size = new Size(72, 22);
            taskCompleteButton.TabIndex = 4;
            taskCompleteButton.Text = "Complete";
            taskCompleteButton.UseVisualStyleBackColor = true;
            taskCompleteButton.Click += taskCompleteButton_Click;
        }

        private void InitializeAIComponents()
        {
            _tabSelectedActions[aiTab] = OnAITabSelected;

            aiResponseTextbox = new TextBox();
            aiResponseLabel = new Label();
            aiRequestLabel = new Label();
            aiRequestTextbox = new TextBox();
            aiSendButton = new Button();

            // 
            // aiTab
            // 
            aiTab.Controls.Add(aiResponseTextbox);
            aiTab.Controls.Add(aiResponseLabel);
            aiTab.Controls.Add(aiRequestLabel);
            aiTab.Controls.Add(aiRequestTextbox);
            aiTab.Controls.Add(aiSendButton);
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
            aiResponseTextbox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            aiResponseTextbox.Location = new Point(6, 169);
            aiResponseTextbox.Multiline = true;
            aiResponseTextbox.Name = "responseTextbox";
            aiResponseTextbox.PlaceholderText = "Please make a request.";
            aiResponseTextbox.ReadOnly = true;
            aiResponseTextbox.Size = new Size(344, 217);
            aiResponseTextbox.TabIndex = 3;
            // 
            // responseLabel
            // 
            aiResponseLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            aiResponseLabel.AutoSize = true;
            aiResponseLabel.Location = new Point(293, 151);
            aiResponseLabel.Name = "responseLabel";
            aiResponseLabel.Size = new Size(57, 15);
            aiResponseLabel.TabIndex = 2;
            aiResponseLabel.Text = "Response";
            // 
            // requestLabel
            // 
            aiRequestLabel.AutoSize = true;
            aiRequestLabel.Location = new Point(8, 3);
            aiRequestLabel.Name = "requestLabel";
            aiRequestLabel.Size = new Size(49, 15);
            aiRequestLabel.TabIndex = 1;
            aiRequestLabel.Text = "Request";
            // 
            // requestTextbox
            // 
            aiRequestTextbox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            aiRequestTextbox.Location = new Point(6, 21);
            aiRequestTextbox.Multiline = true;
            aiRequestTextbox.Name = "requestTextbox";
            aiRequestTextbox.Size = new Size(344, 106);
            aiRequestTextbox.TabIndex = 0;
            // 
            // sendButton
            // 
            aiSendButton.BackColor = Color.FromArgb(192, 255, 192);
            aiSendButton.Location = new Point(6, 133);
            aiSendButton.Name = "sendButton";
            aiSendButton.Size = new Size(100, 30);
            aiSendButton.TabIndex = 4;
            aiSendButton.Text = "Send";
            aiSendButton.UseVisualStyleBackColor = false;
            aiSendButton.Click += aiSendButton_Click;
        }
    }
}
