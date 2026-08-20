using System;
using System.Linq;
using System.Threading.Tasks;
using DocesCabana.Domain;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;
using DocesCabana.Infrastructure.DatabaseContext;
using DocesCabana.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocesCabana.MVC.Helpers;

public static class DbInitializer
{
    public const string EmailAdministrador = "admin@docescabana.com.br";

    // Taxonomia real da loja (spec 012, levantada em 2026-08-19). "Doces
    // Caseiros" e "Doces Zero" se fundiram em "Doces" — a distinção "zero"
    // virou Produto.SemAcucar (RN-04), não subcategoria própria, porque
    // "Barras" e "Potes" existiam nas duas listas originais.
    private static readonly (string Categoria, string[] Subcategorias)[] Taxonomia =
    [
        ("Doces", ["Barras", "Bolachas / Rosquinhas", "Box", "Combos", "Compotas", "Cappuccino", "Latas", "Palhas", "Potes", "Quindim", "Raspa de Tachos", "Sorvetes"]),
        ("Empório", ["Café", "Cappuccino", "Charcutaria", "Croissant", "Desidratados", "Geleias", "Manteiga", "Mel", "Molho", "Risotto"]),
        ("Adega", ["Cachaça", "Licor", "Licor Caseiro", "Vinhos"]),
        ("Souvenir", ["Bijuterias", "Canecas", "Chaveiros", "Kits", "Pelúcia"]),
    ];

    // Subcategorias de Doces que vinham da antiga "Doces Zero" — produtos
    // gerados nelas nascem SemAcucar (spec 012 §11).
    private static readonly HashSet<string> SubcategoriasDeOrigemZero = ["Barras", "Combos", "Cappuccino", "Potes", "Sorvetes"];

    private const int ProdutosPorCategoria = 25;

    // Semente padrão do gerador de avaliações (spec 014, RF-14) — fixa para
    // que recriar a base produza sempre as mesmas notas, nos mesmos
    // produtos (CA-16). É só um número arbitrário, não uma data especial.
    private const int SementeAvaliacoesMock = 20260820;

    // Cerca de 30% dos produtos ficam sem avaliação nenhuma (RF-13) — é o
    // único jeito de exercitar, em demonstração, o ramo do repositório que
    // joga produto sem nota para o fim da ordenação por avaliação
    // (ProdutoRepository.AplicarOrdenacao, "?? -1").
    private const double ProbabilidadeDeReceberAvaliacao = 0.70;

    private static readonly string?[] ComentariosDeExemplo =
    [
        "Simplesmente maravilhoso, super recomendo!",
        "Muito bom, só achei um pouco doce demais para o meu gosto.",
        "Bom, mas esperava mais pelo preço.",
        "Chegou rápido e bem embalado.",
        "Já é a segunda vez que compro, não decepciona.",
        "Gostei bastante, vou comprar de novo.",
        "Dentro do esperado.",
        null,
        null,
    ];

    // Reaproveitadas em ciclo para as 100 linhas do mock — não há 100 fotos
    // reais disponíveis; a taxonomia é que precisa ser real (spec 012 §11).
    private static readonly string[] ImagensDeExemplo =
    [
        "https://drive.google.com/file/d/1q2pScc0aQL8V8w3PeffOQsAfo6_-YxYk/preview",
        "https://drive.google.com/file/d/1nqCmg7DPQQhUhFKQ12b21XMQSVTYWSuT/preview",
        "https://drive.google.com/file/d/1YfVBWgDdQ4XVB1tsSY7yDOssljtJlIuZ/preview",
        "https://drive.google.com/file/d/1jFKyz7UdjlYL6gRJbzi2N4Pm3IsIKrZ4/preview",
        "https://drive.google.com/file/d/1Hq0GQ6axWc-iRPOheT4vBYa0s6MU-q6C/preview",
        "https://drive.google.com/file/d/1bfDl0VMyHkHzxOxluuho3-7EERjjdDa2/preview",
    ];

    public static void Migrar(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DocesCabanaDbContext>();

        context.Database.Migrate();
    }

    public static async Task Semear(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DocesCabanaDbContext>();

        // Se já houver produtos, não faz nada
        if (!context.Produtos.Any())
        {
            // Ordem obrigatória: categoria -> subcategoria -> produto. A FK de
            // Produto.SubcategoriaId é enforçada desde a spec 003; semear fora
            // dessa ordem falha.
            var categorias = new Dictionary<string, Categoria>();
            var subcategoriasPorCategoria = new Dictionary<string, Dictionary<string, Subcategoria>>();

            foreach (var (nomeCategoria, nomesSubcategorias) in Taxonomia)
            {
                var categoria = new Categoria(nomeCategoria);
                categorias[nomeCategoria] = categoria;
                context.Categorias.Add(categoria);

                var subcategoriasDaCategoria = new Dictionary<string, Subcategoria>();
                foreach (var nomeSubcategoria in nomesSubcategorias)
                {
                    var subcategoria = new Subcategoria(categoria.CategoriaId, nomeSubcategoria);
                    subcategoriasDaCategoria[nomeSubcategoria] = subcategoria;
                    context.Subcategorias.Add(subcategoria);
                }
                subcategoriasPorCategoria[nomeCategoria] = subcategoriasDaCategoria;
            }
            await context.SaveChangesAsync();

            var produtosSeed = GerarProdutosMock(subcategoriasPorCategoria);
            context.Produtos.AddRange(produtosSeed);
            await context.SaveChangesAsync();

            // Avaliações de exemplo no primeiro produto (Raspa Tacho, com
            // descrição curada), para a tela ter conteúdo real em
            // desenvolvimento (spec 008), sem depender do administrador
            // semeado existir. Devolve o elenco de clientes fictícios, para
            // o restante do catálogo ser avaliado por eles a seguir (spec 014).
            var usuarioIds = await SemearAvaliacoesDeExemplo(scope.ServiceProvider, context, produtosSeed[0].ProdutoId);

            // O resto do catálogo (spec 014, RF-12/RF-13) — o primeiro
            // produto fica de fora porque já recebeu as avaliações curadas
            // acima; gerar de novo para ele colidiria com o índice único de
            // Avaliacao(UsuarioId, ProdutoId) sempre que sorteasse um dos
            // três autores que já o avaliaram.
            if (usuarioIds.Count > 0)
            {
                var avaliacoesGeradas = GerarAvaliacoesMock(produtosSeed.Skip(1).ToList(), usuarioIds);
                context.Avaliacoes.AddRange(avaliacoesGeradas);
                await context.SaveChangesAsync();
            }
        }

        await SemearAdministrador(scope.ServiceProvider);
    }

    // 100 produtos, 25 por categoria (spec 012 §11) — distribuídos em
    // rodízio pelas subcategorias da categoria, para que toda subcategoria
    // tenha ao menos um produto e o catálogo feche em 3 páginas por
    // categoria (12 por página). Prova a mecânica de filtro e paginação;
    // não é o catálogo real da loja — isso é backlog (spec 012 §8).
    private static List<Produto> GerarProdutosMock(Dictionary<string, Dictionary<string, Subcategoria>> subcategoriasPorCategoria)
    {
        var produtos = new List<Produto>();

        // O primeiro produto é curado, não gerado: mantém a descrição real
        // que a spec 008 usa para demonstrar a página do produto, e serve de
        // alvo às avaliações de exemplo abaixo.
        var raspaDeTachos = subcategoriasPorCategoria["Doces"]["Raspa de Tachos"];
        produtos.Add(new Produto(
            raspaDeTachos.SubcategoriaId, "Raspa Tacho", 19.99m, ImagensDeExemplo[0],
            descricao: "Um clássico caramelizado no ponto certo, com aquele toque de queima que só o tacho de cobre dá. Feito artesanalmente em pequenos lotes, sem conservantes."));

        var indiceImagem = 1;
        foreach (var (nomeCategoria, nomesSubcategorias) in Taxonomia)
        {
            var subcategorias = subcategoriasPorCategoria[nomeCategoria];
            var quantidadeAGerar = nomeCategoria == "Doces" ? ProdutosPorCategoria - 1 : ProdutosPorCategoria;

            for (var i = 1; i <= quantidadeAGerar; i++)
            {
                var nomeSubcategoria = nomesSubcategorias[(i - 1) % nomesSubcategorias.Length];
                var subcategoria = subcategorias[nomeSubcategoria];

                var status = ProdutoStatus.Ativo;
                // Ao menos um inativo e um fora de estoque, para os
                // critérios que dependem deles terem o que exercitar
                // (spec 012, CA-20/CA-21).
                if (nomeCategoria == "Doces" && i == 2)
                    status = ProdutoStatus.Inativo;
                else if (nomeCategoria == "Doces" && i == 3)
                    status = ProdutoStatus.ForaDeEstoque;

                var semAcucar = nomeCategoria == "Doces" && SubcategoriasDeOrigemZero.Contains(nomeSubcategoria);

                var preco = Math.Round(8m + (i * 1.7m % 35m), 2);
                var imagem = ImagensDeExemplo[indiceImagem % ImagensDeExemplo.Length];
                indiceImagem++;

                produtos.Add(new Produto(
                    subcategoria.SubcategoriaId,
                    $"{nomeSubcategoria} {i}",
                    preco,
                    imagem,
                    status,
                    semAcucar: semAcucar));
            }
        }

        return produtos;
    }

    // Gera avaliações para a maior parte dos produtos, deixando parte sem
    // nenhuma (spec 014, RF-12/RF-13). Sem acesso a banco de propósito — é o
    // que permite chamar duas vezes com a mesma semente e comparar o
    // resultado (RF-14, CA-16), sem precisar de um SQLite em memória para
    // testar geração pura.
    //
    // RN-01 (uma avaliação por pessoa por produto) é respeitada por
    // construção: os avaliadores de um produto vêm de um embaralhamento sem
    // reposição da lista de usuários, nunca da mesma pessoa duas vezes no
    // mesmo produto.
    internal static List<Avaliacao> GerarAvaliacoesMock(
        IReadOnlyList<Produto> produtos,
        IReadOnlyList<Guid> usuarioIds,
        int semente = SementeAvaliacoesMock)
    {
        var aleatorio = new Random(semente);
        var avaliacoes = new List<Avaliacao>();

        foreach (var produto in produtos)
        {
            if (aleatorio.NextDouble() >= ProbabilidadeDeReceberAvaliacao)
                continue;

            var quantidade = Math.Min(aleatorio.Next(1, 5), usuarioIds.Count);
            var avaliadores = usuarioIds
                .OrderBy(_ => aleatorio.Next())
                .Take(quantidade);

            foreach (var usuarioId in avaliadores)
            {
                var nota = SortearNotaEnviesada(aleatorio);
                var comentario = ComentariosDeExemplo[aleatorio.Next(ComentariosDeExemplo.Length)];
                avaliacoes.Add(new Avaliacao(usuarioId, produto.ProdutoId, nota, comentario));
            }
        }

        return avaliacoes;
    }

    // Enviesada para cima — loja real tem média perto de 4, não distribuição
    // uniforme entre 1 e 5 (spec 014, plano §1).
    private static byte SortearNotaEnviesada(Random aleatorio) =>
        aleatorio.NextDouble() switch
        {
            < 0.45 => 5,
            < 0.75 => 4,
            < 0.90 => 3,
            < 0.97 => 2,
            _ => 1,
        };

    private static async Task<List<Guid>> SemearAvaliacoesDeExemplo(IServiceProvider serviceProvider, DocesCabanaDbContext context, Guid produtoId)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ContaDeAcesso>>();

        // Elenco de 8 (spec 014, RF-12): com só 3 pessoas, nenhum produto
        // podia ter mais de 3 avaliações, e a regra de "ninguém avalia duas
        // vezes" (RN-01) mais a de "ninguém vota na própria avaliação"
        // (RN-07 da 008) estreitavam demais as combinações possíveis.
        var clientes = new (string Email, string Nome, string Cpf)[]
        {
            ("cliente1.seed@docescabana.com.br", "Zeca Pagodinho", "87654321937"),
            ("cliente2.seed@docescabana.com.br", "Marina Alves", "11144477735"),
            ("cliente3.seed@docescabana.com.br", "João Pedro", "39053344705"),
            // Não "52998224725" — é o CPF do administrador semeado
            // (SemearAdministrador), e o índice único de Usuario.CPF não
            // aceita repetição.
            ("cliente4.seed@docescabana.com.br", "Fernanda Lima", "45678912364"),
            ("cliente5.seed@docescabana.com.br", "Carlos Eduardo", "01234567890"),
            ("cliente6.seed@docescabana.com.br", "Beatriz Souza", "12345678909"),
            ("cliente7.seed@docescabana.com.br", "Rafael Mendes", "98765432100"),
            ("cliente8.seed@docescabana.com.br", "Larissa Costa", "11223344517"),
        };

        var usuarioIds = new List<Guid>();
        foreach (var (email, nome, cpf) in clientes)
        {
            var conta = new ContaDeAcesso(email);
            var resultado = await userManager.CreateAsync(conta, "SenhaSeed@123");
            if (!resultado.Succeeded)
                continue;

            var usuario = new Usuario(conta.Id, nome, cpf, "14999998888", new DateTime(1995, 5, 20));
            context.Usuarios.Add(usuario);
            usuarioIds.Add(conta.Id);
        }
        await context.SaveChangesAsync();

        if (usuarioIds.Count < 3)
            return usuarioIds;

        var avaliacaoMaisVotada = new Avaliacao(usuarioIds[0], produtoId, 5,
            "Simplesmente o melhor doce que já comi. Chegou rápido e bem embalado, recomendo demais!");
        var avaliacaoMediana = new Avaliacao(usuarioIds[1], produtoId, 4, "Muito bom, só achei um pouco doce demais para o meu gosto.");
        var avaliacaoSemVoto = new Avaliacao(usuarioIds[2], produtoId, 3, "Bom, mas esperava mais pelo preço.");

        context.Avaliacoes.AddRange(avaliacaoMaisVotada, avaliacaoMediana, avaliacaoSemVoto);
        await context.SaveChangesAsync();

        context.VotosUteis.Add(new VotoUtil(avaliacaoMaisVotada.AvaliacaoId, usuarioIds[1]));
        context.VotosUteis.Add(new VotoUtil(avaliacaoMaisVotada.AvaliacaoId, usuarioIds[2]));
        context.VotosUteis.Add(new VotoUtil(avaliacaoMediana.AvaliacaoId, usuarioIds[2]));
        await context.SaveChangesAsync();

        return usuarioIds;
    }

    private static async Task SemearAdministrador(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ContaDeAcesso>>();
        var context = serviceProvider.GetRequiredService<DocesCabanaDbContext>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        if (!await roleManager.RoleExistsAsync(Papeis.Administrador))
            await roleManager.CreateAsync(new IdentityRole<Guid>(Papeis.Administrador));

        if (await userManager.FindByEmailAsync(EmailAdministrador) is not null)
            return;

        // A senha do administrador semeado vem de user secret, nunca literal
        // no código. Sem ela configurada, nenhum admin é criado — a aplicação
        // sobe do mesmo jeito, só sem conta administrativa pronta.
        var senha = configuration["Admin:SenhaInicial"];
        if (string.IsNullOrWhiteSpace(senha))
            return;

        var conta = new ContaDeAcesso(EmailAdministrador);
        var resultado = await userManager.CreateAsync(conta, senha);
        if (!resultado.Succeeded)
            return;

        var administrador = new Usuario(
            conta.Id,
            "Administrador Doces Cabana",
            "52998224725",
            "14999999999",
            new DateTime(1990, 1, 1));

        context.Usuarios.Add(administrador);
        await context.SaveChangesAsync();

        await userManager.AddToRoleAsync(conta, Papeis.Administrador);
    }
}
