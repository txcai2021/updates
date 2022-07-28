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
    public class ResourceParameterPM
    {
        public ResourceParameterPM()
        {
        }

        [Key]
        [Required(ErrorMessageResourceName = "ValidationErrorRequiredField", ErrorMessageResourceType = typeof(ErrorResources))]
        public int Id { get; set; }

        [Display(ShortName = "ResourceId", ResourceType = typeof(SharedResources), Name = "ResourceId", Order = 0)]
        public int ResourceId { get; set; }

        //[Display(ShortName = "ParameterId", ResourceType = typeof(SharedResources), Name = "ParameterId", Order = 1)]
        public int ParameterId { get; set; }

        [Required(ErrorMessageResourceName = "ValidationErrorRequiredField", ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(ShortName = "CreatedDate", ResourceType = typeof(SharedResources), Name = "CreatedDate", Order = 2)]
        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        [Association("FK_Resource_ResourceParameters", "ResourceId", "Id", IsForeignKey = true)]
        public ResourcePM resourcePM { get; set; }

        //[Association("FK_Resource_ParameterResources", "ResourceId", "Id", IsForeignKey = true)]
        //public ParameterPM parameterPM { get; set; }

        public string ParameterName { get; set; }
        public string ParameterUoM { get; set; }

        public string ParameterDefaultValue { get; set; }

        public string ParameterMinValue { get; set; }

        public string ParameterMaxValue { get; set; }

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
            if (results.Count()==0)
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
