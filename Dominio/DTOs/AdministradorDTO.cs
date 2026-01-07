using MinimalApi.Dominio.Enuns;

namespace minimalAPI.DTOs;

public class AdministradorDTO
{
    public string email { get; set; } = default!;
    public string password { get; set; } = default!;
    public Perfil? perfil { get; set; } = default!;

}
