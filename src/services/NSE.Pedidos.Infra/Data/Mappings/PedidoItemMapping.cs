using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NSE.Pedidos.Domain.Pedidos;

namespace NSE.Pedidos.Infra.Data.Mappings
{
    public class PedidoItemMapping : IEntityTypeConfiguration<PedidoItem>
    {
        public void Configure(EntityTypeBuilder<PedidoItem> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(p => p.ProdutoNome).IsRequired().HasColumnType("varchar(250)");

            builder.HasOne(p => p.Pedido).WithMany(p => p.PedidoItems);

            builder.ToTable("PedidoItems");
        }
    }
}
