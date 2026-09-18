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

    internal static readonly (string Categoria, string[] Subcategorias)[] Taxonomia =
    [
        ("Doces", ["Barras", "Bolachas / Rosquinhas", "Box", "Combos", "Compotas", "Cappuccino", "Latas", "Palhas", "Potes", "Quindim", "Raspa de Tachos", "Sorvetes"]),
        ("Empório", ["Café", "Cappuccino", "Charcutaria", "Croissant", "Desidratados", "Geleias", "Manteiga", "Mel", "Molho", "Risotto"]),
        ("Adega", ["Cachaça", "Licor", "Licor Caseiro", "Vinhos"]),
        ("Souvenir", ["Bijuterias", "Canecas", "Chaveiros", "Kits", "Pelúcia"]),
    ];

    private static readonly HashSet<string> SubcategoriasDeOrigemZero = ["Barras", "Combos", "Cappuccino", "Potes", "Sorvetes"];

    private const int ProdutosPorCategoria = 25;

    private static readonly Dictionary<string, (decimal Peso, decimal Altura, decimal Largura, decimal Comprimento)> MedidasPorCategoria = new()
    {
        ["Adega"] = (1.200m, 32m, 8m, 8m),       
        ["Doces"] = (0.400m, 12m, 15m, 15m),     
        ["Empório"] = (0.500m, 14m, 10m, 10m),   
        ["Souvenir"] = (0.300m, 20m, 25m, 30m),  
    };

    private const int SementeAvaliacoesMock = 20260820;

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

    private static readonly string[] ImagensDeExemplo =
    [
        "https://mjnlzsucdsxqahabsniy.supabase.co/storage/v1/object/public/images/public/IMG_7382.JPG",
        "https://mjnlzsucdsxqahabsniy.supabase.co/storage/v1/object/public/images/public/IMG_7412.JPG",
        "https://mjnlzsucdsxqahabsniy.supabase.co/storage/v1/object/public/images/public/IMG_7419.JPG",
        "https://mjnlzsucdsxqahabsniy.supabase.co/storage/v1/object/public/images/public/IMG_7422.JPG",
        "https://mjnlzsucdsxqahabsniy.supabase.co/storage/v1/object/public/images/public/IMG_7423.JPG",
        "https://mjnlzsucdsxqahabsniy.supabase.co/storage/v1/object/public/images/public/IMG_7427.JPG",
    ];

    public static void Migrar(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DocesCabanaDbContext>();

        context.Database.Migrate();
    }

    public static async Task PreencherNomesNormalizados(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DocesCabanaDbContext>();

        await PreencherNomesNormalizados(context);
    }

    internal static async Task PreencherNomesNormalizados(DocesCabanaDbContext context)
    {
        var produtosSemNormalizacao = await context.Produtos
            .Where(p => p.NomeNormalizado == "")
            .ToListAsync();

        if (produtosSemNormalizacao.Count == 0)
            return;

        foreach (var produto in produtosSemNormalizacao)
            produto.AlterarNome(produto.Nome);

        await context.SaveChangesAsync();
    }

    public static async Task Semear(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DocesCabanaDbContext>();

        if (!context.Produtos.Any())
        {
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

            var usuarioIds = await SemearAvaliacoesDeExemplo(scope.ServiceProvider, context, produtosSeed[0].ProdutoId);

            if (usuarioIds.Count > 0)
            {
                var avaliacoesGeradas = GerarAvaliacoesMock(produtosSeed.Skip(1).ToList(), usuarioIds);
                context.Avaliacoes.AddRange(avaliacoesGeradas);
                await context.SaveChangesAsync();
            }

            if (usuarioIds.Count > 0)
                await SemearPedidosDeExemplo(context, usuarioIds, produtosSeed);
        }

        await SemearAdministrador(scope.ServiceProvider);
    }

    internal static List<Produto> GerarProdutosMock(Dictionary<string, Dictionary<string, Subcategoria>> subcategoriasPorCategoria)
    {
        var produtos = new List<Produto>();

        var raspaDeTachos = subcategoriasPorCategoria["Doces"]["Raspa de Tachos"];
        var medidasDoces = MedidasPorCategoria["Doces"];
        produtos.Add(new Produto(
            raspaDeTachos.SubcategoriaId, "Raspa Tacho", 19.99m, ImagensDeExemplo[0],
            medidasDoces.Peso, medidasDoces.Altura, medidasDoces.Largura, medidasDoces.Comprimento,
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

                if (nomeCategoria == "Doces" && i == 2)
                    status = ProdutoStatus.Inativo;
                else if (nomeCategoria == "Doces" && i == 3)
                    status = ProdutoStatus.ForaDeEstoque;

                var semAcucar = nomeCategoria == "Doces" && SubcategoriasDeOrigemZero.Contains(nomeSubcategoria);

                var preco = Math.Round(8m + (i * 1.7m % 35m), 2);
                var imagem = ImagensDeExemplo[indiceImagem % ImagensDeExemplo.Length];
                indiceImagem++;

                var medidas = MedidasPorCategoria[nomeCategoria];

                produtos.Add(new Produto(
                    subcategoria.SubcategoriaId,
                    $"{nomeSubcategoria} {i}",
                    preco,
                    imagem,
                    medidas.Peso, medidas.Altura, medidas.Largura, medidas.Comprimento,
                    status,
                    semAcucar: semAcucar));
            }
        }

        return produtos;
    }

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

        var clientes = new (string Email, string Nome, string Cpf)[]
        {
            ("cliente1.seed@docescabana.com.br", "Zeca Pagodinho", "87654321937"),
            ("cliente2.seed@docescabana.com.br", "Marina Alves", "11144477735"),
            ("cliente3.seed@docescabana.com.br", "João Pedro", "39053344705"),
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

    internal static async Task SemearPedidosDeExemplo(DocesCabanaDbContext context, List<Guid> usuarioIds, List<Produto> produtos)
    {
        var enderecoPorUsuario = new Dictionary<Guid, Guid>();
        foreach (var usuarioId in usuarioIds)
        {
            var endereco = new Endereco(usuarioId, "SP", "Lençóis Paulista", "Centro", "17340001", "Rua das Flores", 100);
            endereco.MarcarComoPadrao();
            context.Enderecos.Add(endereco);
            enderecoPorUsuario[usuarioId] = endereco.EnderecoId;
        }
        await context.SaveChangesAsync();

        var maisVendido = produtos[0];
        var segundoMaisVendido = produtos[Math.Min(10, produtos.Count - 1)];
        var poucoVendido = produtos[Math.Min(20, produtos.Count - 1)];

        void CriarPedido(
            Guid usuarioId, (Produto Produto, short Quantidade)[] itens, PedidoStatus status, MetodoPagamento metodo,
            DateTime? data = null)
        {
            var valorDosProdutos = itens.Sum(i => i.Produto.Preco * i.Quantidade);
            const decimal valorDoFrete = 12.90m;

            var pedido = new Pedido(
                usuarioId, enderecoPorUsuario[usuarioId], valorDosProdutos + valorDoFrete, valorDoFrete,
                "Correios", "PAC", 3, 7);

            foreach (var (produto, quantidade) in itens)
                pedido.AcrescentarItem(produto.ProdutoId, quantidade, produto.Preco);

            switch (status)
            {
                case PedidoStatus.Confirmado: pedido.Confirmar(); break;
                case PedidoStatus.Enviado: pedido.Confirmar(); pedido.MarcarComoEnviado(); break;
                case PedidoStatus.Entregue: pedido.Confirmar(); pedido.MarcarComoEnviado(); pedido.MarcarComoEntregue(); break;
                case PedidoStatus.Cancelado: pedido.Cancelar(); break;
            }

            if (data is not null)
                typeof(Pedido).GetProperty(nameof(Pedido.Data))!.SetValue(pedido, data.Value);

            context.Pedidos.Add(pedido);
            context.Pagamentos.Add(new Pagamento(pedido.PedidoId, metodo, valorDosProdutos + valorDoFrete));
        }

        CriarPedido(
            usuarioIds[0], [(maisVendido, 1)], PedidoStatus.Confirmado, MetodoPagamento.CartaoCredito,
            data: DateTime.UtcNow.AddDays(-30));
        CriarPedido(usuarioIds[0], [(maisVendido, 5)], PedidoStatus.Entregue, MetodoPagamento.Pix);
        CriarPedido(usuarioIds[1 % usuarioIds.Count], [(maisVendido, 4)], PedidoStatus.Confirmado, MetodoPagamento.CartaoCredito);
        CriarPedido(usuarioIds[2 % usuarioIds.Count], [(maisVendido, 100)], PedidoStatus.Cancelado, MetodoPagamento.Boleto);
        CriarPedido(usuarioIds[3 % usuarioIds.Count], [(segundoMaisVendido, 3)], PedidoStatus.Enviado, MetodoPagamento.Pix);
        CriarPedido(usuarioIds[4 % usuarioIds.Count], [(segundoMaisVendido, 2)], PedidoStatus.Pendente, MetodoPagamento.CartaoDebito);
        CriarPedido(usuarioIds[5 % usuarioIds.Count], [(poucoVendido, 1)], PedidoStatus.Entregue, MetodoPagamento.Pix);
        CriarPedido(usuarioIds[6 % usuarioIds.Count], [(maisVendido, 2), (segundoMaisVendido, 1)], PedidoStatus.Confirmado, MetodoPagamento.Boleto);

        await context.SaveChangesAsync();
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
