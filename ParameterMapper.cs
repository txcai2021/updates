using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SIMTech.APS.Operation.API.Mappers
{
    using SIMTech.APS.Operation.API.Models;
    using SIMTech.APS.Operation.API.PresentationModels;
    /// <summary>
    /// Maps  PMs (Presentation Models) to  BOs (Business Objects) and vice versa.
    /// </summary>
    public static class ParameterMapper
    {
        //private static IRepository _processRepository;
        //private static IRepository _resourceRepository;

        /// <summary>
        /// Transforms list of parameter BOs list of parameter PMs.
        /// </summary>
        /// <param name="parameters">List of parameter BOs.</param>
        /// <returns>List of parameter PMs.</returns>
        public static IList<ParameterPM> ToPresentationModels(IEnumerable<Parameter> parameters)
        {
            if (parameters == null) return null;
            return parameters.Select(u => ToPresentationModel(u)).ToList();
        }

        /// <summary>
        /// Transforms parameter BO to parameter PM.
        /// </summary>
        /// <param name="parameter">parameter BO.</param>
        /// <returns>parameter PM.</returns>
        public static ParameterPM ToPresentationModel(Parameter parameter)
        {
            if (parameter == null) return null;

            //_processRepository = IoCWorker.Resolve<IRepository>("Process");
            //_resourceRepository = IoCWorker.Resolve<IRepository>("Resource");

            //ObservableCollection<OperationResource> roList = new ObservableCollection<OperationResource>(_processRepository.Find<OperationResource>(x => x.ResourceID == parameter.EquipmentId));

            //List<int?> oIdList = new List<int?>();
            //foreach (OperationResource or in roList)
            //    oIdList.Add(or.OperationID);

            //ObservableCollection<Operation> oList = new ObservableCollection<Operation>(_processRepository.Find<Operation>(x => oIdList.Contains(x.OperationId)));

            //List<string> oNamesList = new List<string>();
            //foreach (Operation o in oList)
            //    oNamesList.Add(o.OperationName);

            return new ParameterPM
            {
                Id = parameter.Id,
                Name = parameter.ParameterName,

                Type = parameter.Type,
                DataType = parameter.DataType,
                DataTypeName = parameter.DataType.Equals("1") ? "Number" : (parameter.DataType.Equals("2") ? "Text" : (parameter.DataType.Equals("3") ? "Boolean" : "Date")),
                UoM = parameter.Uom,

                DefaultValue = parameter.DefaultValue,
                MinValue = parameter.MinValue,
                MaxValue = parameter.MaxValue,

                CreatedDate = parameter.CreatedOn,
            };
        }

        /// <summary>
        /// Transforms list of parameter PMs list of parameter BOs.
        /// </summary>
        /// <param name="parameterPMs">List of parameter PMs.</param>
        /// <returns>List of parameter BOs.</returns>
        public static IList<Parameter> FromPresentationModels(IEnumerable<ParameterPM> parameterPMs)
        {
            if (parameterPMs == null) return null;
            return parameterPMs.Select(u => FromPresentationModel(u)).ToList();
        }

        /// <summary>
        /// Transforms parameter PM to parameter BO.
        /// </summary>
        /// <param name="parameterPM">parameter PM.</param>
        /// <returns>parameter BO.</returns>
        public static Parameter FromPresentationModel(ParameterPM parameterPM)
        {
            if (parameterPM == null) return null;

            return new Parameter
            {
                Id = parameterPM.Id,
                ParameterName = parameterPM.Name,

                Type = parameterPM.Type,
                DataType = parameterPM.DataType,
                Uom = parameterPM.UoM,

                DefaultValue = parameterPM.DefaultValue,
                MinValue = parameterPM.MinValue,
                MaxValue = parameterPM.MaxValue,

                CreatedOn = parameterPM.CreatedDate,
                ModifiedOn = parameterPM.ModifiedDate
            };
        }

        
      
    }
}
