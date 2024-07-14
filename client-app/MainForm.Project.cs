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
        private void OnProjectTabSelected()
        {
            projectAuthorTextBox.Text = currentProject.Author;
            projectDescriptionTextBox.Text = currentProject.Description;
            projectNameTextBox.Text = currentProject.DisplayName;
        }

        private void SaveProjectButton_Click(object sender, EventArgs e)
        {
            if (currentProject == null)
                return;

            currentProject.Author = projectAuthorTextBox.Text;
            currentProject.Description = projectDescriptionTextBox.Text;
            currentProject.DisplayName = projectNameTextBox.Text;

            _projectService.SetCurrentProject(currentProject).Wait();
            if (!_projectService.SaveProjectAsync().Result)
            {
                Debug.WriteLine($"Failed to save project [{projectNameTextBox.Text}]");
            }
        }
    }
}
