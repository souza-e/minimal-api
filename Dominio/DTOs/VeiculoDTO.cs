

// Indica que o campo Nome  obrigatrio (no pode ser nulo, not null)
namespace MinimalApi.DTOs;

public record VeiculoDTO
{


    public string Nome { get; set; } = default!;


    public string Marca { get; set; } = default!;

    public int Ano { get; set; } = default!;

}