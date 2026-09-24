namespace umfg.programacaoiii.minimalapi._2026;

public class Program
{
    //método de startup do projeto
    public static void Main(string[] args)
    {
        //classe padrão do .net permite definir as caracteristicas que a api irá ter
        //ex: autenticação e autorização via JWT / qual banco de dados
        var builder = WebApplication.CreateBuilder(args);

        //aqui a aplicação é construída
        var app = builder.Build();

        //aqui habilitamos as funcionalidades da api
        app.UseHttpsRedirection();

        //a api é iniciada de fato
        app.Run();
        //tudo que for colocado após o run será executado somente quando a api for parada
    }
}