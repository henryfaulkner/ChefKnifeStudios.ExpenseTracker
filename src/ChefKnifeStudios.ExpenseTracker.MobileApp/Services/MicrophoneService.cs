using ChefKnifeStudios.ExpenseTracker.Shared.Services;
using CommunityToolkit.Maui.Media;
using System.Globalization;

namespace ChefKnifeStudios.ExpenseTracker.MobileApp.Services;

public class MicrophoneService : IMicrophoneService
{
    readonly ISpeechToText _speechToText;
    readonly IToastService _toastService;

    public string RecognitionText { get; private set; }

    public MicrophoneService(ISpeechToText speechToText, IToastService toastService)
    {
        _speechToText = speechToText;
        _toastService = toastService;
    }

    public async Task StartListening(CancellationToken cancellationToken)
    {
        var isGranted = await _speechToText.RequestPermissions(cancellationToken);
        if (!isGranted)
        {
            _toastService.ShowWarning("Permission not granted");
            return;
        }

        _speechToText.RecognitionResultUpdated += OnRecognitionTextUpdated;
        _speechToText.RecognitionResultCompleted += OnRecognitionTextCompleted;
        await SpeechToText.StartListenAsync(new SpeechToTextOptions { Culture = CultureInfo.CurrentCulture, ShouldReportPartialResults = true }, CancellationToken.None);
    }

    public async Task StopListening(CancellationToken cancellationToken)
    {
        await _speechToText.StopListenAsync(CancellationToken.None);
        _speechToText.RecognitionResultUpdated -= OnRecognitionTextUpdated;
        _speechToText.RecognitionResultCompleted -= OnRecognitionTextCompleted;
    }

    void OnRecognitionTextUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs args)
    {
        RecognitionText += args.RecognitionResult;
    }

    void OnRecognitionTextCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs args)
    {
        RecognitionText = args.RecognitionResult.Text ?? string.Empty;
    }
}
