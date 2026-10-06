using Novati.API.Dtos.Ficheiros;
using Novati.API.Models.Entities;

namespace Novati.API.Mappers;

public static class FicheiroMapper
{
    public static FicheiroDto ToDto(Ficheiro f) => new(
        f.Id, f.NomeOriginal, f.ContentType, f.TamanhoBytes, f.CriadoEm, f.CriadoPorId);
}
