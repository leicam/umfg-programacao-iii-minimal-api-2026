using Microsoft.EntityFrameworkCore;

namespace umfg.programacaoiii.minimalapi._2026.Contexto;

//está classe representa o banco de dados
public sealed class MySqlContexto : DbContext
{
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
    }
}
