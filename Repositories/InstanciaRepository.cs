using Novati.API.Data;
using Novati.API.Models.Entities;
using Novati.API.Repositories.Interfaces;

namespace Novati.API.Repositories;

public class InstanciaRepository(AppDbContext ctx) : Repository<InstanciaComponente>(ctx), IInstanciaRepository
{
}
