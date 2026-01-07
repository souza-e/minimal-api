
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using minimalAPI.DTOs;
using MinimalApi.Dominio.Interfaces;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Infraestrutura.Db;

namespace MinimalApi.Dominio.Servicos;

public class VeiculoServico : IVeiculoServico
{
    private readonly DbContexto _dbContexto;
    public VeiculoServico(DbContexto db)
    {
        _dbContexto = db;
    }

    public void Adicionar(Veiculo veiculo)
    {
        _dbContexto.Veiculos.Add(veiculo);
        _dbContexto.SaveChanges();

    }

    public void Atualizar(Veiculo veiculo)
    {
        _dbContexto.Veiculos.Update(veiculo);
        _dbContexto.SaveChanges();

    }

    public Veiculo? BuscarPorId(int id)
    {
        return _dbContexto.Veiculos.Where(v => v.Id == id).FirstOrDefault();

    }

    public void Deletar(Veiculo veiculo)
    {
        _dbContexto.Veiculos.Remove(veiculo);
        _dbContexto.SaveChanges();

    }

    public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
    {
        var query = _dbContexto.Veiculos.AsQueryable();
        if (!string.IsNullOrEmpty(nome))
        {
            query = query.Where(v => EF.Functions.Like(v.Nome.ToLower(), $"%{nome}%"));
        }

        int pageSize = 10;

        if (pagina != null)

            query = query.Skip(((int)pagina - 1) * pageSize).Take(pageSize);


        return query.ToList();
    }


}

