using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Kohai;
using Kohai.Configuration;
using Kohai.Models;
using OpenAI;
using System.Net.Http;

namespace Kohai.Services
{
    public class ProjectService : ServiceBase<ProjectServiceConfiguration>
    {
        public override bool ServiceRunning => base.ServiceRunning;

        public event Action<ProjectData?>? OnCurrentProjectChanged;
        private readonly List<ProjectData> _recentProjects = [];

        private ProjectData? _currentProject = null;
        public ProjectData? CurrentProject
        {
            get { return _currentProject; }
            private set
            {
                if (_currentProject == value)
                    return;

                _currentProject = value;

                if (value != null)
                    AddToRecentProjects(value);

                OnCurrentProjectChanged?.Invoke(value);
            }
        }

        // expose configuration (readonly)
        public string? ProjectsDirectory => ServiceConfiguration.ProjectsDirectory;
        public bool LoadLastProjectAtStartup => ServiceConfiguration.LoadLastProjectOnStartup;
        public string? RecentProjectsFilename => ServiceConfiguration.RecentProjectsFilename;
        public string? TasksFilename => ServiceConfiguration.TasksFilename;

        public ProjectService(CancellationTokenSource shutdownTokenSource)
            : base(shutdownTokenSource)
        {
            if (!IConfiguration.ValidateConfiguration(ServiceConfiguration))
            {
                Debug.WriteLine($"Error starting Project service: required configuration not set.");
                base.shutdownTokenSource.Cancel();

                return;
            }

            if (!Directory.Exists(ServiceConfiguration.ProjectsDirectory))
                Directory.CreateDirectory(ServiceConfiguration.ProjectsDirectory);

            LoadRecentProjects();

            // load last project, if configured to do so
            if (ServiceConfiguration.LoadLastProjectOnStartup && _recentProjects.Count > 0)
                CurrentProject = _recentProjects[0];
        }

        public override async Task ServiceStarted()
        {
            await Task.CompletedTask;

            Debug.WriteLine("!");
        }

        public override async Task ServiceStopped()
        {
            await Task.CompletedTask;

            if (CurrentProject != null)
                await SaveProjectAsync(CurrentProject);

            SaveRecentProjects();
        }

        public async Task<ProjectData?> CreateProjectAsync(string? name = null, string? author = null, string? description = null)
        {
            Debug.WriteLine($"Service: [{nameof(ProjectService)}] Method: [{nameof(CreateProjectAsync)}]");

            await Task.CompletedTask;

            var newProject = new ProjectData()
            {
                DisplayName = name,
                Author = author,
                Description = description
            };

            await SaveProjectAsync(newProject);
            CurrentProject = newProject;

            return newProject;
        }

        public async Task<bool> UpdateProjectAsync(ProjectData projectData)
        {
            Debug.WriteLine($"Service: [{nameof(ProjectService)}] Method: [{nameof(UpdateProjectAsync)}]");

            if (CurrentProject?.ID != projectData.ID)
                return false;

            CurrentProject.DisplayName = projectData.DisplayName;
            CurrentProject.Author = projectData.Author;
            CurrentProject.Description = projectData.Description;

            if (!await SaveProjectAsync())
                return false;

            return true;
        }

        public ProjectData? GetCurrentProject()
        {
            Debug.WriteLine($"Service: [{nameof(ProjectService)}] Method: [{nameof(GetCurrentProject)}]");

            return CurrentProject;
        }

        public async Task<bool> SaveProjectAsync()
            => await SaveProjectAsync(CurrentProject);

        private async Task<bool> SaveProjectAsync(ProjectData? projectData)
        {
            Debug.WriteLine($"Service: [{nameof(ProjectService)}] Method: [{nameof(SaveProjectAsync)}]");

            try
            {
                if (projectData == null)
                    return false;

                var projectPath = GetProjectPath(projectData.ID);
                Directory.CreateDirectory(projectPath);

                var projectFilePath = Path.Combine(projectPath, "project.json");
                var json = JsonConvert.SerializeObject(projectData, Formatting.Indented);
                await File.WriteAllTextAsync(projectFilePath, json).ConfigureAwait(false);

                return true;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to save project [{projectData?.DisplayName}] with ID [{projectData?.ID}]. Message: {exc}");
            }

            return false;
        }

        public async Task<ProjectData?> LoadProjectAsync(Guid id)
        {
            Debug.WriteLine($"Service: [{nameof(ProjectService)}] Method: [{nameof(LoadProjectAsync)}]");

            try
            {
                var projectPath = GetProjectPath(id);
                if (!Directory.Exists(projectPath))
                    return null;

                var projectFilePath = Path.Combine(projectPath, "project.json");
                if (!File.Exists(projectFilePath))
                    return null;

                var json = await File.ReadAllTextAsync(projectFilePath).ConfigureAwait(false);
                var project = JsonConvert.DeserializeObject<ProjectData>(json);

                if (project == null)
                    return null;

                CurrentProject = project;

                return project;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to load project with ID [{id}]. Message: {exc}");
            }

            return null;
        }

        public async Task<ProjectData?> LoadProjectAsync(string name)
        {
            Debug.WriteLine($"Service: [{nameof(ProjectService)}] Method: [{nameof(LoadProjectAsync)}]");

            try
            {
                // potentially slow-
                // iterates all projects for one with a matching name, and loads if found
                foreach (var directory in Directory.GetDirectories(ServiceConfiguration.ProjectsDirectory))
                {
                    var projectFilePath = Path.Combine(directory, "project.json");
                    if (File.Exists(projectFilePath))
                    {
                        var json = await File.ReadAllTextAsync(projectFilePath).ConfigureAwait(false);
                        var project = JsonConvert.DeserializeObject<ProjectData>(json);

                        if (project?.DisplayName == null)
                            continue;

                        if (project.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            CurrentProject = project;

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

        private string GetProjectPath(Guid id)
        {
            Debug.WriteLine($"Service: [{nameof(ProjectService)}] Method: [{nameof(GetProjectPath)}]");

            return Path.Combine(ServiceConfiguration.ProjectsDirectory, id.ToString());
        }

        private void AddToRecentProjects(ProjectData project)
        {
            Debug.WriteLine($"Service: [{nameof(ProjectService)}] Method: [{nameof(AddToRecentProjects)}]");

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
            Debug.WriteLine($"Service: [{nameof(ProjectService)}] Method: [{nameof(LoadRecentProjects)}]");

            try
            {
                _recentProjects.Clear();

                if (File.Exists(ServiceConfiguration.RecentProjectsFilename))
                {
                    var recentIDsString = File.ReadAllTextAsync(ServiceConfiguration.RecentProjectsFilename).Result;
                    var recentIDs = JsonConvert.DeserializeObject<List<string>>(recentIDsString);
                    if (recentIDs != null)
                    {
                        foreach (var recentID in recentIDs)
                        {
                            if (Guid.TryParse(recentID, out var projectID))
                            {
                                var recentProject = LoadProjectAsync(projectID).Result;
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
            Debug.WriteLine($"Service: [{nameof(ProjectService)}] Method: [{nameof(SaveRecentProjects)}]");

            try
            {
                var recentProjectIDs = _recentProjects.Select(p => p.ID.ToString()).ToList();

                var json = JsonConvert.SerializeObject(recentProjectIDs, Formatting.Indented);
                File.WriteAllText(ServiceConfiguration.RecentProjectsFilename, json);
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to save recent projects. Message: {exc}");
            }
        }

        public IReadOnlyList<ProjectData> GetRecentProjects()
        {
            Debug.WriteLine($"Service: [{nameof(ProjectService)}] Method: [{nameof(GetRecentProjects)}]");

            return _recentProjects.AsReadOnly();
        }

        public async Task<bool> SaveDataAsync(string key, string data)
        {
            Debug.WriteLine($"Service: [{nameof(ProjectService)}] Method: [{nameof(SaveDataAsync)}]");

            try
            {
                if (string.IsNullOrEmpty(key))
                    return false;

                if (CurrentProject == null)
                    return false;

                var projectPath = GetProjectPath(CurrentProject.ID);
                var filePath = Path.Combine(projectPath, $"{key}.json");
                await File.WriteAllTextAsync(filePath, data).ConfigureAwait(false);

                return true;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to save data for project [{CurrentProject?.ID}] at key [{key}]. Message: {exc}");
            }

            return false;
        }

        public async Task<string?> LoadDataAsync(string key)
        {
            Debug.WriteLine($"Service: [{nameof(ProjectService)}] Method: [{nameof(LoadDataAsync)}]");

            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    return null;

                if (CurrentProject == null)
                    return null;

                var filepath = Path.Combine(GetProjectPath(CurrentProject.ID), $"{key}.json");
                var stringContents = await ReadFileContentsAsync(filepath);

                return stringContents;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to load data for project [{CurrentProject?.ID}] at key [{key}]. Message: {exc}");
            }

            return null;
        }

        public async Task<bool> DeleteDataAsync(string key)
        {
            Debug.WriteLine($"Service: [{nameof(ProjectService)}] Method: [{nameof(DeleteDataAsync)}]");

            try
            {
                await Task.CompletedTask;

                if (string.IsNullOrWhiteSpace(key))
                    return false;

                if (CurrentProject == null)
                    return false;

                var filePath = Path.Combine(GetProjectPath(CurrentProject.ID), $"{key}.json");
                

                return true;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to delete data for project [{CurrentProject?.ID}] at key [{key}]. Message: {exc}");
            }

            return false;
        }

        private static async Task<string?> ReadFileContentsAsync(string? filepath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filepath))
                    return null;

                var stringContents = await File.ReadAllTextAsync(filepath);

                return stringContents;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to load data from file at path [{filepath}]. Message: {exc}");
            }

            return null;
        }

        private static async Task<bool> DeleteFileAsync(string? filepath)
        {
            await Task.CompletedTask;

            try
            {
                if (string.IsNullOrWhiteSpace(filepath))
                    return false;

                File.Delete(filepath);
                return true;
            }
            catch (Exception exc)
            {
                Debug.WriteLine($"An exception occurred when attempting to delete file at path [{filepath}]. Message: {exc}");
            }

            return false;
        }
    }
}
