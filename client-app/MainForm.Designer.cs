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
            tabControl.Location = new Point(0, 0);
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
            Text = "Task List and Chat";
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
