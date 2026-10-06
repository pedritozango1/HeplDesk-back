namespace Novati.API.Dtos.Dispositivos;

/// <summary>Instância de um componente instalado num dispositivo físico.</summary>
public record InstanciaDto(Guid Id, Guid DispositivoFisicoId, Guid ModeloComponenteId, string Codigo, string Estado);
