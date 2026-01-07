using minimalAPI.DTOs;
using MinimalAPI.Dominio.Entidades;

namespace MinimalApi.Dominio.Interfaces;



public interface IVeiculoServico
{
      //string? nome. significa que pode ser nulo
      List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null);
      Veiculo? BuscarPorId(int id);
      void Adicionar(Veiculo veiculo);
      void Atualizar(Veiculo veiculo);
      void Deletar(Veiculo veiculo);
}
