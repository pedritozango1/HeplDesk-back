namespace Novati.API.Dtos.Dispositivos;

/// <summary>Localização física (nó da árvore edifício → sala → posto).</summary>
public record LocalizacaoDto(Guid Id, string Nome, Guid? Pai);
