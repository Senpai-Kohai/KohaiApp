using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.Http;
using System.Text;

namespace client_app
{
    public partial class MainForm : Form
    {
        private ProjectService _projectService;
        private readonly AIService _aiService;
        private AppConfiguration _config;
        private Dictionary<TabPage, Action> _tabSelectedActions = new Dictionary<TabPage, Action>();

        public MainForm(ProjectService projectService, AIService chatGPTService, AppConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _aiService = chatGPTService ?? throw new ArgumentNullException(nameof(chatGPTService));

            InitializeComponents();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadCurrentProject();
            LoadTasks();
        }

        private void LoadCurrentProject()
        {
            if (currentProject == null)
            {
                currentProject = _projectService.GetCurrentProject().Result;
                if (currentProject == null)
                {
                    currentProject = new ProjectData(Guid.NewGuid());
                    currentProject.DisplayName = "Initial Project";
                    currentProject.Author = "Batman";

                    _projectService.SetCurrentProject(currentProject).Wait();
                    _projectService.SaveProjectAsync().Wait();
                }
            }

            if (tabControl.SelectedTab == projectTab)
            {
                projectAuthorTextBox.Text = currentProject.Author;
                projectNameTextBox.Text = currentProject.DisplayName;
                projectDescriptionTextBox.Text = currentProject.Description;
            }
        }

        private void TabControl_TabIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex != -1)
            {
                TabPage selectedTab = tabControl.TabPages[tabControl.SelectedIndex];
                string tabName = selectedTab.Name;

                foreach (var tabPage in _tabSelectedActions)
                {
                    if (string.Equals(tabPage.Key.Name, tabName, StringComparison.OrdinalIgnoreCase))
                    {
                        tabPage.Value?.Invoke();
                        break;
                    }
                }
            }
        }

        private void CreateNewMenuItem_Click(object sender, EventArgs e)
        {
            currentProject = new ProjectData(Guid.NewGuid());
            _projectService.SetCurrentProject(currentProject).Wait();
            _ = _projectService.SaveProjectAsync().Result;

            if (tabControl.SelectedTab == projectTab)
            {
                projectAuthorTextBox.Text = currentProject?.Author;
                projectNameTextBox.Text = currentProject?.DisplayName;
                projectDescriptionTextBox.Text = currentProject?.Description;
            }
            else
            {
                tabControl.SelectedTab = projectTab;
            }
        }

        private void EditMenuItem_Click(object sender, EventArgs e)
        {
            tabControl.SelectedTab = projectTab;
        }

        private void LoadMenuItem_Click(object sender, EventArgs e)
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
                        if (loadedProject != null)
                        {
                            currentProject = loadedProject;
                            _projectService.SetCurrentProject(currentProject).Wait();
                        }

                        if (tabControl.SelectedTab == projectTab)
                        {
                            projectAuthorTextBox.Text = currentProject?.Author;
                            projectNameTextBox.Text = currentProject?.DisplayName;
                            projectDescriptionTextBox.Text = currentProject?.Description;
                        }
                        else
                        {
                            tabControl.SelectedTab = projectTab;
                        }
                    }
                }
            }
        }

    }
}
