namespace StoreManagement.Application.DTOs;

public record AiChatRequest(string Message);
public record AiChatResponse(string Message, object? Data = null);
