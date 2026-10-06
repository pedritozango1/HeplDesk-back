using Novati.API.Dtos.Ficheiros;
using Novati.API.Models.Enums;

namespace Novati.API.Services.Interfaces;

public interface IFicheiroService
{
    Task<FicheiroDto> UploadAsync(Guid userId, IFormFile ficheiro, CancellationToken ct = default);
    Task<FicheiroDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(Stream Conteudo, string ContentType, string Nome)> AbrirAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Role role, Guid id, CancellationToken ct = default);
}
