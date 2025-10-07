using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StoDamageMeter.Models;

namespace StoDamageMeter.Services
{
    /// <summary>
    /// Service für die Kommunikation mit dem OSCR Python-Backend
    /// </summary>
    public interface IOSCRBackendService
    {
        /// <summary>
        /// Führt einen Health-Check des Backends durch
        /// </summary>
        Task<HealthCheckResponse> HealthCheckAsync();

        /// <summary>
        /// Ruft verfügbare Combats aus einer Log-Datei ab
        /// </summary>
        Task<AvailableCombatsResponse> GetAvailableCombatsAsync(string logPath, int maxCombats = 50);

        /// <summary>
        /// Analysiert Combats aus einer Log-Datei
        /// </summary>
        Task<CombatAnalysisResponse> AnalyzeCombatLogAsync(string logPath, int maxCombats = 10, AnalysisSettings? settings = null);

        /// <summary>
        /// Prüft ob das Backend verfügbar ist
        /// </summary>
        Task<bool> IsBackendAvailableAsync();

        /// <summary>
        /// Event das ausgelöst wird, wenn sich der Analyse-Fortschritt ändert
        /// </summary>
        event EventHandler<CombatAnalysisProgressEventArgs>? AnalysisProgress;
    }

    /// <summary>
    /// Event-Args für Analyse-Fortschritt
    /// </summary>
    public class CombatAnalysisProgressEventArgs : EventArgs
    {
        public string Message { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
        public bool IsCompleted { get; set; }
    }

    /// <summary>
    /// Exception für Backend-Fehler
    /// </summary>
    public class OSCRBackendException : Exception
    {
        public OSCRBackendException(string message) : base(message) { }
        public OSCRBackendException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Implementierung des OSCR-Backend-Services
    /// </summary>
    public class OSCRBackendService : IOSCRBackendService
    {
        private readonly string _backendPath;
        private readonly string _backendArgs;
        private readonly ILogger<OSCRBackendService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public event EventHandler<CombatAnalysisProgressEventArgs>? AnalysisProgress;

        public OSCRBackendService(IConfiguration configuration, ILogger<OSCRBackendService> logger)
        {
            _logger = logger;
            
            // Backend-Pfad relativ zum Deploy-Verzeichnis
            var frontendDir = AppDomain.CurrentDomain.BaseDirectory;
            var deployDir = Path.Combine(Path.GetDirectoryName(frontendDir) ?? "", "Deploy");
            
            // Versuche zuerst Batch-Backend, dann arbeitendes OSCR-Backend, dann echtes OSCR-Backend, dann Python-Backend, dann Executable
            var batchBackendPath = Path.Combine(deployDir, "start_backend.bat");
            var workingOscrBackendPath = Path.Combine(deployDir, "working_oscr_backend.py");
            var realOscrBackendPath = Path.Combine(deployDir, "real_oscr_backend.py");
            var pythonBackendPath = Path.Combine(deployDir, "python_backend.py");
            var executableBackendPath = Path.Combine(deployDir, configuration["OSCRBackendPath"] ?? "OSCRBackend.exe");
            
            if (File.Exists(batchBackendPath))
            {
                _backendPath = batchBackendPath;
                _backendArgs = "";
                _logger.LogInformation("Using batch backend: {BackendPath}", batchBackendPath);
            }
            else if (File.Exists(workingOscrBackendPath))
            {
                _backendPath = "python";
                _backendArgs = $"\"{workingOscrBackendPath}\" --api";
                _logger.LogInformation("Using working OSCR backend: {BackendPath}", workingOscrBackendPath);
            }
            else if (File.Exists(realOscrBackendPath))
            {
                _backendPath = "python";
                _backendArgs = $"\"{realOscrBackendPath}\" --api";
                _logger.LogInformation("Using real OSCR backend: {BackendPath}", realOscrBackendPath);
            }
            else if (File.Exists(pythonBackendPath))
            {
                _backendPath = "python";
                _backendArgs = $"\"{pythonBackendPath}\" --api";
                _logger.LogInformation("Using Python backend: {BackendPath}", pythonBackendPath);
            }
            else if (File.Exists(executableBackendPath))
            {
                _backendPath = executableBackendPath;
                _backendArgs = "--api";
                _logger.LogInformation("Using executable backend: {BackendPath}", executableBackendPath);
            }
            else
            {
                _backendPath = executableBackendPath;
                _backendArgs = "--api";
                _logger.LogWarning("Backend not found, using default path: {BackendPath}", executableBackendPath);
            }
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            _logger.LogInformation("OSCRBackendService initialized with backend path: {BackendPath}", _backendPath);
        }

        /// <summary>
        /// Führt einen Health-Check des Backends durch
        /// </summary>
        public async Task<HealthCheckResponse> HealthCheckAsync()
        {
            try
            {
                _logger.LogInformation("Performing health check");
                
                var request = new HealthCheckRequest();
                var response = await ExecuteBackendCommandAsync<HealthCheckResponse>(request);
                
                _logger.LogInformation("Health check completed: {Status}", response.Status);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                throw new OSCRBackendException("Health check failed", ex);
            }
        }

        /// <summary>
        /// Ruft verfügbare Combats aus einer Log-Datei ab
        /// </summary>
        public async Task<AvailableCombatsResponse> GetAvailableCombatsAsync(string logPath, int maxCombats = 50)
        {
            try
            {
                _logger.LogInformation("Getting available combats from: {LogPath}", logPath);
                
                if (!File.Exists(logPath))
                {
                    throw new FileNotFoundException($"Log file not found: {logPath}");
                }

                var request = new AvailableCombatsRequest
                {
                    LogPath = logPath,
                    MaxCombats = maxCombats
                };

                var response = await ExecuteBackendCommandAsync<AvailableCombatsResponse>(request);
                
                _logger.LogInformation("Found {Count} available combats", response.TotalCombats);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available combats from: {LogPath}", logPath);
                throw new OSCRBackendException($"Failed to get available combats from {logPath}", ex);
            }
        }

        /// <summary>
        /// Analysiert Combats aus einer Log-Datei
        /// </summary>
        public async Task<CombatAnalysisResponse> AnalyzeCombatLogAsync(string logPath, int maxCombats = 10, AnalysisSettings? settings = null)
        {
            try
            {
                _logger.LogInformation("Starting combat analysis for: {LogPath}", logPath);
                
                if (!File.Exists(logPath))
                {
                    throw new FileNotFoundException($"Log file not found: {logPath}");
                }

                OnAnalysisProgress("Starting analysis...", 0, false);

                var request = new CombatAnalysisRequest
                {
                    LogPath = logPath,
                    MaxCombats = maxCombats,
                    Settings = settings ?? new AnalysisSettings()
                };

                OnAnalysisProgress("Analyzing combat log...", 25, false);

                var response = await ExecuteBackendCommandAsync<CombatAnalysisResponse>(request);
                
                OnAnalysisProgress("Analysis completed", 100, true);
                
                _logger.LogInformation("Combat analysis completed: {Count} combats analyzed", response.TotalCombats);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Combat analysis failed for: {LogPath}", logPath);
                OnAnalysisProgress($"Analysis failed: {ex.Message}", 0, true);
                throw new OSCRBackendException($"Combat analysis failed for {logPath}", ex);
            }
        }

        /// <summary>
        /// Prüft ob das Backend verfügbar ist
        /// </summary>
        public async Task<bool> IsBackendAvailableAsync()
        {
            try
            {
                if (!File.Exists(_backendPath))
                {
                    _logger.LogWarning("Backend executable not found: {BackendPath}", _backendPath);
                    return false;
                }

                // Führe einen schnellen Health-Check durch
                var healthResponse = await HealthCheckAsync();
                return healthResponse.Success;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Backend availability check failed");
                return false;
            }
        }

        /// <summary>
        /// Führt einen Backend-Befehl aus und gibt die JSON-Response zurück
        /// </summary>
        private async Task<T> ExecuteBackendCommandAsync<T>(object request) where T : OSCRResponse
        {
            var jsonInput = JsonSerializer.Serialize(request, _jsonOptions);
            
            _logger.LogDebug("Executing backend command with input: {JsonInput}", jsonInput);

            using var process = new Process();
            process.StartInfo.FileName = _backendPath;
            process.StartInfo.Arguments = _backendArgs;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    outputBuilder.AppendLine(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    errorBuilder.AppendLine(e.Data);
                }
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // JSON-Input senden
                await process.StandardInput.WriteAsync(jsonInput);
                process.StandardInput.Close();

                // Warten auf Beendigung mit Timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                await process.WaitForExitAsync(cts.Token);
                var completed = process.HasExited;
                
                if (!completed)
                {
                    process.Kill();
                    throw new OSCRBackendException("Backend process timed out after 5 minutes");
                }

                var output = outputBuilder.ToString();
                var error = errorBuilder.ToString();

                _logger.LogDebug("Backend output: {Output}", output);
                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogDebug("Backend error output: {Error}", error);
                }

                if (process.ExitCode != 0)
                {
                    throw new OSCRBackendException($"Backend process failed with exit code {process.ExitCode}. Error: {error}");
                }

                if (string.IsNullOrEmpty(output))
                {
                    throw new OSCRBackendException("Backend returned empty response");
                }

                // JSON-Response parsen
                var response = JsonSerializer.Deserialize<T>(output, _jsonOptions);
                if (response == null)
                {
                    throw new OSCRBackendException("Failed to deserialize backend response");
                }

                if (!response.Success)
                {
                    throw new OSCRBackendException($"Backend returned error: {response.Error}");
                }

                return response;
            }
            catch (Exception ex) when (!(ex is OSCRBackendException))
            {
                throw new OSCRBackendException("Failed to execute backend command", ex);
            }
        }

        /// <summary>
        /// Löst das AnalysisProgress-Event aus
        /// </summary>
        private void OnAnalysisProgress(string message, int progressPercentage, bool isCompleted)
        {
            AnalysisProgress?.Invoke(this, new CombatAnalysisProgressEventArgs
            {
                Message = message,
                ProgressPercentage = progressPercentage,
                IsCompleted = isCompleted
            });
        }
    }
}
