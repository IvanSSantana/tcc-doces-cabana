using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Application.Enums;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DocesCabana.MVC.Helpers;

namespace DocesCabana.MVC.Controllers;

// Duas fontes, uma tela: autenticado lê e escreve no banco; visitante, na
// sessão (spec 017, Fase 6). Nenhuma das quatro ações exige [Authorize] —
// cada uma decide por conta própria, olhando UsuarioAtualId, para atender
// os dois públicos com o mesmo endpoint.
public class CarrinhoController : Controller
{
    private readonly ICarrinhoService _carrinhoService;
    private readonly IFreteService _freteService;
    private readonly IValidator<ConsultaDeFreteDTO> _consultaDeFreteValidator;
    private readonly IPedidoService _pedidoService;
    private readonly IEnderecoService _enderecoService;

    public CarrinhoController(
        ICarrinhoService carrinhoService, IFreteService freteService,
        IValidator<ConsultaDeFreteDTO> consultaDeFreteValidator, IPedidoService pedidoService,
        IEnderecoService enderecoService)
    {
        _carrinhoService = carrinhoService;
        _freteService = freteService;
        _consultaDeFreteValidator = consultaDeFreteValidator;
        _pedidoService = pedidoService;
        _enderecoService = enderecoService;
    }

    // RF-04/RF-09/RF-11 (spec 020): cep é opcional — sem ele, a tela é só o
    // carrinho de sempre. Com ele, cota antes de exibir: CEP inválido não
    // chega a bater no serviço (RF-09/CA-10), e carrinho sem item
    // disponível também não cota (RF-11/CA-13) — não há o que despachar.
    //
    // passo/enderecoId (spec 022, RF-01): qual passo do fechamento exibir —
    // Carrinho é o repouso. Um valor de passo desconhecido cai no primeiro
    // membro do enum (Carrinho) por conta do próprio model binding do
    // ASP.NET Core, sem código extra.
    [HttpGet]
    public async Task<IActionResult> Index(
        string? cep = null, PassoDoFechamento passo = PassoDoFechamento.Carrinho,
        Guid? enderecoId = null, int? servicoDeEntregaId = null)
    {
        var carrinho = await ObterCarrinhoAtual();

        if (!string.IsNullOrWhiteSpace(cep))
        {
            var consulta = new ConsultaDeFreteDTO(cep);
            var validacao = await _consultaDeFreteValidator.ValidateAsync(consulta);

            if (!validacao.IsValid)
            {
                validacao.AddToModelState(ModelState);
            }
            else
            {
                // RN-03: só os itens disponíveis vão para a cotação — o
                // adaptador não precisa conhecer ProdutoStatus.
                var disponiveis = carrinho.Linhas.Where(l => l.Disponivel).ToList();
                if (disponiveis.Count > 0)
                {
                    var cotacao = await _freteService.Cotar(cep, disponiveis);
                    carrinho = carrinho.ComCotacao(cotacao);
                }
            }
        }

        await MontarPassoDoFechamento(carrinho, passo, enderecoId, servicoDeEntregaId);

        if (EhRequisicaoAssincrona)
            return PartialView("_ItensDoCarrinho", carrinho);

        return View(carrinho);
    }

    // RF-07 (spec 022): cadastra o endereço sem sair do fechamento — mesma
    // regra de negócio de Conta/NovoEndereco (IEnderecoService.Cadastrar),
    // só o destino do redirecionamento muda. O primeiro endereço de alguém
    // já nasce principal (EnderecoService, RN-02 da 018), então volta
    // escolhido sozinho (CA-06), sem precisar de mais nada aqui.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> CadastrarEndereco(EnderecoDTO dto)
    {
        if (ModelState.IsValid)
            await _enderecoService.Cadastrar(dto, UsuarioAtualId!.Value);

        if (EhRequisicaoAssincrona)
        {
            var carrinho = await ObterCarrinhoAtual();
            await MontarPassoDoFechamento(carrinho, PassoDoFechamento.Endereco, null, null);
            return PartialView("_ItensDoCarrinho", carrinho);
        }

        return RedirectToAction(nameof(Index), new { passo = PassoDoFechamento.Endereco });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Acrescentar(Guid produtoId, short quantidade = 1)
    {
        var usuarioId = UsuarioAtualId;
        if (usuarioId is not null)
        {
            // Produto indisponível (RN-06) lança InvalidOperationException,
            // que o FilterException global captura e traduz — não há
            // try/catch aqui (Princípio VIII).
            await _carrinhoService.Acrescentar(usuarioId.Value, produtoId, quantidade);
        }
        else
        {
            var atualizado = await _carrinhoService.AcrescentarAvulso(HttpContext.Session.Ler(), produtoId, quantidade);
            HttpContext.Session.Escrever(atualizado);
        }

        return await DevolverResultado();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarQuantidade(Guid produtoId, short quantidade)
    {
        var usuarioId = UsuarioAtualId;
        if (usuarioId is not null)
        {
            await _carrinhoService.AlterarQuantidade(usuarioId.Value, produtoId, quantidade);
        }
        else
        {
            var atualizado = _carrinhoService.AlterarQuantidadeAvulsa(HttpContext.Session.Ler(), produtoId, quantidade);
            HttpContext.Session.Escrever(atualizado);
        }

        return await DevolverResultado();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(Guid produtoId)
    {
        var usuarioId = UsuarioAtualId;
        if (usuarioId is not null)
        {
            await _carrinhoService.Remover(usuarioId.Value, produtoId);
        }
        else
        {
            var atualizado = _carrinhoService.RemoverAvulso(HttpContext.Session.Ler(), produtoId);
            HttpContext.Session.Escrever(atualizado);
        }

        return await DevolverResultado();
    }

    // RF-11 (spec 021): a pergunta é a única tela que funciona sem
    // JavaScript — com script, o mesmo POST é disparado por um diálogo
    // inline (carrinho.js), sem passar por esta view.
    [HttpGet]
    public IActionResult ConfirmarEsvaziar() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Esvaziar()
    {
        var usuarioId = UsuarioAtualId;
        if (usuarioId is not null)
        {
            await _carrinhoService.Esvaziar(usuarioId.Value);
        }
        else
        {
            HttpContext.Session.Limpar();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<CarrinhoDTO> ObterCarrinhoAtual()
    {
        var usuarioId = UsuarioAtualId;
        return usuarioId is not null
            ? await _carrinhoService.ObterDoUsuario(usuarioId.Value)
            : await _carrinhoService.MontarAvulso(HttpContext.Session.Ler());
    }

    // Um endereço, duas representações (mesmo padrão da 014): o caminho
    // assíncrono recebe só o bloco que mudou; o comum, POST-Redirect-Get
    // de volta para a própria tela (Princípio VII).
    private async Task<IActionResult> DevolverResultado()
    {
        if (EhRequisicaoAssincrona)
        {
            var carrinho = await ObterCarrinhoAtual();
            // Ações de quantidade/remover sempre partem do passo do
            // carrinho — não há como chegar nelas de outro passo, os
            // controles vivem só na lista de itens.
            await MontarPassoDoFechamento(carrinho, PassoDoFechamento.Carrinho, null, null);
            return PartialView("_ItensDoCarrinho", carrinho);
        }

        return RedirectToAction(nameof(Index));
    }

    // RF-01 (spec 022): o indicador de passos aparece em toda resposta desta
    // tela, síncrona ou não — quem decide o conteúdo de cada passo é
    // IPedidoService, este método só resolve quem está autenticado (visitante
    // sem conta nunca vê Endereço/Pagamento de verdade, mesmo se navegar
    // direto para lá pela URL).
    private async Task MontarPassoDoFechamento(CarrinhoDTO carrinho, PassoDoFechamento passo, Guid? enderecoId, int? servicoDeEntregaId)
    {
        var usuarioId = UsuarioAtualId;

        var passoEfetivo = usuarioId is null && passo is PassoDoFechamento.Endereco or PassoDoFechamento.Pagamento
            ? PassoDoFechamento.Conta
            : passo;

        ViewData["PassoDoFechamento"] =
            await _pedidoService.MontarPasso(passoEfetivo, carrinho, usuarioId, enderecoId, servicoDeEntregaId);
    }

    private bool EhRequisicaoAssincrona =>
        Request.Headers["X-Requested-With"] == "XMLHttpRequest";

    private Guid? UsuarioAtualId
    {
        get
        {
            if (User.Identity is not { IsAuthenticated: true })
                return null;

            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return id is null ? null : Guid.Parse(id);
        }
    }
}
