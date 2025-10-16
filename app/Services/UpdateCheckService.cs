using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Reflection;

namespace StoDamageMeter.Services
{
    public class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = string.Empty; // "v2.0.1"
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("published_at")]
        public DateTime PublishedAt { get; set; }
        
        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty; // Release Notes
    }

    public class UpdateInfo
    {
        public bool UpdateAvailable { get; set; }
        public string LatestVersion { get; set; } = string.Empty;
        public string ReleaseUrl { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
    }

    public class UpdateCheckService
    {
        private readonly HttpClient _httpClient;
        private const string GITHUB_API_URL = "https://api.github.com/repos/VanKhaos/StoDamageMeter/releases/latest";
        
        // Test-Modus für Update-Simulation
        private const bool TEST_MODE = false; // Setze auf false für Produktion

        public UpdateCheckService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "STO-Damage-Meter/2.0.1");
        }

        public async Task<UpdateInfo?> CheckForUpdatesAsync()
        {
            // Test-Modus: Simuliere immer verfügbares Update
            if (TEST_MODE)
            {
                // Simuliere Netzwerk-Delay
                await Task.Delay(1000);
                
                return new UpdateInfo
                {
                    UpdateAvailable = true,
                    LatestVersion = "2.1.0",
                    ReleaseUrl = "https://github.com/VanKhaos/StoDamageMeter/releases/latest",
                    ReleaseNotes = "🚀 **Test Update - Simulation Mode**\n\n" +
                                  "This is a simulated update for testing purposes.\n\n" +
                                  "**New Features:**\n" +
                                  "- Update notification system\n" +
                                  "- Material Design improvements\n" +
                                  "- Bug fixes and performance enhancements\n\n" +
                                  "**Note:** This is a test update. In production, this would be a real update.",
                    PublishedAt = DateTime.Now.AddDays(-1)
                };
            }

            try
            {
                var response = await _httpClient.GetStringAsync(GITHUB_API_URL);
                var release = JsonSerializer.Deserialize<GitHubRelease>(response);

                if (release == null)
                    return null;

                var currentVersion = GetCurrentVersion();
                var latestVersion = ParseVersion(release.TagName);

                return new UpdateInfo
                {
                    UpdateAvailable = IsNewerVersion(latestVersion, currentVersion),
                    LatestVersion = latestVersion.ToString(),
                    ReleaseUrl = release.HtmlUrl,
                    ReleaseNotes = release.Body,
                    PublishedAt = release.PublishedAt
                };
            }
            catch (Exception)
            {
                // Silent fail - Update check ist nicht kritisch
                return null;
            }
        }

        public Version GetCurrentVersion()
        {
            // Einfache Versionserkennung - hardcoded für Stabilität
            return new Version(2, 0, 1, 0);
        }

        private Version ParseVersion(string tagName)
        {
            // Entferne "v" Prefix falls vorhanden
            var versionString = tagName.StartsWith("v") ? tagName[1..] : tagName;
            
            if (Version.TryParse(versionString, out var version))
                return version;
            
            // Fallback falls Parsing fehlschlägt
            return new Version(0, 0, 0, 0);
        }

        private bool IsNewerVersion(Version latestVersion, Version currentVersion)
        {
            return latestVersion > currentVersion;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
