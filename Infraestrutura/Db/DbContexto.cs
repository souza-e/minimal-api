using Microsoft.EntityFrameworkCore;
using MinimalAPI.Dominio.Entidades;

namespace MinimalAPI.Infraestrutura.Db;




public class DbContexto : DbContext

{
    // Injeo de dependncia para pegar a string de conexo com o banco de dados
    private readonly IConfiguration _configurationAppSettings;
    public DbContexto(IConfiguration configurationAppSettings)
    {
        _configurationAppSettings = configurationAppSettings;
    }
    // Database context implementation

    //
    public DbSet<Administrador> Administradores { get; set; } = default!;
    public DbSet<Veiculo> Veiculos { get; set; } = default!;



    #region  OnModelCreating
    // esse metdodo serve para criar dados iniciais (seed) na tabela de administrador
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrador>().HasData(
            new Administrador
            {
                Id = 1,
                Email = "administrador@teste.com",
                Senha = "123456",
                Perfil = "Adm"


            }
        );

    }

    #endregion


    // esse metdodo serve para configurar a conexo com o banco de dados
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var stringConexao = _configurationAppSettings.GetConnectionString("sqlserver")?.ToString();

            if (!string.IsNullOrEmpty(stringConexao))
            {
                optionsBuilder.UseSqlServer(stringConexao);
            }

        }


    }
}