using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using client_app;
using Newtonsoft.Json;

namespace client_app
{
    public partial class MainForm
    {
        private TabPage projectTab;
        private TextBox projectNameTextBox;
        private TextBox projectAuthorTextBox;
        private TextBox projectDescriptionTextBox;
        private Button saveProjectButton;
        private ProjectData currentProject;

        private void InitializeProjectComponents()
        {
            _tabSelectedActions[projectTab] = OnProjectTabSelected;

            projectNameTextBox = new TextBox();
            projectAuthorTextBox = new TextBox();
            projectDescriptionTextBox = new TextBox();
            saveProjectButton = new Button();

            // Project Tab
            projectTab.Controls.Add(projectNameTextBox);
            projectTab.Controls.Add(projectAuthorTextBox);
            projectTab.Controls.Add(projectDescriptionTextBox);
            projectTab.Controls.Add(saveProjectButton);
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
            saveProjectButton.Location = new System.Drawing.Point(20, 140);
            saveProjectButton.Name = "saveProjectButton";
            saveProjectButton.Size = new System.Drawing.Size(75, 23);
            saveProjectButton.TabIndex = 3;
            saveProjectButton.Text = "Save";
            saveProjectButton.UseVisualStyleBackColor = true;
            saveProjectButton.Click += new EventHandler(SaveProjectButton_Click);
        }
    }
}
