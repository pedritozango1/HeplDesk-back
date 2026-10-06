namespace Novati.API.Dtos.Solicitacoes;

/// <summary>Avaliação do solicitante sobre uma solicitação resolvida/fechada.</summary>
public record AvaliacaoDto(int Estrelas, string Comentario, DateOnly Data);
