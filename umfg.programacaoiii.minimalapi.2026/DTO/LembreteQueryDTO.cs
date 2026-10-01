using umfg.programacaoiii.minimalapi._2026.Entidades;

namespace umfg.programacaoiii.minimalapi._2026.DTO;

public sealed class LembreteQueryDTO
{
    public sealed class LembreteQueryRequestDTO
    {
        private const string C_ORDEM_PADRAO = "DataCadastro";
        private const string C_DIRECAO_PADRAO = "DESC";

        /// <summary>
        /// Padrão uma página
        /// </summary>        
        public int NumeroPagina { get; set; } = 1;

        /// <summary>
        /// Padrão dez registros por página
        /// </summary>        
        public int TamanhoPagina { get; set; } = 10;

        /// <summary>
        /// Filtro para ordenação dos registros retornados. Padrão Nome.
        /// </summary>        
        public string Ordem { get; set; } = C_ORDEM_PADRAO;

        /// <summary>
        /// Direção (DESC - ASC) da ordenação dos registros retornados. Padrão DESC.
        /// </summary>        
        public string Direcao { get; set; } = C_DIRECAO_PADRAO;
        
        public string Descricao { get; set; } = string.Empty;
        
        public DateTime DataCadastroInicial { get; set; } = DateTime.MinValue;
        
        public DateTime DataCadastroFinal { get; set; } = DateTime.MinValue;

        public DateTime DataAtualizacaoInicial { get; set; } = DateTime.MaxValue;

        public DateTime DataAtualizacaoFinal { get; set; } = DateTime.MaxValue;
    }

    public sealed class LembreteQueryResponseDTO
    {
        public ICollection<Lembrete> Lembretes { get; set; } = [];

        public long Total { get; set; } = 0;

        public int NumeroPagina { get; set; } = 0;

        public int TamanhoPagina { get; set; } = 0;
    }
}