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
        var produto = new Produto(subcategoriaId, "Brigadeiro Gourmet", 5.50m, "https://imagem.com/brigadeiro.jpg", ProdutoStatus.Inativo, id);

        var dto = ProdutoMapper.ToDTO(produto);

        Assert.Equal(id, dto.Id);
        Assert.Equal("Brigadeiro Gourmet", dto.Nome);
        Assert.Equal(5.50m, dto.Preco);
        Assert.Equal(ProdutoStatus.Inativo, dto.Status);
        Assert.Equal("https://imagem.com/brigadeiro.jpg", dto.ImagemUrl);
        Assert.Equal(subcategoriaId, dto.SubcategoriaId);
        Assert.Null(dto.PromocaoId);
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
            SubcategoriaId = Guid.NewGuid()
        };

        var produto = ProdutoMapper.ToEntity(dto);

        Assert.Equal(ProdutoStatus.Inativo, produto.Status);
        Assert.Equal(dto.Nome, produto.Nome);
        Assert.Equal(dto.Preco, produto.Preco);
        Assert.Equal(dto.ImagemUrl, produto.ImagemUrl);
        Assert.Equal(dto.SubcategoriaId, produto.SubcategoriaId);
    }

    [Fact]
    public void Dado_UmaListaVazia_Quando_ToDTO_Entao_DeveRetornarListaVazia()
    {
        var resultado = ProdutoMapper.ToDTO(new List<Produto>());

        Assert.Empty(resultado);
    }
}
