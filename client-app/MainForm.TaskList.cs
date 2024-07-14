using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace client_app
{
    public partial class MainForm
    {
        private List<TaskItem> _tasks = new List<TaskItem>();

        private void OnTaskListTabSelected()
        {
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            string taskDescription = taskTextBox.Text;
            if (!string.IsNullOrWhiteSpace(taskDescription))
            {
                var task = new TaskItem { Description = taskDescription, IsCompleted = false };
                _tasks.Add(task);
                tasksListBox.Items.Add(task.Description);
                taskTextBox.Clear();
                SaveTasks();
            }
            else
            {
                MessageBox.Show("Please enter a task.");
            }
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            if (tasksListBox.SelectedIndex != -1)
            {
                int selectedIndex = tasksListBox.SelectedIndex;
                var task = _tasks[selectedIndex];

                using (var editTaskForm = new EditTaskForm(task.Description ?? "", task.IsCompleted))
                {
                    if (editTaskForm.ShowDialog() == DialogResult.OK)
                    {
                        task.Description = editTaskForm.TaskDescription;
                        task.IsCompleted = editTaskForm.IsCompleted;
                        tasksListBox.Items[selectedIndex] = task.Description + (task.IsCompleted ? " (Completed)" : string.Empty);
                        SaveTasks();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a task to edit.");
            }
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            if (tasksListBox.SelectedIndex != -1)
            {
                _tasks.RemoveAt(tasksListBox.SelectedIndex);
                tasksListBox.Items.RemoveAt(tasksListBox.SelectedIndex);
                SaveTasks();
            }
            else
            {
                MessageBox.Show("Please select a task to remove.");
            }
        }

        private void completeButton_Click(object sender, EventArgs e)
        {
            if (tasksListBox.SelectedIndex != -1)
            {
                var task = _tasks[tasksListBox.SelectedIndex];
                task.IsCompleted = !task.IsCompleted;

                if (task.Description != null)
                {
                    if (task.IsCompleted)
                        tasksListBox.Items[tasksListBox.SelectedIndex] = task.Description + " (Completed)";
                    else
                        tasksListBox.Items[tasksListBox.SelectedIndex] = task.Description;

                    SaveTasks();
                }
            }
            else
            {
                MessageBox.Show("Please select a task to mark as completed.");
            }
        }

        private void SaveTasks()
        {
            var json = JsonConvert.SerializeObject(_tasks, Formatting.Indented);
            File.WriteAllText(_config.TasksFilename, json);
        }

        private void LoadTasks()
        {
            if (File.Exists(_config.TasksFilename))
            {
                var json = File.ReadAllText(_config.TasksFilename);
                _tasks = JsonConvert.DeserializeObject<List<TaskItem>>(json) ?? new List<TaskItem>();
                foreach (var task in _tasks)
                {
                    tasksListBox.Items.Add(task.Description + (task.IsCompleted ? " (Completed)" : string.Empty));
                }
            }
            else
            {
                _tasks = new List<TaskItem>();
            }
        }
    }
}
