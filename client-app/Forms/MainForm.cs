using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Kohai;
using Kohai.Models;
using Kohai.Services;
using Kohai.Configuration;

namespace Kohai.Client
{
    public partial class MainForm : Form
    {
        private readonly ProjectService _projectService;
        private readonly AIService _aiService;
        private readonly Dictionary<TabPage, Action> _tabSelectedActions = [];

        public MainForm(ProjectService projectService, AIService aiService)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));

            InitializeComponent();

            _tabSelectedActions[projectTab] = OnProjectTabSelected;
            _tabSelectedActions[taskListTab] = OnTaskListTabSelected;
            _tabSelectedActions[aiTab] = OnAITabSelected;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(MainForm_Load)}");

            ReloadProjectData();
            PopulateRecentProjectsMenu();
            await LoadTasks();

            _projectService.OnCurrentProjectChanged += OnCurrentProjectChanged;
        }

        private void ReloadProjectData()
        {
            Debug.WriteLine($"Method: {nameof(ReloadProjectData)}");

            var currentProject = _projectService.GetCurrentProject();
            var projectName = currentProject?.DisplayName ?? currentProject?.ID.ToString() ?? ("No project loaded");

            Text = $"Helper App ({projectName})";
            projectAuthorTextBox.Text = currentProject?.Author;
            projectNameTextBox.Text = currentProject?.DisplayName;
            projectDescriptionTextBox.Text = currentProject?.Description;

            var projectLoaded = currentProject != null;
            editProject_MenuItem.Enabled = projectLoaded;
            editProject_MenuItem.Visible = projectLoaded;
            tabControl.Enabled = projectLoaded;
            tabControl.Visible = projectLoaded;
        }

        private async void TabControl_TabIndexChanged(object sender, EventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(TabControl_TabIndexChanged)}");
            await Task.CompletedTask;

            if (tabControl.SelectedIndex != -1)
            {
                var selectedTab = tabControl.TabPages[tabControl.SelectedIndex];
                var tabName = selectedTab.Name;

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
    }
}
