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
            currentProject = _projectService.GetCurrentProject().Result;

            if (currentProject == null)
                currentProject = new ProjectData(Guid.NewGuid());
        }

        private void SaveProjectButton_Click(object sender, EventArgs e)
        {
            if (!_projectService.SaveProjectAsync(currentProject).Result)
            {
                Debug.WriteLine($"Failed to save project [{projectNameTextBox.Text}]");
            }
        }
    }
}
