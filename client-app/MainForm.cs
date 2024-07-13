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
            LoadTasks();
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
    }
}
