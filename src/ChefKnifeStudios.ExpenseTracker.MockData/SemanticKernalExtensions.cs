using Microsoft.SemanticKernel;

namespace ChefKnifeStudios.ExpenseTracker.MockData;

public static class SemanticKernalExtensions
{
    public static IKernelBuilder ConfigureSemanticKernel(this IKernelBuilder kernelBuilder, AppSettings appSettings)
    {
        // Register the OpenAI chat completion service
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: appSettings.OpenAI.ChatModelId,
            apiKey: appSettings.OpenAI.ApiKey);

        // Register the OpenAI embedding generator
        #pragma warning disable SKEXP0010
        kernelBuilder.AddOpenAIEmbeddingGenerator(
            modelId: appSettings.OpenAI.EmbeddingModelId,
            apiKey: appSettings.OpenAI.ApiKey);
        #pragma warning restore SKEXP0010

        return kernelBuilder;
    }
}
