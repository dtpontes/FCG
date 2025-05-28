using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace FCG.Domain.Entities
{
    public class Game : Base
    {       
        public string Name { get; set; } = null!;        
        public string Description { get; set; } = null!;
        public DateTime DateRelease { get; set; } 
        public DateTime DateUpdate { get; set; } = DateTime.Now!;
    }
}
