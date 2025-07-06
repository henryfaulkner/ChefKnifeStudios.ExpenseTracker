using ChefKnifeStudios.ExpenseTracker.Shared.Models;
using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Services;

public class AppSessionService : IAppSessionService
{
    private const string AppFolder = "ExpenseTracker";
    private const string SessionFileName = "app_id.txt";
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public AppSessionService()
    {
        // Get the application data directory path
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _filePath = Path.Combine(appDataPath, AppFolder, SessionFileName);

        // Configure JSON serialization options
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (File.Exists(_filePath))
            {
                // Try to read and deserialize existing session
                var fileContent = await File.ReadAllTextAsync(_filePath, cancellationToken);

                if (!string.IsNullOrWhiteSpace(fileContent))
                {
                    try
                    {
                        var existingSession = JsonSerializer.Deserialize<AppSession>(fileContent, _jsonOptions);

                        // Validate that deserialization was successful and AppId is not empty
                        if (existingSession != null && existingSession.AppId != Guid.Empty)
                        {
                            // Session file exists and is valid, no need to create new one
                            return;
                        }
                    }
                    catch (JsonException)
                    {
                        // Deserialization failed, fall through to create new session
                    }
                }
            }

            // File doesn't exist or deserialization failed - create new session
            var newSession = new AppSession
            {
                AppId = Guid.NewGuid()
            };

            await CreateSessionFileAsync(newSession, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize session: {ex.Message}", ex);
        }
    }

    public async Task SetSessionAsync(AppSession session, CancellationToken cancellationToken = default)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        try
        {
            await CreateSessionFileAsync(session, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to set session: {ex.Message}", ex);
        }
    }

    public async Task<AppSession> GetSessionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException($"Session file not found at {_filePath}. Make sure to call InitAsync() first.");
            }

            var fileContent = await File.ReadAllTextAsync(_filePath, cancellationToken);

            if (string.IsNullOrWhiteSpace(fileContent))
            {
                throw new InvalidOperationException("Session file is empty.");
            }

            var session = JsonSerializer.Deserialize<AppSession>(fileContent, _jsonOptions);

            if (session == null)
            {
                throw new InvalidOperationException("Failed to deserialize session from file.");
            }

            return session;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize session: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get session: {ex.Message}", ex);
        }
    }

    private async Task CreateSessionFileAsync(AppSession session, CancellationToken cancellationToken = default)
    {
        var serializedSession = JsonSerializer.Serialize(session, _jsonOptions);

        // Ensure directory exists
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(_filePath, serializedSession, cancellationToken);
    }
}
