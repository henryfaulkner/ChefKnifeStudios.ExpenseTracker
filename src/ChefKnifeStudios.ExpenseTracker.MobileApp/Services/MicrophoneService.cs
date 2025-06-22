using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using CommunityToolkit.Maui.Media;
using System.Globalization;
using System.Text;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Services;

public class MicrophoneService : IMicrophoneService
{
    private readonly ISpeechToText _speechToText;
    private readonly IToastService _toastService;
    private readonly StringBuilder _resultBuilder = new();
    private readonly object _lock = new();

    public MicrophoneService(ISpeechToText speechToText, IToastService toastService)
    {
        _speechToText = speechToText;
        _toastService = toastService;
    }

    public async Task StartListeningAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var isGranted = await _speechToText.RequestPermissions(cancellationToken);
            if (!isGranted)
            {
                _toastService.ShowWarning("Permission not granted");
                return;
            }

            _speechToText.RecognitionResultUpdated += OnRecognitionTextUpdated;
            _speechToText.RecognitionResultCompleted += OnRecognitionTextCompleted;

            _resultBuilder.Clear(); // Reset the recognition text
            await _speechToText.StartListenAsync(
                new SpeechToTextOptions
                {
                    Culture = CultureInfo.CurrentCulture,
                    ShouldReportPartialResults = true
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"Error starting speech recognition: {ex.Message}");
        }
    }

    public string GetCurrentText()
    {
        return _resultBuilder.ToString();
    }

    public async Task<string> StopListeningAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _speechToText.StopListenAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"Error stopping speech recognition: {ex.Message}");
        }
        finally
        {
            _speechToText.RecognitionResultUpdated -= OnRecognitionTextUpdated;
            _speechToText.RecognitionResultCompleted -= OnRecognitionTextCompleted;
        }

        // Return the aggregated recognition text
        lock (_lock)
        {
            return _resultBuilder.ToString();
        }
    }

    private void OnRecognitionTextUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs args)
    {
        lock (_lock)
        {
            _resultBuilder.Append(args.RecognitionResult);
        }
    }

    private void OnRecognitionTextCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs args)
    {
        lock (_lock)
        {
            _resultBuilder.Clear();
            _resultBuilder.Append(args.RecognitionResult.Text ?? string.Empty);
        }
    }
}
