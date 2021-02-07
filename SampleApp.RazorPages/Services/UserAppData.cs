using Dapper.CX.SqlServer.AspNetCore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SampleApp.RazorPages.Services
{
    /// <summary>
    /// reads/writes Json to a folder in App_Data in the app to
    /// serve as a user session data repository, to avoid unnecessary database traffic    
    /// </summary>
    public class UserAppData : ISession
    {        
        private readonly string _rootPath;
        private readonly string _storagePath;
        private readonly string _userName;
        private readonly bool _enabled;

        public UserAppData(IServiceProvider serviceProvider, string storagePath = "Session")
        {
            var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();

            _rootPath = env.ContentRootPath;
            _storagePath = storagePath;
            _userName = serviceProvider.GetUserName();
            _enabled = !string.IsNullOrEmpty(_userName);
        }

        private string GetPath() => (IsAvailable) ? 
            Path.Combine(_rootPath, "App_Data", _storagePath, _userName) : 
            throw new Exception("No logged in user.");

        private string GetFilename(string key) => Path.Combine(GetPath(), $"{key}.json");

        public bool IsAvailable => _enabled;

        public string Id => throw new NotImplementedException();

        public IEnumerable<string> Keys => Directory
            .GetFiles(GetPath(), "*.json")
            .Select(fileName => Path.GetFileNameWithoutExtension(fileName));

        public void Clear()
        {
            var files = Directory.GetFiles(GetPath(), "*.json");
            foreach (var file in files) File.Delete(file);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default) => await Task.CompletedTask;

        public async Task LoadAsync(CancellationToken cancellationToken = default) => await Task.CompletedTask;        

        public void Remove(string key)
        {
            try
            {
                File.Delete(GetFilename(key));
            }
            catch 
            {
                // do nothing
            }            
        }

        public void Set(string key, byte[] value)
        {
            if (!IsAvailable) return;
            if (!Directory.Exists(GetPath())) Directory.CreateDirectory(GetPath());
            File.WriteAllBytes(GetFilename(key), value);
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            var result = TryGetFileContent(key);
            if (result.success)
            {
                value = Encoding.UTF8.GetBytes(result.content);
                return true;
            }

            value = null;
            return false;
        }

        private (bool success, string content) TryGetFileContent(string key)
        {
            try
            {
                if (!IsAvailable) return (false, null);

                string content = File.ReadAllText(GetFilename(key));
                return (true, content);
            }
            catch
            {
                return (false, null);
            }
        }
    }
}
