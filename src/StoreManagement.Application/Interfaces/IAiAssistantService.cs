using StoreManagement.Application.DTOs;

namespace StoreManagement.Application.Interfaces;

public interface IAiAssistantService
{
    Task<AiChatResponse> ChatAsync(AiChatRequest request, CancellationToken cancellationToken);
}
