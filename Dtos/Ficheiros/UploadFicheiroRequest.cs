using System.ComponentModel.DataAnnotations;

namespace Novati.API.Dtos.Ficheiros;

/// <summary>Upload multipart/form-data com um campo "ficheiro".</summary>
public class UploadFicheiroRequest
{
    [Required(ErrorMessage = "Envie um ficheiro no campo \"ficheiro\".")]
    public IFormFile Ficheiro { get; set; } = null!;
}
