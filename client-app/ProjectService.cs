using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace client_app
{
    public class ProjectService
    {
        private readonly AppConfiguration _config;
        private readonly List<ProjectData> _recentProjects = new();
        private ProjectData? _currentProject = null;

        public ProjectService(AppConfiguration config)
        {
            _config = config;

            if (!Directory.Exists(config.ProjectsDirectory))
                Directory.CreateDirectory(config.ProjectsDirectory);

            LoadRecentProjects();

            // load last project, if configured to do so
            if (config.LoadLastProjectOnStartup && _recentProjects.Count > 0)
                _currentProject = _recentProjects[0];
        }

        public async Task<ProjectData?> GetCurrentProject()
        {
            await Task.CompletedTask;

            return _currentProject;
        }

        public async Task<bool> SaveProjectAsync(ProjectData project)
        {
            try
            {
                string projectPath = GetProjectPath(project.ID);
                if (!Directory.Exists(projectPath))
                    Directory.CreateDirectory(projectPath);

                string projectFilePath = Path.Combine(projectPath, "project.json");
                string json = JsonSerializer.Serialize(project);
                await File.WriteAllTextAsync(projectFilePath, json);

                AddToRecentProjects(project);

                return true;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to save project [{project.DisplayName}] with ID [{project.ID}]. Message: {exc}");
                return false;
            }
        }

        public async Task<ProjectData?> LoadProjectAsync(Guid projectID)
        {
            try
            {
                string projectPath = GetProjectPath(projectID);
                if (!Directory.Exists(projectPath))
                    return null;

                string projectFilePath = Path.Combine(projectPath, "project.json");
                if (!File.Exists(projectFilePath))
                    return null;

                string json = await File.ReadAllTextAsync(projectFilePath);
                ProjectData? project = JsonSerializer.Deserialize<ProjectData>(json);

                if (project == null)
                    return null;

                AddToRecentProjects(project);

                return project;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to load project with ID [{projectID}]. Message: {exc}");
                return null;
            }
        }

        public async Task<ProjectData?> LoadProjectAsync(string projectName)
        {
            try
            {
                // potentially slow-
                // iterates all projects for one with a matching name, and loads if found
                foreach (var directory in Directory.GetDirectories(_config.ProjectsDirectory))
                {
                    string projectFilePath = Path.Combine(directory, "project.json");
                    if (File.Exists(projectFilePath))
                    {
                        string json = await File.ReadAllTextAsync(projectFilePath);
                        ProjectData? project = JsonSerializer.Deserialize<ProjectData>(json);
                        if (project?.DisplayName == null)
                            return null;

                        if (project.DisplayName.Equals(projectName, StringComparison.OrdinalIgnoreCase))
                        {
                            AddToRecentProjects(project);
                            return project;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to load project with name [{projectName}]. Message: {exc}");
            }

            return null;
        }

        public async Task<bool> SaveDataAsync(string key, string data)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return false;

                if (_currentProject == null)
                    return false;

                string projectPath = GetProjectPath(_currentProject.ID);
                string filePath = Path.Combine(projectPath, $"{key}.json");
                await File.WriteAllTextAsync(filePath, data);

                return true;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to save data for project [{_currentProject?.ID}] at key [{key}]. Message: {exc}");
                return false;
            }
        }

        public async Task<string?> LoadDataAsync(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    return null;

                if (_currentProject == null)
                    return null;

                string filePath = Path.Combine(GetProjectPath(_currentProject.ID), $"{key}.json");

                return await File.ReadAllTextAsync(filePath);
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to load data for project [{_currentProject?.ID}] at key [{key}]. Message: {exc}");
                return null;
            }
        }

        public async Task<bool> DeleteDataAsync(string key)
        {
            try
            {
                await Task.CompletedTask;

                if (string.IsNullOrWhiteSpace(key))
                    return false;

                if (_currentProject == null)
                    return false;

                string filePath = Path.Combine(GetProjectPath(_currentProject.ID), $"{key}.json");
                File.Delete(filePath);

                return true;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to delete data for project [{_currentProject?.ID}] at key [{key}]. Message: {exc}");
                return false;
            }
        }

        private string GetProjectPath(Guid projectID)
        {
            return Path.Combine(_config.ProjectsDirectory, projectID.ToString());
        }

        private void AddToRecentProjects(ProjectData project)
        {
            try
            {
                // remove and add back to the top of the list
                // so that the list is in last-load order
                _recentProjects.RemoveAll(p => p.ID == project.ID);
                _recentProjects.Insert(0, project);

                if (_recentProjects.Count > 10)
                    _recentProjects.RemoveAt(10);

                SaveRecentProjects();
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to get recent projects. Message: {exc}");
            }
        }

        private void LoadRecentProjects()
        {
            try
            {
                if (File.Exists(_config.RecentProjectsFilename))
                {
                    string json = File.ReadAllText(_config.RecentProjectsFilename);
                    var recentProjects = JsonSerializer.Deserialize<List<ProjectData>>(json);
                    if (recentProjects != null)
                    {
                        _recentProjects.AddRange(recentProjects);
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to load recent projects. Message: {exc}");
            }
        }

        private void SaveRecentProjects()
        {
            try
            {
                string json = JsonSerializer.Serialize(_recentProjects);
                File.WriteAllText(_config.RecentProjectsFilename, json);
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to save recent projects. Message: {exc}");
            }
        }

        public IReadOnlyList<ProjectData> GetRecentProjects()
        {
            return _recentProjects.AsReadOnly();
        }
    }
}
