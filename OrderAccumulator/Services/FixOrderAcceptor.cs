using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;
using FixMessage = QuickFix.Message;

namespace OrderAccumulator.Services;

public sealed class FixOrderAcceptor : IApplication, IHostedService, IDisposable
{
    private const decimal LimiteExposicao = 100_000_000m;
    private readonly ILogger<FixOrderAcceptor> _logger;
    private readonly object _sync = new();
    private readonly Dictionary<string, decimal> _exposure = new(StringComparer.Ordinal);
    private ThreadedSocketAcceptor? _acceptor;

    public FixOrderAcceptor(ILogger<FixOrderAcceptor> logger) => _logger = logger;

    public IReadOnlyDictionary<string, decimal> GetExposures()
    {
        lock (_sync)
            return new Dictionary<string, decimal>(_exposure);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        //Carregar configuração do protocolo FIX (padrão)
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "fix-acceptor.cfg");
        var settings = new SessionSettings(settingsPath);
        _acceptor = new ThreadedSocketAcceptor(this, new FileStoreFactory(settings), settings, new FileLogFactory(settings), new DefaultMessageFactory());
        _acceptor.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _acceptor?.Stop();
        return Task.CompletedTask;
    }

    //Chamada pelo frontend
    public void FromApp(FixMessage message, SessionID sessionId)
    {
        if (message is not NewOrderSingle order)
            return;
     
        var quantidade = order.OrderQty.Value;
        var preco = order.Price.Value;
        var clOrdId = order.ClOrdID.Value;
        var acao = order.Symbol.Value;
        var tipoOrdem = order.Side.Value;
        var quantidadeInt = (int)quantidade;
        var validaRequest = ValidarRequest(acao,tipoOrdem,quantidadeInt,preco);

        var accepted = false;
        decimal resultingExposure = 0;
        string resultadoFinal;
        lock (_sync)
        {
            if (string.IsNullOrEmpty(validaRequest))
            {
                var currentExposure = _exposure.GetValueOrDefault(acao);
                var signedNotional = preco * quantidade * (tipoOrdem == Side.BUY ? 1 : -1);
                resultingExposure = currentExposure + signedNotional;
                if (Math.Abs(resultingExposure) > LimiteExposicao)
                {
                    resultingExposure = currentExposure;
                    resultadoFinal = $"Ordem rejeitada: o limite de exposição de R$ {LimiteExposicao:N2} seria excedido.";
                }
                else
                {
                    accepted = true;
                    _exposure[acao] = resultingExposure;
                    resultadoFinal = $"Ordem aceita. Exposição de {acao}: R$ {resultingExposure:N2}.";
                }
            }
            else
            {
                resultadoFinal = $"Ordem rejeitada: campos inválidos.\n{validaRequest}";
            }
        }

        var report = new ExecutionReport();
        report.Set(new OrderID(Guid.NewGuid().ToString("N")));
        report.Set(new ExecID(Guid.NewGuid().ToString("N")));
        report.Set(new ClOrdID(clOrdId));
        report.Set(new ExecType(accepted ? ExecType.NEW : ExecType.REJECTED));
        report.Set(new OrdStatus(accepted ? OrdStatus.NEW : OrdStatus.REJECTED));
        report.Set(new Symbol(acao));
        report.Set(new Side(tipoOrdem));
        report.Set(new OrderQty(quantidade));
        report.Set(new Price(preco));
        report.Set(new LeavesQty(accepted ? quantidade : 0));
        report.Set(new CumQty(0));
        report.Set(new AvgPx(0));
        report.Set(new TransactTime(DateTime.UtcNow));
        report.Set(new Text(resultadoFinal));

        try
        {
            Session.SendToTarget(report, sessionId);
            _logger.LogInformation("Ordem {ClOrdId} {Result}; símbolo {Symbol}; exposição {Exposure}", clOrdId, accepted ? "aceita" : "rejeitada", acao, resultingExposure);
        }
        catch (SessionNotFound ex)
        {
            _logger.LogError(ex, "Não foi possível responder à ordem {ClOrdId}", clOrdId);
        }
    }

    public string ValidarRequest(string acao,char tipoOrdem, int quantidadeInt, decimal preco)
    {
        string retorno = string.Empty;
        if (quantidadeInt <= 0 || quantidadeInt > 100000)
        {
            retorno = "Quantidade deve estar entre 0 e 100.000";
        }
        if (preco <= 0 || preco > 1000)
        {
            retorno = retorno.Length > 0 ? retorno + "\n" + "Preço deve estar entre R$0,01 e R$1.000,00": "Preço deve estar entre R$0,01 e R$1.000,00";            
        }
        if (acao != "PETR4" && acao != "VALE3" && acao != "VIIA4")
        {
            retorno = retorno.Length > 0 ? retorno + "\n" + $"Ação {acao} não reconhecida" : $"Ação {acao} não reconhecida";
        }
        if(tipoOrdem != Side.BUY && tipoOrdem != Side.SELL)
        {
            retorno = retorno.Length > 0 ? retorno + "\n" + $"Tipo ordem {tipoOrdem} não reconhecida" : $"Tipo ordem {tipoOrdem} não reconhecida";
        }
        return retorno;
    }
    #region Métodos do QuickFix
    public void OnCreate(SessionID sessionId) { }
    public void OnLogon(SessionID sessionId) => _logger.LogInformation("Initiator conectado: {Session}", sessionId);
    public void OnLogout(SessionID sessionId) => _logger.LogWarning("Initiator desconectado: {Session}", sessionId);
    public void ToAdmin(FixMessage message, SessionID sessionId) { }
    public void ToApp(FixMessage message, SessionID sessionId) { }
    public void FromAdmin(FixMessage message, SessionID sessionId) { }
    #endregion

    public void Dispose()
    {
        _acceptor?.Stop();
        _acceptor?.Dispose();
    }
}
