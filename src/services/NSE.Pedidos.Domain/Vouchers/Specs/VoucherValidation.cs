using NetDevPack.Specification;

namespace NSE.Pedidos.Domain.Vouchers.Specs
{
    public class VoucherValidation : SpecValidator<Voucher>
    {
        public VoucherValidation()
        {
            var dataSpec = new VoucherDataSpecification();
            var qtdSpec = new VoucherQuantidadeSpecification();
            var ativoSpec = new VoucherAtivoSpecification();

            Add("dataSpec", new Rule<Voucher>(dataSpec, "Este voucher está expirado"));
            Add("qtdSpec", new Rule<Voucher>(qtdSpec, "Este voucher já foi utilizado"));
            Add("ativoSpec", new Rule<Voucher>(ativoSpec, "Este voucher não está mais ativo"));
        }
    }
}
