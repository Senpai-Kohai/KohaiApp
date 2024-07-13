using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        private void okButton_Click(object sender, EventArgs e)
        {
            TaskDescription = taskDescriptionTextBox.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void completedCheckbox_Updated(object sender, EventArgs e)
        {
            IsCompleted = completedCheckbox.Checked;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
