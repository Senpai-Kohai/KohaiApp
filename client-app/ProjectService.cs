using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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

        public async Task<ProjectData?> CreateProjectAsync(string? name = null, string? author = null, string? description = null)
        {
            await Task.CompletedTask;

            ProjectData newProject = new ProjectData()
            {
                DisplayName = name,
                Author = author,
                Description = description
            };

            _currentProject = newProject;
            AddToRecentProjects(newProject);

            return newProject;
        }

        public async Task<bool> UpdateProjectAsync(ProjectData projectData)
        {
            if (_currentProject?.ID != projectData.ID)
                return false;

            _currentProject.DisplayName = projectData.DisplayName;
            _currentProject.Author = projectData.Author;
            _currentProject.Description = projectData.Description;

            return await SaveProjectAsync();
        }

        public async Task<ProjectData?> GetCurrentProject()
        {
            await Task.CompletedTask;

            return _currentProject;
        }

        public async Task<bool> SaveProjectAsync()
        {
            try
            {
                if (_currentProject == null)
                    return false;

                string projectPath = GetProjectPath(_currentProject.ID);
                if (!Directory.Exists(projectPath))
                    Directory.CreateDirectory(projectPath);

                string projectFilePath = Path.Combine(projectPath, "project.json");
                string json = JsonConvert.SerializeObject(_currentProject, Formatting.Indented);
                await File.WriteAllTextAsync(projectFilePath, json).ConfigureAwait(false);
                AddToRecentProjects(_currentProject);

                return true;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to save project [{_currentProject?.DisplayName}] with ID [{_currentProject?.ID}]. Message: {exc}");
                return false;
            }
        }

        public async Task<ProjectData?> LoadProjectAsync(Guid ID)
        {
            try
            {
                string projectPath = GetProjectPath(ID);
                if (!Directory.Exists(projectPath))
                    return null;

                string projectFilePath = Path.Combine(projectPath, "project.json");
                if (!File.Exists(projectFilePath))
                    return null;

                string json = await File.ReadAllTextAsync(projectFilePath).ConfigureAwait(false);
                ProjectData? project = JsonConvert.DeserializeObject<ProjectData>(json);

                if (project == null)
                    return null;

                _currentProject = project;
                AddToRecentProjects(project);

                return project;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to load project with ID [{ID}]. Message: {exc}");
            }

            return null;
        }

        public async Task<ProjectData?> LoadProjectAsync(string name)
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
                        string json = await File.ReadAllTextAsync(projectFilePath).ConfigureAwait(false);
                        ProjectData? project = JsonConvert.DeserializeObject<ProjectData>(json);
                        if (project?.DisplayName == null)
                            return null;

                        if (project.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            _currentProject = project;
                            AddToRecentProjects(project);

                            return project;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to load project with name [{name}]. Message: {exc}");
            }

            return null;
        }

        private string GetProjectPath(Guid ID)
        {
            return Path.Combine(_config.ProjectsDirectory, ID.ToString());
        }

        private void AddToRecentProjects(ProjectData project)
        {
            try
            {
                // remove and add back to the top of the list
                // so that the list is in last-load order
                _recentProjects.RemoveAll(_ => _.ID == project.ID);
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
                _recentProjects.Clear();

                if (File.Exists(_config.RecentProjectsFilename))
                {
                    string recentIDsString = File.ReadAllTextAsync(_config.RecentProjectsFilename).Result;
                    var recentIDs = JsonConvert.DeserializeObject<List<string>>(recentIDsString);
                    if (recentIDs != null)
                    {
                        foreach (var recentID in recentIDs)
                        {
                            if (Guid.TryParse(recentID, out Guid projectID))
                            {
                                ProjectData? recentProject = LoadProjectAsync(projectID).Result;
                                if (recentProject != null)
                                {
                                    _recentProjects.RemoveAll(_ => _.ID == projectID);
                                    _recentProjects.Add(recentProject);

                                    if (_recentProjects.Count == 10)
                                        break;
                                }
                            }
                        }
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
                var recentProjectIDs = _recentProjects.Select(p => p.ID.ToString()).ToList();

                string json = JsonConvert.SerializeObject(recentProjectIDs, Formatting.Indented);
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
                await File.WriteAllTextAsync(filePath, data).ConfigureAwait(false);

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
    }
}
