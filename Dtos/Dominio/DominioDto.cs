namespace Novati.API.Dtos.Dominio;

/// <summary>Um valor de enum com o rótulo em português e a cor usada nos distintivos da UI.</summary>
/// <param name="Valor">Texto exato do enum (o que a API envia e recebe).</param>
/// <param name="Rotulo">Rótulo para mostrar ao utilizador.</param>
/// <param name="Cor">Tom do distintivo: blue, green, amber, red, purple ou gray.</param>
public record ValorDominioDto(string Valor, string Rotulo, string Cor);

/// <summary>Tudo o que o front precisa para mostrar e validar valores de domínio.</summary>
/// <param name="Enums">Por nome do enum (Role, Prioridade, EstadoSolicitacao…), os valores pela ordem do fluxo.</param>
/// <param name="Categorias">Categorias de solicitação.</param>
/// <param name="TiposDispositivo">Tipos de modelo de dispositivo.</param>
/// <param name="TiposComponente">Tipos de modelo de componente.</param>
/// <param name="SlaHoras">Horas de SLA por prioridade.</param>
public record DominioDto(
    Dictionary<string, List<ValorDominioDto>> Enums,
    List<string> Categorias,
    List<string> TiposDispositivo,
    List<string> TiposComponente,
    Dictionary<string, int> SlaHoras);
