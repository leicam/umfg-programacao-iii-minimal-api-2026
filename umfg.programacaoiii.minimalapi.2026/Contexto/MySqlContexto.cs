using Microsoft.EntityFrameworkCore;
using umfg.programacaoiii.minimalapi._2026.Entidades;

namespace umfg.programacaoiii.minimalapi._2026.Contexto;

//está classe representa o banco de dados
public sealed class MySqlContexto : DbContext
{
	public DbSet<Lembrete> Lembrete { get; set; }

	//base acesso um construtor da classe pai, o this da propria classe
	public MySqlContexto(DbContextOptions<MySqlContexto> options)
		: base(options)
	{
		if (Database.GetPendingMigrations().Any())
			Database.Migrate();
	}

	//definimos o mapeamento de classes (entidades) em tabelas
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Lembrete>().HasKey(x => x.Id);
		modelBuilder.Entity<Lembrete>().Property(x => x.Descricao).IsRequired();
		modelBuilder.Entity<Lembrete>().Property(x => x.DataCadastro).IsRequired();
		modelBuilder.Entity<Lembrete>().Property(x => x.DataAtualizacao).IsRequired();
		modelBuilder.Entity<Lembrete>().Property(x => x.IsAtivo).IsRequired();
    }
}
