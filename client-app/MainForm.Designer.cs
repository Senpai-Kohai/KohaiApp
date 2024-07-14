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
            projectNameTextBox = new TextBox();
            projectAuthorTextBox = new TextBox();
            projectDescriptionTextBox = new TextBox();
            projectSaveButton = new Button();
            aiTab = new TabPage();
            aiResponseTextbox = new TextBox();
            aiResponseLabel = new Label();
            aiRequestLabel = new Label();
            aiRequestTextbox = new TextBox();
            aiSendButton = new Button();
            taskListTab = new TabPage();
            taskTextBox = new TextBox();
            taskAddButton = new Button();
            taskEditButton = new Button();
            tasksListBox = new ListBox();
            taskRemoveButton = new Button();
            taskCompleteButton = new Button();
            menuStrip.SuspendLayout();
            tabControl.SuspendLayout();
            projectTab.SuspendLayout();
            aiTab.SuspendLayout();
            taskListTab.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new ToolStripItem[] { projectsMenu });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(584, 24);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "menuStrip";
            // 
            // projectsMenu
            // 
            projectsMenu.DropDownItems.AddRange(new ToolStripItem[] { createProject_MenuItem, editProject_MenuItem, loadProject_MenuItem, recentProjects_MenuItem });
            projectsMenu.Name = "projectsMenu";
            projectsMenu.ShortcutKeyDisplayString = "P";
            projectsMenu.ShortcutKeys = Keys.Alt | Keys.P;
            projectsMenu.Size = new Size(61, 20);
            projectsMenu.Text = "&Projects";
            // 
            // createProject_MenuItem
            // 
            createProject_MenuItem.Name = "createProject_MenuItem";
            createProject_MenuItem.ShortcutKeyDisplayString = "C";
            createProject_MenuItem.ShortcutKeys = Keys.Alt | Keys.C;
            createProject_MenuItem.Size = new Size(180, 22);
            createProject_MenuItem.Text = "&Create";
            createProject_MenuItem.Click += CreateProject_MenuItem_Click;
            // 
            // editProject_MenuItem
            // 
            editProject_MenuItem.Name = "editProject_MenuItem";
            editProject_MenuItem.ShortcutKeyDisplayString = "E";
            editProject_MenuItem.ShortcutKeys = Keys.Alt | Keys.E;
            editProject_MenuItem.Size = new Size(180, 22);
            editProject_MenuItem.Text = "&Edit";
            editProject_MenuItem.Click += EditProject_MenuItem_Click;
            // 
            // loadProject_MenuItem
            // 
            loadProject_MenuItem.Name = "loadProject_MenuItem";
            loadProject_MenuItem.ShortcutKeyDisplayString = "L";
            loadProject_MenuItem.ShortcutKeys = Keys.Alt | Keys.L;
            loadProject_MenuItem.Size = new Size(180, 22);
            loadProject_MenuItem.Text = "&Load";
            loadProject_MenuItem.Click += LoadProject_MenuItem_Click;
            // 
            // recentProjects_MenuItem
            // 
            recentProjects_MenuItem.Name = "recentProjects_MenuItem";
            recentProjects_MenuItem.ShortcutKeyDisplayString = "R";
            recentProjects_MenuItem.ShortcutKeys = Keys.Alt | Keys.R;
            recentProjects_MenuItem.Size = new Size(180, 22);
            recentProjects_MenuItem.Text = "&Recent";
            // 
            // tabControl
            // 
            tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl.Appearance = TabAppearance.Buttons;
            tabControl.Controls.Add(projectTab);
            tabControl.Controls.Add(aiTab);
            tabControl.Controls.Add(taskListTab);
            tabControl.Location = new Point(9, 24);
            tabControl.Margin = new Padding(0);
            tabControl.Name = "tabControl";
            tabControl.Padding = new Point(5, 3);
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(566, 428);
            tabControl.TabIndex = 0;
            tabControl.SelectedIndexChanged += TabControl_TabIndexChanged;
            // 
            // projectTab
            // 
            projectTab.Controls.Add(projectNameTextBox);
            projectTab.Controls.Add(projectAuthorTextBox);
            projectTab.Controls.Add(projectDescriptionTextBox);
            projectTab.Controls.Add(projectSaveButton);
            projectTab.Location = new Point(4, 27);
            projectTab.Name = "projectTab";
            projectTab.Padding = new Padding(3);
            projectTab.Size = new Size(558, 397);
            projectTab.TabIndex = 2;
            projectTab.Text = "Project";
            projectTab.UseVisualStyleBackColor = true;
            // 
            // projectNameTextBox
            // 
            projectNameTextBox.Location = new Point(8, 6);
            projectNameTextBox.Name = "projectNameTextBox";
            projectNameTextBox.PlaceholderText = "Project Name";
            projectNameTextBox.Size = new Size(544, 23);
            projectNameTextBox.TabIndex = 0;
            // 
            // projectAuthorTextBox
            // 
            projectAuthorTextBox.Location = new Point(8, 35);
            projectAuthorTextBox.Name = "projectAuthorTextBox";
            projectAuthorTextBox.PlaceholderText = "Author";
            projectAuthorTextBox.Size = new Size(544, 23);
            projectAuthorTextBox.TabIndex = 1;
            // 
            // projectDescriptionTextBox
            // 
            projectDescriptionTextBox.Location = new Point(8, 64);
            projectDescriptionTextBox.Multiline = true;
            projectDescriptionTextBox.Name = "projectDescriptionTextBox";
            projectDescriptionTextBox.PlaceholderText = "Description";
            projectDescriptionTextBox.Size = new Size(544, 70);
            projectDescriptionTextBox.TabIndex = 2;
            // 
            // projectSaveButton
            // 
            projectSaveButton.Location = new Point(8, 140);
            projectSaveButton.Name = "projectSaveButton";
            projectSaveButton.Size = new Size(75, 23);
            projectSaveButton.TabIndex = 3;
            projectSaveButton.Text = "Save";
            projectSaveButton.UseVisualStyleBackColor = true;
            projectSaveButton.Click += SaveProject_Button_Click;
            // 
            // aiTab
            // 
            aiTab.Controls.Add(aiResponseTextbox);
            aiTab.Controls.Add(aiResponseLabel);
            aiTab.Controls.Add(aiRequestLabel);
            aiTab.Controls.Add(aiRequestTextbox);
            aiTab.Controls.Add(aiSendButton);
            aiTab.Location = new Point(4, 27);
            aiTab.Name = "aiTab";
            aiTab.Padding = new Padding(3);
            aiTab.Size = new Size(558, 397);
            aiTab.TabIndex = 1;
            aiTab.Text = "Chat";
            aiTab.UseVisualStyleBackColor = true;
            // 
            // aiResponseTextbox
            // 
            aiResponseTextbox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            aiResponseTextbox.Location = new Point(11, 169);
            aiResponseTextbox.Multiline = true;
            aiResponseTextbox.Name = "aiResponseTextbox";
            aiResponseTextbox.PlaceholderText = "Please make a request.";
            aiResponseTextbox.ReadOnly = true;
            aiResponseTextbox.Size = new Size(539, 222);
            aiResponseTextbox.TabIndex = 3;
            // 
            // aiResponseLabel
            // 
            aiResponseLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            aiResponseLabel.AutoSize = true;
            aiResponseLabel.Location = new Point(493, 148);
            aiResponseLabel.Name = "aiResponseLabel";
            aiResponseLabel.Size = new Size(57, 15);
            aiResponseLabel.TabIndex = 2;
            aiResponseLabel.Text = "Response";
            // 
            // aiRequestLabel
            // 
            aiRequestLabel.AutoSize = true;
            aiRequestLabel.Location = new Point(6, 6);
            aiRequestLabel.Name = "aiRequestLabel";
            aiRequestLabel.Size = new Size(49, 15);
            aiRequestLabel.TabIndex = 1;
            aiRequestLabel.Text = "Request";
            // 
            // aiRequestTextbox
            // 
            aiRequestTextbox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            aiRequestTextbox.Location = new Point(6, 24);
            aiRequestTextbox.Multiline = true;
            aiRequestTextbox.Name = "aiRequestTextbox";
            aiRequestTextbox.Size = new Size(544, 106);
            aiRequestTextbox.TabIndex = 0;
            // 
            // aiSendButton
            // 
            aiSendButton.BackColor = Color.FromArgb(192, 255, 192);
            aiSendButton.Location = new Point(6, 133);
            aiSendButton.Name = "aiSendButton";
            aiSendButton.Size = new Size(100, 30);
            aiSendButton.TabIndex = 4;
            aiSendButton.Text = "Send";
            aiSendButton.UseVisualStyleBackColor = false;
            aiSendButton.Click += aiSendButton_Click;
            // 
            // taskListTab
            // 
            taskListTab.Controls.Add(taskTextBox);
            taskListTab.Controls.Add(taskAddButton);
            taskListTab.Controls.Add(taskEditButton);
            taskListTab.Controls.Add(tasksListBox);
            taskListTab.Controls.Add(taskRemoveButton);
            taskListTab.Controls.Add(taskCompleteButton);
            taskListTab.Location = new Point(4, 27);
            taskListTab.Name = "taskListTab";
            taskListTab.Padding = new Padding(3);
            taskListTab.Size = new Size(558, 397);
            taskListTab.TabIndex = 0;
            taskListTab.Text = "Task List";
            taskListTab.UseVisualStyleBackColor = true;
            // 
            // taskTextBox
            // 
            taskTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            taskTextBox.Location = new Point(6, 6);
            taskTextBox.Name = "taskTextBox";
            taskTextBox.Size = new Size(468, 23);
            taskTextBox.TabIndex = 0;
            // 
            // taskAddButton
            // 
            taskAddButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            taskAddButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            taskAddButton.Location = new Point(480, 7);
            taskAddButton.Name = "taskAddButton";
            taskAddButton.Size = new Size(72, 22);
            taskAddButton.TabIndex = 1;
            taskAddButton.Text = "Add";
            taskAddButton.UseVisualStyleBackColor = true;
            taskAddButton.Click += taskAddButton_Click;
            // 
            // taskEditButton
            // 
            taskEditButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            taskEditButton.Location = new Point(480, 35);
            taskEditButton.Name = "taskEditButton";
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
            tasksListBox.Location = new Point(6, 35);
            tasksListBox.Name = "tasksListBox";
            tasksListBox.Size = new Size(468, 349);
            tasksListBox.TabIndex = 2;
            // 
            // taskRemoveButton
            // 
            taskRemoveButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            taskRemoveButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            taskRemoveButton.Location = new Point(480, 64);
            taskRemoveButton.Name = "taskRemoveButton";
            taskRemoveButton.Size = new Size(72, 22);
            taskRemoveButton.TabIndex = 3;
            taskRemoveButton.Text = "Remove";
            taskRemoveButton.UseVisualStyleBackColor = true;
            taskRemoveButton.Click += taskRemoveButton_Click;
            // 
            // taskCompleteButton
            // 
            taskCompleteButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            taskCompleteButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            taskCompleteButton.Location = new Point(480, 92);
            taskCompleteButton.Name = "taskCompleteButton";
            taskCompleteButton.Size = new Size(72, 22);
            taskCompleteButton.TabIndex = 4;
            taskCompleteButton.Text = "Complete";
            taskCompleteButton.UseVisualStyleBackColor = true;
            taskCompleteButton.Click += taskCompleteButton_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(584, 461);
            Controls.Add(menuStrip);
            Controls.Add(tabControl);
            MainMenuStrip = menuStrip;
            Name = "MainForm";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Helper App (No Project Loaded)";
            Load += MainForm_Load;
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            tabControl.ResumeLayout(false);
            projectTab.ResumeLayout(false);
            projectTab.PerformLayout();
            aiTab.ResumeLayout(false);
            aiTab.PerformLayout();
            taskListTab.ResumeLayout(false);
            taskListTab.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
