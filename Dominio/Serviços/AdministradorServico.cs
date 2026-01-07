
using System.Data.Common;
using minimalAPI.DTOs;
using MinimalApi.Dominio.Interfaces;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Infraestrutura.Db;

namespace MinimalApi.Dominio.Servicos;

public class AdministradorServico : IAdministradorServico
{
    private readonly DbContexto _dbContexto;
    public AdministradorServico(DbContexto db)
    {
        _dbContexto = db;
    }

    public Administrador? BuscarPorId(int id)
    {
        return _dbContexto.Administradores.Where(v => v.Id == id).FirstOrDefault();
    }

    public Administrador Incluir(Administrador administrador)
    {
        _dbContexto.Administradores.Add(administrador);
        _dbContexto.SaveChanges();
        return administrador;
    }

    public Administrador? Login(LoginDTO loginDTO)
    {
        var adm = _dbContexto.Administradores.Where(a => a.Email == loginDTO.email && a.Senha == loginDTO.password).FirstOrDefault();
        return adm;



    }

    public List<Administrador> Todos(int? pagina)
    {
        var query = _dbContexto.Administradores.AsQueryable();


        int pageSize = 10;

        if (pagina != null)

            query = query.Skip(((int)pagina - 1) * pageSize).Take(pageSize);


        return query.ToList();
    }
}

