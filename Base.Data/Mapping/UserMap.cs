using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Base.Data.Models.Mapping
{
    public class UserMap : EntityTypeConfiguration<User>
    {
        public UserMap()
        {
            ToTable("Customer");

            HasKey(t => t.Id);
            Property(t => t.Id).HasColumnName("Id");
            Property(t => t.Name).HasColumnName("Name").HasMaxLength(255).IsRequired();
            Property(t => t.CompanyName).HasColumnName("CompanyName").HasMaxLength(255);
            Property(t => t.CNPJ).HasColumnName("CNPJ").HasMaxLength(255);
            Property(t => t.CPF).HasColumnName("CPF").HasMaxLength(255).IsRequired();
            Property(t => t.Email).HasColumnName("Email").HasMaxLength(255).IsRequired();
            Property(t => t.Telephone).HasColumnName("Telephone").HasMaxLength(255);
            Property(t => t.Celphone).HasColumnName("Celphone").HasMaxLength(255);
            Property(t => t.Password).HasColumnName("Password").HasMaxLength(255).IsRequired();
            Property(t => t.RegisteredOn).HasColumnName("RegisteredOn").IsRequired();
        }
    }
}
