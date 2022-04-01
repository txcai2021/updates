using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SIMTech.APS.Scheduling.API.Mappers
{
    using SIMTech.APS.DPS.Engine.Model;
    using SIMTech.APS.Scheduling.API.PresentationModels;
    public static class RuleMapper
    {
        public static IList<RulePM> ToPresentationModels(IEnumerable<Entity> rules)
        {
            if (rules == null) return null;
            return rules.Select(u => ToPresentationModel(u)).ToList();
        }

        public static RulePM ToPresentationModel(Entity rule)
        {
            if (rule == null) return null;

            return new RulePM
            {
                Id = (int) rule.Id,
                Name = rule.Name,
                Description = rule.Description ,

                Category = rule.Category,
                LinkedRuleId =rule.Priority                

            };
        }
    }
}
