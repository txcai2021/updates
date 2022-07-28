using System.ComponentModel.DataAnnotations;
using SIMTech.APS.Resources;

namespace SIMTech.APS.Customer.API.PresentationModels
{
    public class SupplierPM
    {
        [Key]
        public int Id { get; set; }

        [StringLength(250, ErrorMessageResourceType = typeof(ErrorResources), ErrorMessageResourceName = "ValidationErrorBadSupplierCode")]
        //[Required(ErrorMessageResourceType = typeof(ErrorResources), ErrorMessageResourceName = "ValidationErrorRequiredField")]
        [Display(ShortName = "SupplierCode", ResourceType = typeof(SharedResources), Name = "SupplierCode")]
        public string Code { get; set; }

        [StringLength(50, ErrorMessageResourceType = typeof(ErrorResources), ErrorMessageResourceName = "ValidationErrorBadSupplierName")]
        [Required(ErrorMessageResourceType = typeof(ErrorResources), ErrorMessageResourceName = "ValidationErrorRequiredField")]
        [RegularExpression("^[a-zA-Z0-9_ ]*$", ErrorMessageResourceType = typeof(ErrorResources), ErrorMessageResourceName = "ValidationErrorInvalidName")]
        [Display(ShortName = "SupplierName", ResourceType = typeof(SharedResources), Name = "SupplierName")]
        public string Name { get; set; }

        [StringLength(50, ErrorMessageResourceType = typeof(ErrorResources), ErrorMessageResourceName = "ValidationErrorBadSupplierSite")]
        //[Display(ShortName = "SupplierSite", ResourceType = typeof(SharedResources), Name = "SupplierSite")]
        public string Site { get; set; }

        [StringLength(50, ErrorMessageResourceType = typeof(ErrorResources), ErrorMessageResourceName = "ValidationErrorBadSupplierDepartment")]
        //[Display(ShortName = "SupplierDepartment", ResourceType = typeof(SharedResources), Name = "SupplierDepartment")]
        public string Department { get; set; }

        [StringLength(50, ErrorMessageResourceType = typeof(ErrorResources), ErrorMessageResourceName = "ValidationErrorBadSupplierSite")]
        [Display(ShortName = "ContactPerson", ResourceType = typeof(SharedResources), Name = "ContactPerson")]
        public string ContactPerson { get; set; }

        [StringLength(50, ErrorMessageResourceType = typeof(ErrorResources), ErrorMessageResourceName = "ValidationErrorBadSupplierSite")]
        [Display(ShortName = "Phone", ResourceType = typeof(SharedResources), Name = "ContactPerson")]
        public string Phone { get; set; }

        [StringLength(50, ErrorMessageResourceType = typeof(ErrorResources), ErrorMessageResourceName = "ValidationErrorBadSupplierSite")]
        [Display(ShortName = "Fax", ResourceType = typeof(SharedResources), Name = "ContactPerson")]
        public string Fax { get; set; }

        [StringLength(250, ErrorMessageResourceType = typeof(ErrorResources), ErrorMessageResourceName = "ValidationErrorBadSupplierSite")]
        [Display(ShortName = "Email", ResourceType = typeof(SharedResources), Name = "ContactPerson")]
        public string Email { get; set; }

        [StringLength(250, ErrorMessageResourceType = typeof(ErrorResources), ErrorMessageResourceName = "ValidationErrorBadSupplierSite")]
        [Display(ShortName = "Address", ResourceType = typeof(SharedResources), Name = "Address")]
        public string Address { get; set; }


        [StringLength(250, ErrorMessageResourceType = typeof(ErrorResources), ErrorMessageResourceName = "ValidationErrorBadSupplierSite")]
        [Display(ShortName = "Billing Address")]
        public string BillingAddress { get; set; }


        [Display(ShortName = "Description", ResourceType = typeof(SharedResources), Name = "Description")]
        public string Description { get; set; }


        //[Display(ShortName = "Supplier Code", ResourceType = typeof(SharedResources), Name = "SupplierCode")]
        //public string SupplierCode { get; set; }

        [Display(ShortName = "Credit Term", ResourceType = typeof(SharedResources), Name = "CreditTerm")]
        public string CreditTerm { get; set; }


        public string Category { get; set; }

        public int? PictureId { get; set; }

        public byte[] ImageLogo { get; set; }
        public string Currency { get; set; }




    }
}
