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

        public MainForm(ProjectService projectService, AIService aiService, AppConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));

            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(MainForm_Load)}");

            ReloadProjectData();
            PopulateRecentProjectsMenu();
            LoadTasks();

            _projectService.OnCurrentProjectChanged += OnCurrentProjectChanged;
        }

        private void ReloadProjectData()
        {
            Debug.WriteLine($"Method: {nameof(ReloadProjectData)}");

            ProjectData? currentProject = _projectService.GetCurrentProject();
            string projectName = currentProject?.DisplayName ?? currentProject?.ID.ToString() ?? ("No project loaded");

            Text = $"Helper App ({projectName})";
            projectAuthorTextBox.Text = currentProject?.Author;
            projectNameTextBox.Text = currentProject?.DisplayName;
            projectDescriptionTextBox.Text = currentProject?.Description;

            bool projectLoaded = currentProject != null;
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
    }
}
