using Novati.API.Dtos.Users;

namespace Novati.API.Dtos.Auth;

/// <summary>Resposta ao login: token + dados do utilizador.</summary>
public record LoginResponse(string AccessToken, UserDto User);