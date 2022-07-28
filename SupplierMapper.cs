using System.Collections.Generic;
using System.Linq;


namespace SIMTech.APS.Customer.API.Mappers
{
    using SIMTech.APS.Customer.API.Models;
    using SIMTech.APS.Customer.API.PresentationModels;

    public class SupplierMapper
    {
        public static IEnumerable<SupplierPM> ToPresentationModels(IEnumerable<Customer> suppliers)
        {
            if (suppliers == null) return null;
            return suppliers.Select(ToPresentationModel);
        }

        public static SupplierPM ToPresentationModel(Customer supplier)
        {
            if (supplier == null) return null;

            return new SupplierPM
            {
                BillingAddress = string.IsNullOrEmpty(supplier.BillingAddress) ? string.Empty : supplier.BillingAddress,
                Category = string .IsNullOrEmpty (supplier .Category )? string.Empty  :supplier .Category ,
                Address = string.IsNullOrEmpty(supplier.Address) ? string.Empty : supplier.Address,
                Code = string.IsNullOrEmpty(supplier.CustomerName) ? string.Empty : supplier.CustomerName,
                ContactPerson = string.IsNullOrEmpty(supplier.ContactPerson) ? string.Empty : supplier.ContactPerson,
                Department = string.IsNullOrEmpty(supplier.String2) ? string.Empty : supplier.String2,
                Description = string.IsNullOrEmpty(supplier.MaxString1) ? string.Empty : supplier.MaxString1,
                Email = string.IsNullOrEmpty(supplier.Email) ? string.Empty : supplier.Email,
                Fax = string.IsNullOrEmpty(supplier.String3) ? string.Empty : supplier.String3,
                Id = supplier.Id,
                Name = string.IsNullOrEmpty(supplier.CompanyName) ? string.Empty : supplier.CompanyName,
                Phone = string.IsNullOrEmpty(supplier.Phone) ? string.Empty : supplier.Phone,
                Site = string.IsNullOrEmpty(supplier.String1) ? string.Empty : supplier.String1,
                Currency = string.IsNullOrEmpty(supplier.String4) ? string.Empty : supplier.String4,
                CreditTerm = string.IsNullOrEmpty(supplier.String5) ? string.Empty : supplier.String5,
                PictureId = supplier.PictureId,                
            };
        }

        public static IEnumerable<Customer> FromPresentationModels(IEnumerable<SupplierPM> supplierPMs)
        {
            if (supplierPMs == null) return null;
            return supplierPMs.Select(FromPresentationModel);
        }

        public static Customer FromPresentationModel(SupplierPM supplierPM)
        {
            if (supplierPM == null) return null;

            return new Customer
            {
                //comment
                BillingAddress = supplierPM.BillingAddress ,
                Category = supplierPM.Category ,
                Address = supplierPM.Address,
                CompanyName = supplierPM.Name,
                ContactPerson = supplierPM.ContactPerson,
                Id = supplierPM.Id,
                CustomerName = supplierPM.Code,               
                Email = supplierPM.Email,
                MaxString1 = supplierPM.Description,
                Phone = supplierPM.Phone,
                String1 = supplierPM.Site,
                String2 = supplierPM.Department,
                String3 = supplierPM.Fax,
                String4 = supplierPM.Currency,
                String5 = supplierPM.CreditTerm,
                PictureId = supplierPM.PictureId ,
            };
        }

        public static void UpdatePresentationModel(SupplierPM supplierPM, Customer customer)
        {
            if (supplierPM == null || customer == null) return;
            //comment
            supplierPM.BillingAddress = string.IsNullOrEmpty(customer.BillingAddress) ? string.Empty : customer.BillingAddress;
            supplierPM.Category = string.IsNullOrEmpty(customer.Category ) ? string.Empty : customer.Category ;
            supplierPM.Address = string.IsNullOrEmpty(customer.Address) ? string.Empty : customer.Address;
            supplierPM.Code = string.IsNullOrEmpty(customer.CustomerName) ? string.Empty : customer.CustomerName;
            supplierPM.Name = string.IsNullOrEmpty(customer.CompanyName) ? string.Empty : customer.CompanyName;
            supplierPM.ContactPerson = string.IsNullOrEmpty(customer.ContactPerson) ? string.Empty : customer.ContactPerson;
            supplierPM.Department = string.IsNullOrEmpty(customer.String2) ? string.Empty : customer.String2;
            supplierPM.Description = string.IsNullOrEmpty(customer.MaxString1) ? string.Empty : customer.MaxString1;
            supplierPM.Email = string.IsNullOrEmpty(customer.Email) ? string.Empty : customer.Email;
            supplierPM.Fax = string.IsNullOrEmpty(customer.String3) ? string.Empty : customer.String3;
            supplierPM.Id = customer.Id;        
            supplierPM.Phone = string.IsNullOrEmpty(customer.Phone) ? string.Empty : customer.Phone;
            supplierPM.Site = string.IsNullOrEmpty(customer.String1) ? string.Empty : customer.String1;
            supplierPM.Currency = string.IsNullOrEmpty(customer.String4) ? string.Empty : customer.String4;
            supplierPM.CreditTerm = string.IsNullOrEmpty(customer.String5) ? string.Empty : customer.String5;
            //Signature ID is Priorty Column
            supplierPM.PictureId  = customer.PictureId;
        }
    }
}
