using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace client_app
{
    public partial class MainForm
    {
        private List<TaskItem> _tasks = new List<TaskItem>();

        private async void OnTaskListTabSelected()
        {
            await Task.CompletedTask;
        }

        private async void taskAddButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(taskAddButton_Click)}");

            string taskDescription = taskTextBox.Text;
            if (!string.IsNullOrWhiteSpace(taskDescription))
            {
                var task = new TaskItem { Description = taskDescription, IsCompleted = false };
                _tasks.Add(task);
                tasksListBox.Items.Add(task.Description);
                taskTextBox.Clear();
                await SaveTasks();
            }
            else
            {
                MessageBox.Show("Please enter a task.");
            }
        }

        private async void taskEditButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(taskEditButton_Click)}");

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
                        await SaveTasks();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a task to edit.");
            }
        }

        private async void taskRemoveButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(taskRemoveButton_Click)}");

            if (tasksListBox.SelectedIndex != -1)
            {
                _tasks.RemoveAt(tasksListBox.SelectedIndex);
                tasksListBox.Items.RemoveAt(tasksListBox.SelectedIndex);
                await SaveTasks();
            }
            else
            {
                MessageBox.Show("Please select a task to remove.");
            }
        }

        private async void taskCompleteButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(taskCompleteButton_Click)}");
            await Task.CompletedTask;

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

                    await SaveTasks();
                }
            }
            else
            {
                MessageBox.Show("Please select a task to mark as completed.");
            }
        }

        private async Task SaveTasks()
        {
            Debug.WriteLine($"Method: {nameof(SaveTasks)}");

            var json = JsonConvert.SerializeObject(_tasks, Formatting.Indented);
            await File.WriteAllTextAsync(_config.TasksFilename, json).ConfigureAwait(false);
        }

        private async Task LoadTasks()
        {
            Debug.WriteLine($"Method: {nameof(LoadTasks)}");

            if (File.Exists(_config.TasksFilename))
            {
                var json = await File.ReadAllTextAsync(_config.TasksFilename).ConfigureAwait(false);
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
