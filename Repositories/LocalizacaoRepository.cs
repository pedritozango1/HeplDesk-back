using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

public class LocalizacaoRepository(AppDbContext ctx) : Repository<Localizacao>(ctx), ILocalizacaoRepository
{
}
