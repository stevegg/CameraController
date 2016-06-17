using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebServer.model
{
    public sealed class Camera
    {
        private Guid _id = Guid.NewGuid();

        [PrimaryKey, NotNull]
        public Guid id
        {
            get
            {
                return _id;
            }
            set
            {
                if ( _id != value )
                {
                    _id = value;
                }
            }
        }
        public String name { get; set; }
        public String imageUrl { get; set; }
        public String videoUrl { get; set; }
    }
}
