
using MinimalApi.Dominio.Enuns;

namespace MinimalApi.Dominio.ModelViews;

public record AdministradorModelView
{

    public int id { get; set; } = default!;
    public string email { get; set; } = default!;
    //  public string password { get; set; } = default!;
    public string perfil { get; set; } = default!;


}