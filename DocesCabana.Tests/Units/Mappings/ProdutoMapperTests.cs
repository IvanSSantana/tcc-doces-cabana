using DocesCabana.Application.DTOs;
using DocesCabana.Application.Mappings;
using DocesCabana.Domain.Entities;
using DocesCabana.Domain.Enums;

namespace DocesCabana.Tests.Units.Mappings;

public class ProdutoMapperTests
{
    [Fact]
    public void Dado_UmaEntidade_Quando_ToDTO_Entao_DevePreservarTodosOsCampos()
    {
        var id = Guid.NewGuid();
        var subcategoriaId = Guid.NewGuid();
        var produto = new Produto(subcategoriaId, "Brigadeiro Gourmet", 5.50m, "https://imagem.com/brigadeiro.jpg", 0.5m, 10m, 15m, 20m, ProdutoStatus.Inativo, id);

        var dto = ProdutoMapper.ToDTO(produto);

        Assert.Equal(id, dto.ProdutoId);
        Assert.Equal("Brigadeiro Gourmet", dto.Nome);
        Assert.Equal(5.50m, dto.Preco);
        Assert.Equal(ProdutoStatus.Inativo, dto.Status);
        Assert.Equal("https://imagem.com/brigadeiro.jpg", dto.ImagemUrl);
        Assert.Equal(subcategoriaId, dto.SubcategoriaId);
        Assert.Null(dto.PromocaoId);
        Assert.Equal(0.5m, dto.Peso);
        Assert.Equal(10m, dto.Altura);
        Assert.Equal(15m, dto.Largura);
        Assert.Equal(20m, dto.Comprimento);
    }

    [Fact]
    public void Dado_UmDTOComStatusInativo_Quando_ToEntity_Entao_DevePreservarStatus()
    {
        var dto = new ProdutoDTO
        {
            Nome = "Pé de Moça",
            Preco = 27.00m,
            Status = ProdutoStatus.Inativo,
            ImagemUrl = "https://imagem.com/pe-de-moca.jpg",
            SubcategoriaId = Guid.NewGuid(),
            Peso = 0.5m,
            Altura = 10m,
            Largura = 15m,
            Comprimento = 20m
        };

        var produto = ProdutoMapper.ToEntity(dto);

        Assert.Equal(ProdutoStatus.Inativo, produto.Status);
        Assert.Equal(dto.Nome, produto.Nome);
        Assert.Equal(dto.Preco, produto.Preco);
        Assert.Equal(dto.ImagemUrl, produto.ImagemUrl);
        Assert.Equal(dto.SubcategoriaId, produto.SubcategoriaId);
        Assert.Equal(dto.Peso, produto.Peso);
        Assert.Equal(dto.Altura, produto.Altura);
        Assert.Equal(dto.Largura, produto.Largura);
        Assert.Equal(dto.Comprimento, produto.Comprimento);
    }

    [Fact]
    public void Dado_UmaListaVazia_Quando_ToDTO_Entao_DeveRetornarListaVazia()
    {
        var resultado = ProdutoMapper.ToDTO(new List<Produto>());

        Assert.Empty(resultado);
    }

    // ComImagem vive em ProdutoDTO (não em ProdutoMapper), mas é provado
    // aqui — vizinho mais próximo do DTO — em vez de uma pasta Units/DTOs
    // nova para um método de duas linhas (spec 027, T005).
    [Fact]
    public void Dado_UmDTO_Quando_ComImagem_Entao_DeveDevolverCopiaComEnderecoPreenchidoPreservandoOResto()
    {
        var dto = new ProdutoDTO
        {
            ProdutoId = Guid.NewGuid(),
            Nome = "Brigadeiro Gourmet",
            Preco = 5.50m,
            Status = ProdutoStatus.Ativo,
            ImagemUrl = "",
            Descricao = "Descrição",
            SubcategoriaId = Guid.NewGuid(),
            SemAcucar = true,
            Peso = 0.5m,
            Altura = 10m,
            Largura = 15m,
            Comprimento = 20m
        };

        var copia = dto.ComImagem("https://imagem.com/produto.jpg");

        Assert.Equal("https://imagem.com/produto.jpg", copia.ImagemUrl);
        Assert.Equal(dto.ProdutoId, copia.ProdutoId);
        Assert.Equal(dto.Nome, copia.Nome);
        Assert.Equal(dto.Preco, copia.Preco);
        Assert.Equal(dto.Status, copia.Status);
        Assert.Equal(dto.Descricao, copia.Descricao);
        Assert.Equal(dto.SubcategoriaId, copia.SubcategoriaId);
        Assert.Equal(dto.SemAcucar, copia.SemAcucar);
        Assert.Equal(dto.Peso, copia.Peso);
        Assert.Equal(dto.Altura, copia.Altura);
        Assert.Equal(dto.Largura, copia.Largura);
        Assert.Equal(dto.Comprimento, copia.Comprimento);
    }
}
