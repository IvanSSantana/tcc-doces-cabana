using System;

namespace DocesCabana.Domain.Entities;

// Classe que representa um Produto do sistema (entidade de domínio)
public class Produto
{
    // Identificador único do produto
    public Guid ProdutoId { get; private set; }

    // Referência para a subcategoria do produto
    public Guid SubcategoriaId { get; private set; }

    // Nome do produto
    public string Nome { get; private set; }

    // Preço do produto
    public decimal Preco { get; private set; }

    // Status do produto (ex: 1 = ativo, 0 = inativo)
    public int Status { get; private set; }

    // Promoção associada (pode ser nula, pois nem todo produto tem promoção)
    public Guid? PromocaoId { get; private set; }

    // URL da imagem do produto
    public string ImagemUrl { get; private set; }

    // Construtor protegido usado pelo Entity Framework para materializar o objeto
    protected Produto() { }

    // Construtor principal usado para criar um produto válido
    public Produto(Guid subcategoriaId, string nome, decimal preco, string imagemUrl)
    {
        // Gera um ID único automaticamente
        ProdutoId = Guid.NewGuid();

        // Define como ativo por padrão
        Status = 1;

        // Usa métodos para garantir validações
        AlterarSubcategoria(subcategoriaId);
        AlterarNome(nome);
        DefinirPreco(preco);
        DefinirImagem(imagemUrl);
    }

    // Altera o nome do produto com validação
    public void AlterarNome(string nome)
    {
        // Verifica se o nome é vazio ou só espaços
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome inválido");

        // Regra adicional: mínimo de caracteres
        if (nome.Length < 3)
            throw new ArgumentException("Nome deve ter no mínimo 3 caracteres");

        Nome = nome;
    }

    // Define o preço do produto com validação
    public void DefinirPreco(decimal preco)
    {
        // Não permite preço zero ou negativo
        if (preco <= 0)
            throw new ArgumentException("Preço inválido");

        Preco = preco;
    }

    // Define ou altera a subcategoria do produto
    public void AlterarSubcategoria(Guid subcategoriaId)
    {
        // Verifica se o GUID é válido
        if (subcategoriaId == Guid.Empty)
            throw new ArgumentException("Subcategoria inválida");

        SubcategoriaId = subcategoriaId;
    }

    // Define a URL da imagem
    public void DefinirImagem(string url)
    {
        // Não permite URL vazia
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Imagem inválida");

        ImagemUrl = url;
    }

    // Ativa o produto
    public void Ativar()
    {
        Status = 1;
    }

    // Inativa o produto
    public void Inativar()
    {
        Status = 0;
    }

    // Associa uma promoção ao produto
    public void AplicarPromocao(Guid promocaoId)
    {
        // Verifica se o ID da promoção é válido
        if (promocaoId == Guid.Empty)
            throw new ArgumentException("Promoção inválida");

        // Regra de negócio: produto inativo não pode entrar em promoção
        if (Status == 0)
            throw new InvalidOperationException("Produto inativo não pode entrar em promoção");

        PromocaoId = promocaoId;
    }

    // Remove a promoção do produto
    public void RemoverPromocao()
    {
        PromocaoId = null;
    }
}