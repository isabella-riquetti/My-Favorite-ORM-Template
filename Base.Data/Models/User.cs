using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Base.Data.Models
{
    public class User
    {
        public User()
        {
            //CustomerAddresses = new List<CustomerAddress>();
        }

        public Guid Id { get; set; }

        [Display(Name = "Nome Completo")]
        [Required(ErrorMessage = "O '{0}' é obrigatório")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "O campo '{0}' deve ter entre {1} e {2} caracteres")]
        public string Name { get; set; }

        [Display(Name = "Empresa")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "O campo '{0}' deve ter entre {1} e {2} caracteres")]
        public string CompanyName { get; set; }

        [Display(Name = "CNPJ")]
        [StringLength(18, MinimumLength = 18, ErrorMessage = "O campo '{0}' deve ter entre {1} e {2} caracteres")]
        public string CNPJ { get; set; }

        [Display(Name = "CPF")]
        [Required(ErrorMessage = "O '{0}' é obrigatório")]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "O campo '{0}' deve ter entre {1} e {2} caracteres")]
        public string CPF { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "E-mail em formato inválido.")]
        [Required(ErrorMessage = "O '{0}' é obrigatório")]
        public string Email { get; set; }

        [Display(Name = "Telefone Fixo")]
        [RegularExpression(@"([(][0-9]{2}[)])\s[0-9]{4}\-[0-9]{4}")]
        public string Telephone { get; set; }

        [Display(Name = "Celular")]
        [RegularExpression(@"[(]\d{2}[)] \d{1} \d{4}-\d{4}")]
        public string Celphone { get; set; }

        [Display(Name = "Senha")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "A '{0}' é obrigatória")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$")]
        public string Password { get; set; }

        [Display(Name = "Data do Registro")]
        [Required(ErrorMessage = "A '{0}' é obrigatória")]
        public DateTime RegisteredOn { get; set; }

        //public virtual ICollection<CustomerAddress> CustomerAddresses { get; set; }
    }
}
