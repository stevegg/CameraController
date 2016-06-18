using SQLite.Net.Attributes;
using System;

namespace CameraController.model
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
        public String hostname { get; set; }
        public int port { get; set; }
        public String imageUrl { get; set; }
        public String videoUrl { get; set; }
        public String username { get; set; }
        public String password { get; set; }
    }
}
