using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using client_app.Models;

namespace client_app
{
    public partial class EditTaskForm : Form
    {
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }

        public EditTaskForm(string taskDescription, bool isCompleted)
        {
            InitializeComponent();

            taskDescriptionTextBox.Text = TaskDescription = taskDescription;
            completedCheckbox.Checked = IsCompleted = isCompleted;
        }

        private async void TaskWindow_okButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(TaskWindow_okButton_Click)}");
            await Task.CompletedTask;

            TaskDescription = taskDescriptionTextBox.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private async void TaskWindow_completedCheckbox_Updated(object sender, EventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(TaskWindow_completedCheckbox_Updated)}");
            await Task.CompletedTask;

            IsCompleted = completedCheckbox.Checked;
        }

        private async void TaskWindow_cancelButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(TaskWindow_cancelButton_Click)}");
            await Task.CompletedTask;

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
