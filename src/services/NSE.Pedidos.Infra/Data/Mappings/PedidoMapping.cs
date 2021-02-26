using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NSE.Pedidos.Domain.Pedidos;

namespace NSE.Pedidos.Infra.Data.Mappings
{
    public class PedidoMapping : IEntityTypeConfiguration<Pedido>
    {
        public void Configure(EntityTypeBuilder<Pedido> builder)
        {
            builder.HasKey(c => c.Id);

            builder.OwnsOne(p => p.Endereco, e =>
              {
                  e.Property(p => p.Logradouro).HasColumnName("Logradouro");
                  e.Property(p => p.Numero).HasColumnName("Numero");
                  e.Property(p => p.Complemento).HasColumnName("Complemento");
                  e.Property(p => p.Bairro).HasColumnName("Bairro");
                  e.Property(p => p.Cep).HasColumnName("Cep");
                  e.Property(p => p.Cidade).HasColumnName("Cidade");
                  e.Property(p => p.Estado).HasColumnName("Estado");
              });

            builder.Property(c => c.Codigo).HasDefaultValueSql("NEXT VALUE FOR MinhaSequencia");

            builder.HasMany(c => c.PedidoItems).WithOne(c => c.Pedido).HasForeignKey(c => c.PedidoId);

            builder.ToTable("Pedidos");
        }
    }
}
