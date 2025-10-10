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
        /// Ruft verfügbare Combats mit Progress-Reporting ab
        /// </summary>
        Task<AvailableCombatsResponse> GetAvailableCombatsWithProgressAsync(
            string logPath, 
            int maxCombats = 50,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Analysiert Combats aus einer Log-Datei
        /// </summary>
        Task<CombatAnalysisResponse> AnalyzeCombatLogAsync(string logPath, int maxCombats = 10, AnalysisSettings? settings = null);

        /// <summary>
        /// Analysiert einen einzelnen Combat
        /// </summary>
        Task<CombatAnalysisResponse> AnalyzeSingleCombatAsync(string logPath, int combatId, CancellationToken cancellationToken = default);

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
            // BaseDirectory ist z.B.: D:\Projekte\StoDamageMeter\frontend\bin\Debug\net9.0-windows\
            // Wir müssen 4 Ebenen hoch zum Projekt-Root
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var backendFileName = configuration["OSCRBackendPath"] ?? "OSCRBackend.exe";
            
            // 1. Versuch: Backend im gleichen Verzeichnis wie die .exe (Release/Publish)
            var backendPathSameDir = Path.Combine(appDirectory, backendFileName);
            
            // 2. Versuch: Backend im Deploy-Verzeichnis (Development)
            var projectRoot = Path.GetFullPath(Path.Combine(appDirectory, "..", "..", "..", ".."));
            var deployDir = Path.Combine(projectRoot, "Deploy");
            var workingOscrBackendPath = Path.Combine(deployDir, "working_oscr_backend.py");
            var executableBackendPath = Path.Combine(deployDir, backendFileName);
            
            if (File.Exists(backendPathSameDir))
            {
                // Release-Modus: Backend liegt neben der .exe
                _backendPath = backendPathSameDir;
                _backendArgs = "--api";
                _logger.LogInformation("Using release backend from application directory: {BackendPath}", _backendPath);
            }
            else if (File.Exists(workingOscrBackendPath))
            {
                // Development-Modus: Python Backend
                _backendPath = "python";
                _backendArgs = $"\"{workingOscrBackendPath}\" --api";
                _logger.LogInformation("Using development backend (Python): {BackendPath}", workingOscrBackendPath);
            }
            else if (File.Exists(executableBackendPath))
            {
                // Development-Modus: Executable Backend
                _backendPath = executableBackendPath;
                _backendArgs = "--api";
                _logger.LogInformation("Using development backend (Executable): {BackendPath}", executableBackendPath);
            }
            else
            {
                // Fallback: Verwende den Pfad aus dem gleichen Verzeichnis (auch wenn er nicht existiert)
                _backendPath = backendPathSameDir;
                _backendArgs = "--api";
                _logger.LogWarning("Backend not found! Tried: {Path1} and {Path2}", backendPathSameDir, executableBackendPath);
                _logger.LogWarning("Using fallback path: {BackendPath}", _backendPath);
            }
            
            _logger.LogInformation("Final backend configuration - Path: {Path}, Args: {Args}", _backendPath, _backendArgs);
            _logger.LogInformation("Backend exists: {Exists}", File.Exists(_backendPath));
            
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
        /// Analysiert einen einzelnen Combat basierend auf ID
        /// </summary>
        public async Task<CombatAnalysisResponse> AnalyzeSingleCombatAsync(
            string logPath, 
            int combatId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Analyzing single combat ID {CombatId} from: {LogPath}", combatId, logPath);
                
                if (!File.Exists(logPath))
                {
                    throw new FileNotFoundException($"Log file not found: {logPath}");
                }

                var request = new SingleCombatAnalysisRequest
                {
                    LogPath = logPath,
                    CombatId = combatId,
                    Settings = new AnalysisSettings()
                };

                var response = await ExecuteBackendCommandWithProgressAsync<CombatAnalysisResponse>(request, cancellationToken);
                
                _logger.LogInformation("Single combat analysis completed for ID {CombatId}", combatId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Single combat analysis failed for ID {CombatId}: {LogPath}", combatId, logPath);
                throw new OSCRBackendException($"Single combat analysis failed for combat {combatId}", ex);
            }
        }

        /// <summary>
        /// Ruft verfügbare Combats mit Progress-Reporting ab
        /// </summary>
        public async Task<AvailableCombatsResponse> GetAvailableCombatsWithProgressAsync(
            string logPath, 
            int maxCombats = 50,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting available combats with progress for: {LogPath}", logPath);
                
                if (!File.Exists(logPath))
                {
                    throw new FileNotFoundException($"Log file not found: {logPath}");
                }

                var fileInfo = new FileInfo(logPath);
                var fileSizeKB = fileInfo.Length / 1024.0;
                OnAnalysisProgress($"Loading combat list from file ({fileSizeKB:F2} KB)...", 0, false);

                var request = new AvailableCombatsRequest
                {
                    LogPath = logPath,
                    MaxCombats = maxCombats
                };

                OnAnalysisProgress("Reading combat log...", 25, false);

                var response = await ExecuteBackendCommandWithProgressAsync<AvailableCombatsResponse>(
                    request, 
                    cancellationToken);
                
                OnAnalysisProgress($"Found {response.TotalCombats} combats", 100, true);
                
                _logger.LogInformation("Found {Count} available combats", response.TotalCombats);
                return response;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Combat list loading was cancelled");
                OnAnalysisProgress("Loading cancelled", 0, true);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available combats from: {LogPath}", logPath);
                OnAnalysisProgress($"Failed to load combat list: {ex.Message}", 0, true);
                throw new OSCRBackendException($"Failed to get available combats from {logPath}", ex);
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
        /// Führt einen Backend-Befehl aus mit CancellationToken-Support
        /// </summary>
        private async Task<T> ExecuteBackendCommandWithProgressAsync<T>(
            object request, 
            CancellationToken cancellationToken) where T : OSCRResponse
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
            
            // Erzwinge UTF-8 nur für Python's Output (stdout/stderr), nicht für Input
            process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

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
                    // Progress-Updates aus stderr
                    if (e.Data.Contains("progress") || e.Data.Contains("combats"))
                    {
                        OnAnalysisProgress(e.Data, 50, false);
                    }
                }
            };

            try
            {
                LogToFile($"Starting backend process: {process.StartInfo.FileName} {process.StartInfo.Arguments}");
                LogToFile($"JSON Input: {jsonInput}");
                
                _logger.LogInformation("Starting backend process: {FileName} {Arguments}", process.StartInfo.FileName, process.StartInfo.Arguments);
                
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                LogToFile("Process started. Sending JSON input...");
                _logger.LogInformation("Process started. Sending JSON input...");
                
                // JSON-Input als UTF-8 Bytes senden
                var jsonBytes = Encoding.UTF8.GetBytes(jsonInput);
                await process.StandardInput.BaseStream.WriteAsync(jsonBytes, 0, jsonBytes.Length);
                await process.StandardInput.BaseStream.FlushAsync();
                process.StandardInput.Close();
                
                LogToFile("JSON input sent. Waiting for process to exit...");
                _logger.LogInformation("JSON input sent. Waiting for process to exit...");

                // Warten auf Beendigung mit Timeout und Cancellation
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    new CancellationTokenSource(TimeSpan.FromMinutes(5)).Token);
                
                await process.WaitForExitAsync(cts.Token);
                
                if (cancellationToken.IsCancellationRequested)
                {
                    process.Kill();
                    throw new OperationCanceledException();
                }

                var output = outputBuilder.ToString();
                var error = errorBuilder.ToString();

                // Erweitertes Logging für Debugging
                _logger.LogInformation("=== Backend Response Debug ===");
                _logger.LogInformation("Exit Code: {ExitCode}", process.ExitCode);
                _logger.LogInformation("Output Length: {Length} chars", output?.Length ?? 0);
                _logger.LogInformation("Error Length: {Length} chars", error?.Length ?? 0);
                
                if (!string.IsNullOrEmpty(output))
                {
                    _logger.LogDebug("Backend stdout: {Output}", output.Length > 500 ? output.Substring(0, 500) + "..." : output);
                }
                
                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogWarning("Backend stderr: {Error}", error);
                    
                    // In Log-Datei schreiben (vollständig)
                    try
                    {
                        var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                        Directory.CreateDirectory(logsDir);
                        var logFile = Path.Combine(logsDir, "backend_debug.log");
                        File.AppendAllText(logFile, $"\n=== {DateTime.Now} ===\n");
                        File.AppendAllText(logFile, $"Command: {_backendPath} {_backendArgs}\n");
                        File.AppendAllText(logFile, $"Exit Code: {process.ExitCode}\n");
                        File.AppendAllText(logFile, $"STDOUT ({output?.Length ?? 0} chars): {output}\n");
                        File.AppendAllText(logFile, $"STDERR ({error?.Length ?? 0} chars): {error}\n");
                        File.AppendAllText(logFile, "=== END ===\n\n");
                    }
                    catch { }
                }

                if (process.ExitCode != 0)
                {
                    var errorMsg = $"Backend process failed with exit code {process.ExitCode}.";
                    if (!string.IsNullOrEmpty(error))
                    {
                        errorMsg += $"\nBackend stderr: {error}";
                    }
                    if (!string.IsNullOrEmpty(output))
                    {
                        errorMsg += $"\nBackend stdout: {output}";
                    }
                    throw new OSCRBackendException(errorMsg);
                }

                if (string.IsNullOrEmpty(output))
                {
                    var errorMsg = "Backend returned empty response.";
                    if (!string.IsNullOrEmpty(error))
                    {
                        errorMsg += $"\nBackend stderr: {error}";
                    }
                    throw new OSCRBackendException(errorMsg);
                }

                // Entferne UTF-8 BOM falls vorhanden (U+FEFF)
                output = output.TrimStart('\uFEFF');

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
            catch (Exception ex) when (!(ex is OSCRBackendException) && !(ex is OperationCanceledException))
            {
                throw new OSCRBackendException("Failed to execute backend command", ex);
            }
        }

        /// <summary>
        /// Führt einen Backend-Befehl aus und gibt die JSON-Response zurück
        /// </summary>
        private async Task<T> ExecuteBackendCommandAsync<T>(object request) where T : OSCRResponse
        {
            return await ExecuteBackendCommandWithProgressAsync<T>(request, CancellationToken.None);
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

        private void LogToFile(string message)
        {
            try
            {
                var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                Directory.CreateDirectory(logsDir);
                var logFile = Path.Combine(logsDir, "backend_service_debug.log");
                File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}");
            }
            catch
            {
                // Ignore
            }
        }
    }
}
