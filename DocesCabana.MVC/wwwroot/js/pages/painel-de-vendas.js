// Painel de vendas (Admin/Vendas): desenha os gráficos a partir do JSON que o
// servidor deixa em #dados-do-painel. Cartões e tabelas já vêm prontos no HTML;
// sem este script (ou sem o Chart.js) a página continua legível pelas tabelas.
(function () {
    const fonte = document.getElementById('dados-do-painel');
    if (!fonte || typeof Chart === 'undefined') return;

    const dados = JSON.parse(fonte.textContent);
    const estilo = getComputedStyle(document.documentElement);
    const token = (nome) => estilo.getPropertyValue(nome).trim();

    const corPrimaria = token('--cor-primaria');
    const corDestaque = token('--cor-destaque');
    const corTextoSuave = token('--neutro-texto-suave');
    const corDivisor = token('--neutro-divisor');

    const moeda = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });

    Chart.defaults.font.family = token('--fonte-principal') || 'Inter, sans-serif';
    Chart.defaults.color = corTextoSuave;

    function desenhar(idDoCanvas, pontos, cor) {
        const canvas = document.getElementById(idDoCanvas);
        if (!canvas || !pontos || pontos.length === 0) return;

        const maior = Math.max(...pontos.map(p => p.receita));

        new Chart(canvas, {
            type: 'bar',
            data: {
                labels: pontos.map(p => p.rotulo),
                datasets: [{
                    label: 'Receita',
                    data: pontos.map(p => p.receita),
                    // O pico ganha a cor de destaque; o resto fica na cor da marca.
                    backgroundColor: pontos.map(p => (maior > 0 && p.receita === maior) ? corDestaque : cor),
                    borderRadius: 4,
                    maxBarThickness: 32
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: (contexto) => {
                                const ponto = pontos[contexto.dataIndex];
                                const pedidos = ponto.pedidos === 1 ? '1 pedido' : `${ponto.pedidos} pedidos`;
                                return `${moeda.format(ponto.receita)} · ${pedidos}`;
                            }
                        }
                    }
                },
                scales: {
                    x: { grid: { display: false } },
                    y: {
                        beginAtZero: true,
                        grid: { color: corDivisor },
                        ticks: { callback: (valor) => moeda.format(valor) }
                    }
                }
            }
        });
    }

    desenhar('grafico-serie', dados.serie, corPrimaria);
    desenhar('grafico-dia-da-semana', dados.porDiaDaSemana, corPrimaria);
    desenhar('grafico-hora', dados.porHora, corPrimaria);
})();
