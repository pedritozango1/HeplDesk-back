using Microsoft.Extensions.Options;
using Novati.API.Common.Settings;
using Novati.API.Dtos.Dominio;
using Novati.API.Models.Enums;
using Novati.API.Services.Interfaces;

namespace Novati.API.Services;

/// <summary>
/// Fonte única dos valores de domínio para o front: os enums (com rótulo e cor)
/// e as listas configuráveis do appsettings. O front não tem nenhuma destas listas.
/// </summary>
public class DominioService(IOptions<DominioSettings> settings) : IDominioService
{
    // Rótulo e cor de cada membro. Um membro de enum sem entrada aqui aparece com
    // o próprio nome — assim acrescentar um valor ao enum nunca parte o front.
    private static readonly Dictionary<Enum, (string Rotulo, string Cor)> Textos = new()
    {
        [Role.ADMIN] = ("Administrador", "blue"),
        [Role.GESTOR] = ("Gestor", "purple"),
        [Role.TECNICO] = ("Técnico", "amber"),
        [Role.FUNCIONARIO] = ("Funcionário", "gray"),

        [Prioridade.BAIXA] = ("Baixa", "gray"),
        [Prioridade.MEDIA] = ("Média", "blue"),
        [Prioridade.ALTA] = ("Alta", "amber"),
        [Prioridade.URGENTE] = ("Urgente", "red"),

        [EstadoSolicitacao.ABERTA] = ("Aberta", "blue"),
        [EstadoSolicitacao.EM_ATENDIMENTO] = ("Em atendimento", "amber"),
        [EstadoSolicitacao.AGUARDA_VALIDACAO] = ("Aguarda validação", "purple"),
        [EstadoSolicitacao.RESOLVIDA] = ("Resolvida", "green"),
        [EstadoSolicitacao.FECHADA] = ("Fechada", "gray"),

        [EstadoOrdem.EM_DIAGNOSTICO] = ("Diagnóstico", "amber"),
        [EstadoOrdem.EM_REPARACAO] = ("Reparação", "blue"),
        [EstadoOrdem.AGUARDA_VALIDACAO] = ("Validação", "purple"),
        [EstadoOrdem.RESOLVIDO] = ("Concluído", "green"),

        [EstadoDispositivo.ATIVO] = ("Ativo", "green"),
        [EstadoDispositivo.MANUTENCAO] = ("Manutenção", "amber"),
        [EstadoDispositivo.INATIVO] = ("Inativo", "gray"),

        [EstadoInstancia.INSTALADO] = ("Instalado", "blue"),

        [EstadoUnidade.DISPONIVEL] = ("Disponível", "green"),
        [EstadoUnidade.RESERVADA] = ("Reservada", "amber"),
        [EstadoUnidade.AVARIADA] = ("Avariada", "red"),
        [EstadoUnidade.INSTALADA] = ("Instalada", "blue"),

        [TipoMovimento.ENTRADA] = ("Entrada", "green"),
        [TipoMovimento.SAIDA] = ("Saída", "red"),
        [TipoMovimento.TRANSFERENCIA] = ("Transferência", "purple"),
        [TipoMovimento.RESERVA] = ("Reserva", "amber"),
        [TipoMovimento.INSTALACAO] = ("Instalação", "blue"),

        [EstadoRequisicao.PENDENTE] = ("Pendente", "amber"),
        [EstadoRequisicao.APROVADA] = ("Aprovada", "blue"),
        [EstadoRequisicao.RECUSADA] = ("Recusada", "red"),
        [EstadoRequisicao.ENTREGUE] = ("Entregue", "green"),

        [StatusRelatorio.rascunho] = ("Rascunho", "gray"),
        [StatusRelatorio.finalizado] = ("Finalizado", "green"),
        [StatusRelatorio.aprovado] = ("Aprovado", "blue"),

        [AcaoRelatorio.CRIADO] = ("Criado", "gray"),
        [AcaoRelatorio.EDITADO] = ("Campo editado", "blue"),
        [AcaoRelatorio.FINALIZADO] = ("Finalizado", "green"),
        [AcaoRelatorio.REABERTO] = ("Reaberto", "amber"),
        [AcaoRelatorio.APROVADO] = ("Aprovado", "purple"),

        [TipoHistoricoOrdem.ASSUMIDA] = ("Assumida", "blue"),
        [TipoHistoricoOrdem.DIAGNOSTICO] = ("Diagnóstico", "amber"),
        [TipoHistoricoOrdem.SOLUCAO] = ("Solução proposta", "purple"),
        [TipoHistoricoOrdem.ACEITE] = ("Aceite", "green"),
        [TipoHistoricoOrdem.REJEITADA] = ("Rejeitada", "red"),
        [TipoHistoricoOrdem.COMENTARIO] = ("Comentário", "gray"),
        [TipoHistoricoOrdem.REATRIBUIDA] = ("Reatribuída", "blue"),
    };

    public DominioDto Obter()
    {
        var s = settings.Value;
        var enums = new Dictionary<string, List<ValorDominioDto>>
        {
            [nameof(Role)] = Valores<Role>(),
            [nameof(Prioridade)] = Valores<Prioridade>(),
            [nameof(EstadoSolicitacao)] = Valores<EstadoSolicitacao>(),
            [nameof(EstadoOrdem)] = Valores<EstadoOrdem>(),
            [nameof(EstadoDispositivo)] = Valores<EstadoDispositivo>(),
            [nameof(EstadoInstancia)] = Valores<EstadoInstancia>(),
            [nameof(EstadoUnidade)] = Valores<EstadoUnidade>(),
            [nameof(TipoMovimento)] = Valores<TipoMovimento>(),
            [nameof(EstadoRequisicao)] = Valores<EstadoRequisicao>(),
            [nameof(StatusRelatorio)] = Valores<StatusRelatorio>(),
            [nameof(AcaoRelatorio)] = Valores<AcaoRelatorio>(),
            [nameof(TipoHistoricoOrdem)] = Valores<TipoHistoricoOrdem>(),
        };
        return new DominioDto(enums, s.Categorias, s.TiposDispositivo, s.TiposComponente, s.SlaHoras);
    }

    private static List<ValorDominioDto> Valores<T>() where T : struct, Enum
        => Enum.GetValues<T>()
            .Select(v => Textos.TryGetValue(v, out var t)
                ? new ValorDominioDto(v.ToString(), t.Rotulo, t.Cor)
                : new ValorDominioDto(v.ToString(), v.ToString(), "gray"))
            .ToList();
}
