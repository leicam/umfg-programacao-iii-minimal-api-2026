using Microsoft.EntityFrameworkCore;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using umfg.programacaoiii.minimalapi._2026.Contexto;
using umfg.programacaoiii.minimalapi._2026.DTO;
using umfg.programacaoiii.minimalapi._2026.Entidades;

namespace umfg.programacaoiii.minimalapi._2026;

public class Program
{    
    //método de startup do projeto
    public static void Main(string[] args)
    {
        //variavel padrão do .net permite definir as caracteristicas que a api irá ter
        //ex: autenticação e autorização via JWT / qual banco de dados
        var builder = WebApplication.CreateBuilder(args);
        var connectionString = builder.Configuration["ConnectionString"] ?? string.Empty;

        //aqui configuramos a conexão da API com o banco de dados
        builder.Services
            .AddDbContext<MySqlContexto>(options => options.UseMySQL(connectionString));

        //implementacao do CORS para permitir que a aplicação frontend acesse a API (localhost)
        //no mundo real, restrinja o acesso apenas para o dominio do frontend conhecido. Ex.: .WithOrigins("https://app.empresa.com.br")
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        //aqui a aplicação é construída
        var app = builder.Build();

        app.MapPost("/lembretes", async (Lembrete lembrete, MySqlContexto contexto) =>
        {
            await contexto.Lembrete.AddAsync(lembrete);
            await contexto.SaveChangesAsync();

            return Results.Created($"/lembretes/{lembrete.Id}", lembrete);
        });

        app.MapGet("/lembretes/{id}", async (string id, MySqlContexto contexto) =>
        {
            if (string.IsNullOrWhiteSpace(id))
                return Results.BadRequest("id invalido!");

            return await contexto.Lembrete.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id) && x.IsAtivo) 
            is Lembrete lembrete ? Results.Ok(lembrete) : Results.NotFound();
        });

        app.MapGet("/lembretes", async (MySqlContexto contexto) =>
        {
            return await contexto.Lembrete.Where(x => x.IsAtivo).ToListAsync();            
        });

        app.MapGet("/lembretes/filtro", async ([AsParameters] LembreteQueryDTO.LembreteQueryRequestDTO dto, MySqlContexto contexto) =>
        {
            if (dto.NumeroPagina <= 0 || dto.TamanhoPagina <= 0)
                return Results.BadRequest("Página e tamanho da página são obrigatórios.");

            var (lembretes, total) = await FiltrarLembretes(dto, contexto);

            var resultado = new LembreteQueryDTO.LembreteQueryResponseDTO()
            {
                Total = total,
                NumeroPagina = dto.NumeroPagina,
                TamanhoPagina = dto.TamanhoPagina,
                Lembretes = lembretes,
            };

            return Results.Ok(resultado);
        });

        Settings.License = LicenseType.Community;

        app.MapGet("/lembretes/imprimir", async ([AsParameters] LembreteQueryDTO.LembreteQueryRequestDTO dto, MySqlContexto contexto) =>
        {
            if (dto.NumeroPagina <= 0 || dto.TamanhoPagina <= 0)
                return Results.BadRequest("Página e tamanho da página são obrigatórios.");

            var (lembretes, total) = await FiltrarLembretes(dto, contexto);
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);

                    page.Header()
                        .Text("Relatório de Lembretes")
                        .SemiBold()
                        .FontSize(20)
                        .FontColor(Colors.Blue.Medium);

                    page
                        .Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Spacing(10);
                            column
                                .Item()
                                .Text($"Gerado em {DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}");

                            column
                                .Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.ConstantColumn(110);
                                        columns.ConstantColumn(110);
                                        columns.ConstantColumn(80);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Descrição").SemiBold();
                                        header.Cell().Text("Cadastro").SemiBold();
                                        header.Cell().Text("Atualização").SemiBold();
                                        header.Cell().Text("Status").SemiBold();
                                    });

                                    foreach (var lembrete in lembretes)
                                    {
                                        table.Cell().Text(lembrete.Descricao).FontSize(10);
                                        table.Cell().Text(lembrete.DataCadastro.ToString("dd/MM/yyyy hh:mm:ss")).FontSize(10);
                                        table.Cell().Text(lembrete.DataAtualizacao.ToString("dd/MM/yyyy hh:mm:ss")).FontSize(10);
                                        table.Cell().Text(lembrete.IsAtivo ? "ATIVO" : "INATIVO").FontSize(10).FontColor(lembrete.IsAtivo ? Color.FromHex("#00FF00") : Color.FromHex("#FF0000"));
                                    }
                                });
                        });

                    page.Footer().AlignCenter().Text(x => x.CurrentPageNumber());
                });
            });

            return Results.File(documento.GeneratePdf(), "application/pdf", "relatorio-lembretes.pdf");
        });

        app.MapDelete("/lembretes/{id}", async (string id, MySqlContexto contexto) =>
        {
            var lembrete = await contexto.Lembrete.FindAsync(Guid.Parse(id));

            //conceito de fail first
            if (lembrete is null)
                return Results.NotFound();

            lembrete.SetAtivo(false);
            lembrete.Update();

            contexto.Lembrete.Update(lembrete);
            await contexto.SaveChangesAsync();

            return Results.NoContent();
        });

        app.MapPut("/lembretes/{id}", async (string id, LembreteDTO dto, MySqlContexto contexto) =>
        {
            var lembreteExistente = await contexto.Lembrete.FindAsync(Guid.Parse(id));

            if (lembreteExistente is null)
                return Results.NotFound();

            lembreteExistente.SetDescricao(dto.Descricao);
            lembreteExistente.Update();

            contexto.Lembrete.Update(lembreteExistente);
            await contexto.SaveChangesAsync();

            return Results.Ok(lembreteExistente);
        });

        //aqui habilitamos as funcionalidades da api
        app.UseHttpsRedirection();

        //habilitar o uso da regra de CORS
        app.UseCors("Frontend");

        //a api é iniciada de fato
        app.Run();
        //tudo que for colocado após o run será executado somente quando a api for parada
    }

    private static async Task<(List<Lembrete>, long)> FiltrarLembretes(LembreteQueryDTO.LembreteQueryRequestDTO dto, MySqlContexto contexto)
    {        
        var query = contexto.Lembrete
            .AsNoTracking()
            .Where(x => x.IsAtivo);

        //busca por descrição com tratamento de caracteres especiais para LIKE
        if (!string.IsNullOrWhiteSpace(dto.Descricao))
        {
            var termo = dto.Descricao.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            query = query.Where(x => EF.Functions.Like(x.Descricao, $"%{termo}%"));
        }

        // Filtros opcionais
        if (dto.DataCadastroInicial > DateTime.MinValue)
            query = query.Where(x => x.DataCadastro >= dto.DataCadastroInicial);

        if (dto.DataCadastroFinal < DateTime.MaxValue)
            query = query.Where(x => x.DataCadastro < dto.DataCadastroFinal.Date.AddDays(1)); // inclui o dia inteiro

        if (dto.DataAtualizacaoInicial > DateTime.MinValue)
            query = query.Where(x => x.DataAtualizacao >= dto.DataAtualizacaoInicial);

        if (dto.DataAtualizacaoFinal < DateTime.MaxValue)
            query = query.Where(x => x.DataAtualizacao < dto.DataAtualizacaoFinal.Date.AddDays(1)); // inclui o dia inteiro

        //ordenar sempre do mais recente para o mais antigo
        if (dto.Direcao.ToUpper() != "ASC" && dto.Direcao.ToUpper() != "DESC")
            dto.Direcao = "DESC";

        //ordenação
        query = dto.Direcao.ToUpper() == "DESC" ? query.OrderByDescending(x => dto.Ordem) : query.OrderBy(x => dto.Ordem);

        // paginação
        var pular = (dto.NumeroPagina - 1) * dto.TamanhoPagina;
        var lembretes = await query.Skip(pular).Take(dto.TamanhoPagina).ToListAsync();
        var total = await query.CountAsync();

        return (lembretes, total);
    }
}