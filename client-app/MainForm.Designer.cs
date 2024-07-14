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
        private MenuStrip menuStrip;
        private ToolStripMenuItem projectsMenu;
        private ToolStripMenuItem createProject_MenuItem;
        private ToolStripMenuItem editProject_MenuItem;
        private ToolStripMenuItem loadProject_MenuItem;
        private ToolStripMenuItem recentProjects_MenuItem;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponents()
        {
            // file menu initialization
            menuStrip = new MenuStrip();
            projectsMenu = new ToolStripMenuItem();
            createProject_MenuItem = new ToolStripMenuItem();
            editProject_MenuItem = new ToolStripMenuItem();
            loadProject_MenuItem = new ToolStripMenuItem();
            recentProjects_MenuItem = new ToolStripMenuItem();

            menuStrip.Items.AddRange(new ToolStripItem[] {
                projectsMenu,
            });

            projectsMenu.DropDownItems.AddRange(new ToolStripItem[] {
                createProject_MenuItem,
                editProject_MenuItem,
                loadProject_MenuItem,
                recentProjects_MenuItem,
            });

            projectsMenu.Text = "Projects";
            createProject_MenuItem.Text = "Create";
            editProject_MenuItem.Text = "Edit";
            loadProject_MenuItem.Text = "Load";
            recentProjects_MenuItem.Text = "Recent";

            createProject_MenuItem.Click += new EventHandler(CreateProject_MenuItem_Click);
            editProject_MenuItem.Click += new EventHandler(EditProject_MenuItem_Click);
            loadProject_MenuItem.Click += new EventHandler(LoadProject_MenuItem_Click);

            menuStrip.Location = new System.Drawing.Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new System.Drawing.Size(800, 24);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "menuStrip";

            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;

            // Main form initialization code
            tabControl = new TabControl();
            projectTab = new TabPage();
            taskListTab = new TabPage();
            aiTab = new TabPage();
            tabControl.SuspendLayout();
            SuspendLayout();

            // Initialize individual tabs
            InitializeProjectComponents();
            InitializeTaskListComponents();
            InitializeAIComponents();

            // 
            // tabControl
            // 
            tabControl.Controls.Add(projectTab);
            tabControl.Controls.Add(aiTab);
            tabControl.Controls.Add(taskListTab);
            tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl.Location = new Point(0, 30);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(366, 422);
            tabControl.TabIndex = 0;
            tabControl.SelectedIndexChanged += TabControl_TabIndexChanged;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(366, 422);
            Controls.Add(tabControl);
            Name = "MainForm";
            Text = "Helper App (No Project Loaded)";
            Load += MainForm_Load;
            tabControl.ResumeLayout(false);
            projectTab.ResumeLayout(false);
            projectTab.PerformLayout();
            taskListTab.ResumeLayout(false);
            taskListTab.PerformLayout();
            aiTab.ResumeLayout(false);
            aiTab.PerformLayout();
            ResumeLayout(false);
        }
    }
}
