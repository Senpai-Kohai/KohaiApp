using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using client_app.Models;

namespace client_app
{
    public partial class MainForm
    {
        private void PopulateRecentProjectsMenu()
        {
            Debug.WriteLine($"Method: {nameof(PopulateRecentProjectsMenu)}");

            recentProjects_MenuItem.DropDownItems.Clear();

            foreach (var project in _projectService.GetRecentProjects())
            {
                var projectMenuItem = new ToolStripMenuItem
                {
                    Text = !string.IsNullOrEmpty(project.DisplayName) ? project.DisplayName : project.ID.ToString(),
                    Tag = project.ID
                };

                projectMenuItem.Click += async (sender, e) =>
                {
                    var projectID = (Guid?)((ToolStripMenuItem?)sender)?.Tag;
                    if (projectID == null)
                        return;

                    _ = await _projectService.LoadProjectAsync(projectID.Value);
                };

                recentProjects_MenuItem.DropDownItems.Add(projectMenuItem);
            }
        }

        private async void OnProjectTabSelected()
        {
            await Task.CompletedTask;

            ReloadProjectData();
        }

        private async void SaveProject_Button_Click(object sender, EventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(SaveProject_Button_Click)}");

            var currentProject = _projectService.GetCurrentProject();
            if (currentProject == null)
                return;

            currentProject.Author = projectAuthorTextBox.Text;
            currentProject.Description = projectDescriptionTextBox.Text;
            currentProject.DisplayName = projectNameTextBox.Text;

            if (!await _projectService.UpdateProjectAsync(currentProject))
            {
                Debug.WriteLine($"Failed to update project for [{currentProject.DisplayName ?? currentProject.ID.ToString()}]");
                return;
            }

            ReloadProjectData();
        }

        private async void CreateProject_MenuItem_Click(object sender, EventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(CreateProject_MenuItem_Click)}");

            if (await _projectService.CreateProjectAsync() == null)
            {
                Debug.WriteLine($"Failed to create new project.");
            }
        }

        private async void EditProject_MenuItem_Click(object sender, EventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(EditProject_MenuItem_Click)}");
            await Task.CompletedTask;

            tabControl.SelectedTab = projectTab;
        }

        private async void LoadProject_MenuItem_Click(object sender, EventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(LoadProject_MenuItem_Click)}");

            using var openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = _config.ProjectsDirectory;
            openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var path = openFileDialog.FileName;
                var parentDirectoryName = new DirectoryInfo(Path.GetDirectoryName(path) ?? "").Name;

                if (Guid.TryParse(parentDirectoryName, out var projectID))
                    if (await _projectService.LoadProjectAsync(projectID) == null)
                        Debug.WriteLine($"Failed to load project [{openFileDialog.FileName}]");
            }
        }

        private async void OnCurrentProjectChanged(ProjectData? newProject)
        {
            Debug.WriteLine($"Method: {nameof(OnCurrentProjectChanged)}");
            await Task.CompletedTask;

            if (InvokeRequired)
            {
                // If the method is called from a different thread, invoke it on the UI thread
                Invoke(new Action<ProjectData?>(OnCurrentProjectChanged), newProject);
            }
            else
            {
                if (tabControl.SelectedTab != projectTab)
                    tabControl.SelectTab(projectTab);
                else
                    ReloadProjectData();

                PopulateRecentProjectsMenu();
            }
        }
    }
}
