using OrderAccumulator.Bussiness.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderAccumulator.Bussiness.Interfaces
{
    public interface IValidadorOrdemService
    {
        public List<ValidacaoResult> ValidarOrdem(OrdemRequest request);
    }
}
