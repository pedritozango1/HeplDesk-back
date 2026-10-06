namespace Novati.API.Dtos.Auth;

/// <summary>Conta disponível para entrar com um clique no ecrã de login (só em Development).</summary>
public record ContaDemoDto(string Nome, string Email, string Role);

/// <summary>Contas de demonstração e a password comum do seed (só em Development).</summary>
public record ContasDemoResponse(string Password, List<ContaDemoDto> Contas);
