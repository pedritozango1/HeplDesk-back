using Novati.API.Common.Exceptions;
using Novati.API.Common.Paginacao;
using Novati.API.Dtos.Compras;
using Novati.API.Mappers;
using Novati.API.Models.Entities;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;
using Novati.API.Services.Interfaces;

namespace Novati.API.Services;

/// <summary>Fluxo de requisição de compra: aprovar → dar entrada em stock → entregue.</summary>
public class ComprasService(
    ICompraRepository compras,
    IStockService stockService,
    INotificacaoService notificacaoService,
    IUnitOfWork uow) : IComprasService
{
    public async Task<List<RequisicaoDto>> GetVisiveisAsync(Role role, CancellationToken ct = default)
    {
        if (role is not (Role.ADMIN or Role.GESTOR))
            return [];

        return (await compras.GetAllOrderedAsync(ct)).Select(RequisicaoMapper.ToDto).ToList();
    }

    public async Task<PaginaResultado<RequisicaoDto>> GetPaginaAsync(Role role, PaginacaoQuery paginacao, string? estado, CancellationToken ct = default)
    {
        // Mesma regra de visibilidade da lista completa: filtra em vez de devolver 403.
        if (role is not (Role.ADMIN or Role.GESTOR))
            return PaginaResultado<RequisicaoDto>.Vazia(paginacao);

        var filtroEstado = FiltroEnum.Ler<EstadoRequisicao>(estado, "estado");
        return (await compras.GetPaginaAsync(paginacao, filtroEstado, ct)).Map(RequisicaoMapper.ToDto);
    }

    public async Task<RequisicaoDto> AprovarAsync(Guid id, CancellationToken ct = default)
    {
        var requisicao = await CarregarAsync(id, ct);

        if (requisicao.Estado != EstadoRequisicao.PENDENTE)
            throw new BusinessRuleException("Só requisições pendentes podem ser aprovadas.");

        requisicao.Estado = EstadoRequisicao.APROVADA;
        compras.Update(requisicao);

        await notificacaoService.CriarAsync(
            requisicao.SolicitanteId,
            "A sua requisição de compra foi aprovada.",
            "/compras", ct);

        await uow.SaveChangesAsync(ct);

        return RequisicaoMapper.ToDto(requisicao);
    }

    public async Task<RequisicaoDto> DarEntradaAsync(Guid id, CancellationToken ct = default)
    {
        var requisicao = await CarregarAsync(id, ct);

        if (requisicao.Estado != EstadoRequisicao.APROVADA)
            throw new BusinessRuleException("A requisição tem de estar aprovada para dar entrada.");

        requisicao.Estado = EstadoRequisicao.ENTREGUE;
        compras.Update(requisicao);

        // Reutiliza a criação de unidades + movimento do Módulo 9 — nada de duplicar esta lógica.
        // O CriarUnidadesAsync grava tudo o que estiver pendente no contexto, incluindo o estado acima.
        await stockService.CriarUnidadesAsync(
            requisicao.ModeloComponenteId,
            requisicao.Quantidade,
            $"Entrada da requisição {requisicao.Id}.",
            TipoMovimento.ENTRADA, ct);

        await notificacaoService.CriarAsync(
            requisicao.SolicitanteId,
            $"A requisição de compra {requisicao.Id} foi satisfeita — peças já em stock.",
            "/compras", ct);

        await uow.SaveChangesAsync(ct);

        return RequisicaoMapper.ToDto(requisicao);
    }

    public async Task<RequisicaoDto> RecusarAsync(Guid id, CancellationToken ct = default)
    {
        var requisicao = await CarregarAsync(id, ct);

        if (requisicao.Estado != EstadoRequisicao.PENDENTE)
            throw new BusinessRuleException("Só requisições pendentes podem ser recusadas.");

        requisicao.Estado = EstadoRequisicao.RECUSADA;
        compras.Update(requisicao);

        await notificacaoService.CriarAsync(
            requisicao.SolicitanteId,
            $"A sua requisição de compra {requisicao.Id} foi recusada.",
            "/compras", ct);

        await uow.SaveChangesAsync(ct);

        return RequisicaoMapper.ToDto(requisicao);
    }

    private async Task<RequisicaoCompra> CarregarAsync(Guid id, CancellationToken ct)
        => await compras.GetByIdAsync(id, ct)
           ?? throw new NotFoundException("Requisição de compra não encontrada.");
}
