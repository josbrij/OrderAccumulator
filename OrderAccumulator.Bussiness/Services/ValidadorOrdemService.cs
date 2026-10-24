using OrderAccumulator.Bussiness.Interfaces;
using OrderAccumulator.Bussiness.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderAccumulator.Bussiness.Services
{
    public class ValidadorOrdemService : IValidadorOrdemService
    {
        public ValidadorOrdemService() { }
        public List<ValidacaoResult> ValidarOrdem(OrdemRequest request)
        {
            List<ValidacaoResult> retorno = new List<ValidacaoResult>();
            ValidacaoResult validacao;

            if (request == null)
            {
                validacao = new ValidacaoResult("Requisição inválida");
                retorno.Add(validacao);
            }
            if(request.Quantidade <= 0 || request.Quantidade > 100000)
            {
                validacao = new ValidacaoResult("Quantidade deve estar entre 0 e 100.000");
                retorno.Add(validacao);
            }
            if (request.Preco <= 0 || request.Preco > 1000)
            {
                validacao = new ValidacaoResult("Preço deve estar entre R$0,01 e R$1.000,00");
                retorno.Add(validacao);
            }
           
            return retorno;
        }
    }
}
