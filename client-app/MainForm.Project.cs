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

        private void PopulateRecentProjectsMenu()
        {
            recentProjects_MenuItem.DropDownItems.Clear();

            foreach (var project in _projectService.GetRecentProjects())
            {
                var projectMenuItem = new ToolStripMenuItem
                {
                    Text = !string.IsNullOrEmpty(project.DisplayName) ? project.DisplayName : project.ID.ToString(),
                    Tag = project.ID
                };

                projectMenuItem.Click += (sender, e) =>
                {
                    Guid? projectID = (Guid?)((ToolStripMenuItem?)sender)?.Tag;
                    if (projectID == null)
                        return;

                    ProjectData? loadedProject = _projectService.LoadProjectAsync(projectID.Value).Result;
                    if (loadedProject == null)
                        return;

                    projectAuthorTextBox.Text = loadedProject?.Author;
                    projectNameTextBox.Text = loadedProject?.DisplayName;
                    projectDescriptionTextBox.Text = loadedProject?.Description;

                    tabControl.SelectTab(projectTab);
                };

                recentProjects_MenuItem.DropDownItems.Add(projectMenuItem);
            }
        }

        private void LoadCurrentProject()
        {
            ProjectData? currentProject = _projectService.GetCurrentProject().Result;
            if (currentProject == null)
                return;

            projectAuthorTextBox.Text = currentProject?.Author;
            projectNameTextBox.Text = currentProject?.DisplayName;
            projectDescriptionTextBox.Text = currentProject?.Description;
        }

        private void OnProjectTabSelected()
        {
            LoadCurrentProject();
        }

        private void SaveProject_Button_Click(object sender, EventArgs e)
        {
            ProjectData? currentProject = _projectService.GetCurrentProject().Result;
            if (currentProject == null)
                return;

            currentProject.Author = projectAuthorTextBox.Text;
            currentProject.Description = projectDescriptionTextBox.Text;
            currentProject.DisplayName = projectNameTextBox.Text;

            _projectService.UpdateProjectAsync(currentProject).Wait();
            if (!_projectService.SaveProjectAsync().Result)
            {
                Debug.WriteLine($"Failed to save project [{projectNameTextBox.Text}]");
            }
        }

        private void CreateProject_MenuItem_Click(object sender, EventArgs e)
        {
            ProjectData? newProject = _projectService.CreateProjectAsync().Result;
            if (newProject == null)
                return;

            tabControl.SelectedTab = projectTab;
        }

        private void EditProject_MenuItem_Click(object sender, EventArgs e)
        {
            tabControl.SelectedTab = projectTab;
        }

        private void LoadProject_MenuItem_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = _config.ProjectsDirectory;
                openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string path = openFileDialog.FileName;
                    string parentDirectoryName = new DirectoryInfo(Path.GetDirectoryName(path) ?? "").Name;

                    if (Guid.TryParse(parentDirectoryName, out Guid projectID))
                    {
                        ProjectData? loadedProject = _projectService.LoadProjectAsync(projectID).Result;
                        if (loadedProject == null)
                            return;

                        tabControl.SelectedTab = projectTab;
                    }
                }
            }
        }
    }
}
