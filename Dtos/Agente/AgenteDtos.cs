namespace Novati.API.Dtos.Agente;

/// <summary>Pedido de um link de download do agente. Com funcionarioId, o link é-lhe enviado por notificação.</summary>
public class CriarLinkAgenteRequest
{
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid? FuncionarioId { get; set; }
}

/// <summary>Link de download: <c>Caminho</c> é a página do front (/agente?t=…) a entregar ao funcionário.</summary>
public record LinkAgenteDto(string Caminho, string Token, DateTime ExpiraEm, Guid? EnviadoA);

/// <summary>Estado de um link, para a página de download.</summary>
public record AgenteInfoDto(bool Disponivel, bool LinkValido, DateTime? ExpiraEm, long? TamanhoBytes);
