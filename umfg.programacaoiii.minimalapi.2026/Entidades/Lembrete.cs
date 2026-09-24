namespace umfg.programacaoiii.minimalapi._2026.Entidades;

public sealed class Lembrete
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Descricao { get; private set; } = string.Empty;
    public DateTime DataCadastro { get; private set; } = DateTime.Now;
    public DateTime DataAtualizacao { get; private set; } = DateTime.Now;
    public bool IsAtivo { get; private set; } = true;

    public Lembrete(string descricao)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(descricao);
        Descricao = descricao;
    }
}