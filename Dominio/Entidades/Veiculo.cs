using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinimalAPI.Dominio.Entidades;

public class Veiculo
{

    //Neste trecho de cdigo, estamos definindo a entidade Veiculo com suas propriedades e atributos de validao de dados
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Indica que a propriedade Id  a chave primria e seu valor  gerado automaticamente pelo banco de dados

    public int Id { get; set; } = default!;

    [Required] // Indica que o campo Nome  obrigatrio (no pode ser nulo, not null)
    [StringLength(150)]
    public string Nome { get; set; } = default!;

    [Required]
    [StringLength(100)]
    public string Marca { get; set; } = default!;

    [Required]
    public int Ano { get; set; } = default!;
}
