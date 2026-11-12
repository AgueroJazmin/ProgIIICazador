using System.ComponentModel.DataAnnotations;

namespace ProgIIICazador.Web.Modules.Administracion.ViewModels;

public class LoginViewModel
{
    [Required] public string Usuario { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Contrasena { get; set; } = string.Empty;
}