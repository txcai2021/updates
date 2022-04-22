using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using SIMTech.APS.Resources;

namespace SIMTech.APS.Resource.API.PresentationModels
{
    /// <summary>
    /// Operation class exposes the following data members to the client:
    /// UserName, FirstName, LastName, Mobile, Email, IsLockedOut, Comment, 
    /// LastLoginDate, LastPasswordChangedDate, LastLockoutDate, FailedPasswordAttemptCount,
    /// and CreatedDate
    /// </summary>
    public class ResourcePM
    {
        public ResourcePM()
        {
            //UserRoles = new EntityCollection<UserRolePM>();
          //  resourceOperationPMs = new List<ResourceOperationPM>();
           // resourceParameterPMs = new List<ResourceParameterPM>();
           // resourceMParameterPMs = new List<ResourceMParameterPM>();
        }

        [Key]
        [Required(ErrorMessageResourceName = "ValidationErrorRequiredField", ErrorMessageResourceType = typeof(ErrorResources))]
        public int Id { get; set; }

      //  [Display(ShortName = "EquipmentName", ResourceType = typeof(SharedResources), Name = "EquipmentName", Order = 0)]
        [Required(ErrorMessageResourceName = "ValidationErrorRequiredField", ErrorMessageResourceType = typeof(ErrorResources))]
        [StringLength(50)]
        //[RegularExpression("^[a-zA-Z0-9_]*$", ErrorMessageResourceName = "ValidationErrorInvalidName", ErrorMessageResourceType = typeof(ErrorResources))]
        public string Name { get; set; }

       // [Display(ShortName = "ParentEquipmentId", ResourceType = typeof(SharedResources), Name = "ParentEquipmentId", Order = 1)]
        public int? ParentResourceId { get; set; }

      //  [Display(ShortName = "ResourceType", ResourceType = typeof(SharedResources), Name = "ResourceType", Order = 2)]
        [Required(ErrorMessageResourceName = "ValidationErrorRequiredField", ErrorMessageResourceType = typeof(ErrorResources))]
        //[CustomValidation(typeof(UserRules), "IsValidEmail")]
        public int? Type { get; set; }

      //  [Display(ShortName = "Category", ResourceType = typeof(SharedResources), Name = "Category", Order = 3)]
        //[Required(ErrorMessageResourceName = "ValidationErrorRequiredField", ErrorMessageResourceType = typeof(ErrorResources))]
        //[CustomValidation(typeof(UserRules), "IsValidEmail")]
        public string Category { get; set; }

       // [Display(ShortName = "Subcategory", ResourceType = typeof(SharedResources), Name = "Subcategory", Order = 4)]
        //[CustomValidation(typeof(UserRules), "IsValidEmail")]
        public string Subcategory { get; set; }

       // [Display(ShortName = "LocationId", ResourceType = typeof(SharedResources), Name = "LocationId", Order = 5)]
        //[Required(ErrorMessageResourceName = "ValidationErrorRequiredField", ErrorMessageResourceType = typeof(ErrorResources))]
        public int LocationId { get; set; }

      //  [Display(ShortName = "CalendarId", ResourceType = typeof(SharedResources), Name = "CalendarId", Order = 6)]
        public int? CalendarId { get; set; }

        //[Display(ShortName = "SkillLevel", ResourceType = typeof(SharedResources), Name = "SkillLevel", Order = 7)]
        public int? SkillLevel { get; set; }

        //[Display(ShortName = "Cost", ResourceType = typeof(SharedResources), Name = "Cost", Order = 8)]
      //  [Display(ShortName = "Hourly Rate", Name = "Hourly Rate")]
        public double? Cost { get; set; }

      //  [Display(ShortName = "CostBasisType", ResourceType = typeof(SharedResources), Name = "CostBasisType", Order = 9)]
        public string CostBasisType { get; set; }

       // [Display(ShortName = "Description", ResourceType = typeof(SharedResources), Name = "Description", Order = 10)]
        public string Description { get; set; }

       // [Display(ShortName = "String1", ResourceType = typeof(SharedResources), Name = "String1", Order = 11)]
        public string String1 { get; set; }

        [Required(ErrorMessageResourceName = "ValidationErrorRequiredField", ErrorMessageResourceType = typeof(ErrorResources))]
       // [Display(ShortName = "CreatedDate", ResourceType = typeof(SharedResources), Name = "CreatedDate", Order = 12)]
        public DateTime CreatedDate { get; set; }

        //[Include]
        //[Association("FK_ParentResource", "ParentResourceId", "Id", IsForeignKey = true)]
       // [Display(ShortName = "ParentResource", ResourceType = typeof(SharedResources), Name = "ParentResource", Order = 13)]
        public ResourcePM ParentResource { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string LocationName { get; set; }

      //  [Display(ShortName = "Calendar", Name = "Calendar")]
        public string CalendarName { get; set; }

        public string TypeName { get; set; }

        public string ParentName { get; set; }

        public int Operations { get; set; }

        public int NoofBlockOuts { get; set; }

        //[Include]
        //[Composition]
        //[Association("FK_Resource_ResourceOperations", "Id", "ResourceId")]
        //public List<ResourceOperationPM> resourceOperationPMs { get; set; }

        public string OperationNames { get; set; }

        public int Parameters { get; set; }

        //[Include]
        //[Composition]
        //[Association("FK_Resource_ResourceParameters", "Id", "ResourceId")]
       // public List<ResourceParameterPM> resourceParameterPMs { get; set; }

      //  public string ParameterNames { get; set; }

        //public int MParameters { get; set; }

        //[Include]
        //[Composition]
        //[Association("FK_Resource_ResourceMParameters", "Id", "ResourceId")]
       // public List<ResourceMParameterPM> resourceMParameterPMs { get; set; }

      //  public string MParameterNames { get; set; }

        #region Auxiliary routines

        /// <summary>
        /// Validates the given property.
        /// </summary>
        /// <param name="propertyName">Property to validate.</param>
        /// <returns>Validation result.</returns>
        private bool Validate(string propertyName, object value)
        {
            bool flag = false;

            var context = new ValidationContext(this, null, null) { MemberName = propertyName };
            var validationResults = new Collection<System.ComponentModel.DataAnnotations.ValidationResult>();
            if (propertyName == "Password")
            {
                //flag = System.ComponentModel.DataAnnotations.Validator.TryValidateProperty(Password, context, validationResults);
                if (!flag)
                    ShowValidationResults(validationResults);
            }
            else
                flag = true;

            return flag;
        }

        static void ShowValidationResults(Collection<System.ComponentModel.DataAnnotations.ValidationResult> results)
        {
            // Check if the ValidationResults detected any validation errors.
            if (results.Count() == 0)
            {
                Console.WriteLine("There were no validation errors.");
            }
            else
            {
                Console.WriteLine("The following {0} validation errors were detected:", results.Count);
                // Iterate through the collection of validation results.
                foreach (ValidationResult item in results)
                {
                    // Show the target member name and current value.
                    //Console.WriteLine("+ Target object: {0}, Member: {1}", GetTypeNameOnly(item.Target), item.Key);
                    // Display details of this validation error.
                    Console.WriteLine("{0}- Message: '{1}'", "  ", item.ErrorMessage);
                }
            }
            Console.WriteLine();
        }

        #endregion Auxiliary routines

    }
}
