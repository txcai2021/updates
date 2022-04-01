using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SIMTech.APS.Scheduling.API.PresentationModels
{
    public class Entity
    {
        public Entity(long id = 0, string name = "", string description = "") 
        { Id = id; Name = name; Description = description; }

        public string Category { get; set; }
        public string Description { get; set; }
        public bool Hidden { get; set; }
        [Key]
        public long Id { get; set; }
        public List<Entity> Members { get; set; }
        public string Name { get; set; }
        public Entity Parent { get; set; }
        public int Priority { get; set; }
        public string SubCategory { get; set; }
        public string Value { get; set; }
    }
}
