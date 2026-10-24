OrderAccumulator

O OrderAccumulator é a aplicação da "Corretora" que recebe ordens do OrderGenerator por uma sessão FIX 4.4, usando QuickFIX


Ao receber uma mensagem FIX `NewOrderSingle`, a aplicação valida os campos e calcula a nova exposição do símbolo:

Exposição financeira = somatório de (preço quantidade executada) de cada ordem de compra - somatório de (preço quantidade executada) de venda.

Ou seja, as ordens de compra aumentam a exposição e as de venda diminuem a exposição.

O OrderAccumulator terá um limite interno constante, por símbolo, de R$ 100.000.000 (cem milhões).

Isso significa que qualquer ordem que venha a ultrapassar em valor absoluto o limite de exposição deve ser respondida com uma rejeição.

Ou seja, caso a ordem seja aceita, o OrderAccumulator deve responder com um ExecutionReport tendo ExecType = New e a ordem deve ser considerada no cálculo de exposição.

A exposição fica em memória e é mantida separadamente para cada símbolo. Ela reinicia quando o processo do OrderAccumulator é reiniciado.

## Executar localmente

Requer o SDK do .NET 8 e acesso ao NuGet para restaurar os pacotes. 

Porta HTTP padrão:

- Swagger: <http://localhost:5240/swagger>
- Exposicoes atuais: <http://localhost:5240/api/exposures>

## Configuracao FIX

O arquivo [fix-acceptor.cfg](OrderAccumulator/fix-acceptor.cfg) configura a sessão FIX 4.4:

- SenderCompID: `ORDERACCUMULATOR`
- TargetCompID: `ORDERGENERATOR`
- Endereco de escuta: `127.0.0.1:5001`

O OrderGenerator deve usar os mesmos IDs de sessão, invertidos, e conectar-se ao host e porta configurados aqui. O arquivo é copiado para a pasta de saída durante a compilação.
Log de mensagens FIX é gravado no arquivo fix-acceptor.log na pasta de saída.
