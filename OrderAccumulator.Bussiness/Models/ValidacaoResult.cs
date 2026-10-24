using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderAccumulator.Bussiness.Models
{
    public class ValidacaoResult
    {
        public ValidacaoResult(string mensagem) { 
            Mensagem = mensagem;
        }
        public string Mensagem { get; set; }
    }
}
